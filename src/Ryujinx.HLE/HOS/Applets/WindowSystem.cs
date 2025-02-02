using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Services.Am.AppletAE;
using Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.HLE.HOS.Applets
{
    public enum ActivityState
    {
        ForegroundVisible = 0,
        ForegroundObscured = 1,
        BackgroundVisible = 2,
        BackgroundObscured = 3,
    }

    public enum SuspendMode
    {
        NoOverride = 0,
        ForceResume = 1,
        ForceSuspend = 2,
    }

    public class WindowSystem
    {
        private Horizon _system;
        private readonly object _lock = new();
        EventObserver _eventObserver = null;

        // Foreground roots
        RealApplet _homeMenu = null;
        RealApplet _application = null;

        // Home menu state
        bool _homeMenuForegroundLocked = false;
        RealApplet _foregroundRequestedApplet = null;


        // aruid -> applet map
        Dictionary<ulong, RealApplet> _applets = new();
        List<RealApplet> _rootApplets = new();

        public WindowSystem(Horizon system)
        {
            _system = system;
        }

        void Dispose()
        {
            // SetWindowSystem(null);
        }

        internal void SetEventObserver(EventObserver eventObserver)
        {
            _eventObserver = eventObserver;
            // SetWindowSystem(this);
        }

        internal void Update()
        {
            // lock (_lock)
            {
                PruneTerminatedAppletsLocked();

                if (LockHomeMenuIntoForegroundLocked())
                {
                    return;
                }

                if (_foregroundRequestedApplet == null && _rootApplets.Count != 0)
                {
                    _foregroundRequestedApplet = _rootApplets.Last();
                }

                foreach (var applet in _rootApplets)
                {
                    UpdateAppletStateLocked(applet, _foregroundRequestedApplet == applet);
                }
            }
        }

        internal RealApplet TrackProcess(ulong pid, ulong callerPid, bool isApplication)
        {
            if (_applets.TryGetValue(pid, out var applet))
            {
                Logger.Info?.Print(LogClass.ServiceAm, $"TrackProcess() called on existing applet {pid} - caller {callerPid}");
                return applet;
            }

            Logger.Info?.Print(LogClass.ServiceAm, $"Tracking process {pid} as {(isApplication ? "application" : "applet")} - caller {callerPid}");
            if (_system.KernelContext.Processes.TryGetValue(pid, out var _process))
            {
                applet = new RealApplet(pid, isApplication, _system);

                if (callerPid == 0)
                {
                    _rootApplets.Add(applet);
                }
                else
                {
                    var callerApplet = _applets[callerPid];
                    applet.CallerApplet = callerApplet;
                    callerApplet.RegisterChild(applet);
                }

                TrackApplet(applet, isApplication);
                return applet;
            }

            return null;
        }

        internal void TrackApplet(RealApplet applet, bool isApplication)
        {
            if (_applets.ContainsKey(applet.AppletResourceUserId))
            {
                return;
            }

            // lock (_lock)
            {
                if (applet.AppletId == AppletId.QLaunch)
                {
                    _homeMenu = applet;
                }
                else if (isApplication)
                {
                    _application = applet;
                }

                _applets[applet.AppletResourceUserId] = applet;
                _eventObserver.TrackAppletProcess(applet);

                if (_applets.Count == 1)
                {
                    SetupFirstApplet(applet);
                }

                // _foregroundRequestedApplet = applet;
                // applet.AppletState.SetFocusState(FocusState.InFocus);
            }

            _eventObserver.RequestUpdate();
        }

        private void SetupFirstApplet(RealApplet applet)
        {
            // applet.AppletState.SetFocusState(FocusState.InFocus);

            if (applet.AppletId == AppletId.QLaunch)
            {
                applet.AppletState.SetFocusHandlingMode(false);
                applet.AppletState.SetOutOfFocusSuspendingEnabled(false);
                RequestHomeMenuToGetForeground();
            }
            else
            {
                RequestApplicationToGetForeground();
            }

            applet.UpdateSuspensionStateLocked(true);
        }

        internal RealApplet GetByAruId(ulong aruid)
        {
            // lock (_lock)
            {
                if (_applets.TryGetValue(aruid, out RealApplet applet))
                {
                    return applet;
                }

                return null;
            }
        }

        internal RealApplet GetMainApplet()
        {
            // lock (_lock)
            {
                if (_application != null)
                {
                    if (_applets.TryGetValue(_application.AppletResourceUserId, out RealApplet applet))
                    {
                        return applet;
                    }
                }

                return null;
            }
        }

        internal void RequestHomeMenuToGetForeground()
        {
            // lock (_lock)
            {
                _foregroundRequestedApplet = _homeMenu;
            }

            _eventObserver.RequestUpdate();
        }

        internal void RequestApplicationToGetForeground()
        {
            // lock (_lock)
            {
                _foregroundRequestedApplet = _application;
            }

            _eventObserver.RequestUpdate();
        }

        internal void RequestLockHomeMenuIntoForeground()
        {
            // lock (_lock)
            {
                _homeMenuForegroundLocked = true;
            }

            _eventObserver.RequestUpdate();
        }

        internal void RequestUnlockHomeMenuFromForeground()
        {
            // lock (_lock)
            {
                _homeMenuForegroundLocked = false;
            }

            _eventObserver.RequestUpdate();
        }

        internal void RequestAppletVisibilityState(RealApplet applet, bool isVisible)
        {
            lock (applet.Lock)
            {
                applet.WindowVisible = isVisible;
            }

            _eventObserver.RequestUpdate();
        }

        internal void OnOperationModeChanged()
        {
            // lock (_lock)
            {
                foreach (var (aruid, applet) in _applets)
                {
                    lock (applet.Lock)
                    {
                        applet.AppletState.OnOperationAndPerformanceModeChanged();
                    }
                }
            }
        }

        internal void OnExitRequested()
        {
            // lock (_lock)
            {
                foreach (var (aruid, applet) in _applets)
                {
                    lock (applet.Lock)
                    {
                        applet.AppletState.OnExitRequested();
                    }
                }
            }
        }

        internal void OnSystemButtonPress(SystemButtonType type)
        {
            // lock (_lock)
            {
                switch (type)
                {
                    case SystemButtonType.PerformHomeButtonShortPressing:
                    case SystemButtonType.PerformHomeButtonLongPressing:
                        HandleHomeButtonPress(type == SystemButtonType.PerformHomeButtonLongPressing);
                        break;
                }
            }
        }

        private void HandleHomeButtonPress(bool longPress)
        {
            if (_homeMenu == null)
            {
                return;
            }

            lock (_homeMenu.Lock)
            {
                _homeMenu.AppletState.PushUnorderedMessage(longPress ? AppletMessage.DetectLongPressingHomeButton : AppletMessage.DetectShortPressingHomeButton);
            }
        }

        private void PruneTerminatedAppletsLocked()
        {
            foreach (var (aruid, applet) in _applets)
            {
                lock (applet.Lock)
                {
                    if (applet.ProcessHandle.State != ProcessState.Exited)
                    {
                        continue;
                    }

                    if (applet.ChildApplets.Count != 0)
                    {
                        TerminateChildAppletsLocked(applet);
                        continue;
                    }

                    if (applet.CallerApplet != null)
                    {
                        applet.CallerApplet.ChildApplets.Remove(applet);
                        applet.CallerApplet = null;
                    }

                    if (applet == _foregroundRequestedApplet)
                    {
                        _foregroundRequestedApplet = null;
                    }

                    if (applet == _homeMenu)
                    {
                        _homeMenu = null;
                        _foregroundRequestedApplet = null;
                    }

                    if (applet == _application)
                    {
                        _application = null;
                        _foregroundRequestedApplet = null;

                        if (_homeMenu != null)
                        {
                            _homeMenu.AppletState.PushUnorderedMessage(AppletMessage.ApplicationExited);
                        }
                    }

                    applet.OnProcessTerminatedLocked();

                    _eventObserver.RequestUpdate();
                    _applets.Remove(aruid);
                    _rootApplets.Remove(applet);
                }
            }
        }

        private bool LockHomeMenuIntoForegroundLocked()
        {
            // If the home menu is not locked into the foreground, then there's nothing to do.
            if (_homeMenu == null || !_homeMenuForegroundLocked)
            {
                _homeMenuForegroundLocked = false;
                return false;
            }

            lock (_homeMenu.Lock)
            {
                TerminateChildAppletsLocked(_homeMenu);

                if (_homeMenu.ChildApplets.Count == 0)
                {
                    _homeMenu.WindowVisible = true;
                    _foregroundRequestedApplet = _homeMenu;
                    return false;
                }
            }

            return true;
        }

        private void TerminateChildAppletsLocked(RealApplet parent)
        {
            foreach (var child in parent.ChildApplets)
            {
                if (child.ProcessHandle.State != ProcessState.Exited)
                {
                    child.ProcessHandle.Terminate();
                    child.TerminateResult = (ResultCode)Services.Am.ResultCode.LibraryAppletTerminated;
                }
            }
        }

        private void UpdateAppletStateLocked(RealApplet applet, bool isForeground)
        {
            if (applet == null)
            {
                return;
            }

            lock (applet.Lock)
            {
                var inheritedForeground = applet.IsProcessRunning && isForeground;
                var visibleState = inheritedForeground ? ActivityState.ForegroundVisible : ActivityState.BackgroundVisible;
                var obscuredState = inheritedForeground ? ActivityState.ForegroundObscured : ActivityState.BackgroundObscured;

                var hasObscuringChildApplets = applet.ChildApplets.Any(child =>
                {
                    lock (child.Lock)
                    {
                        var mode = child.LibraryAppletMode;
                        if (child.IsProcessRunning && child.WindowVisible &&
                            (mode == LibraryAppletMode.AllForeground || mode == LibraryAppletMode.AllForegroundInitiallyHidden))
                        {
                            return true;
                        }
                    }

                    return false;
                });

                // TODO: Update visibility state

                applet.SetInteractibleLocked(isForeground && applet.WindowVisible);

                var isObscured = hasObscuringChildApplets || !applet.WindowVisible;
                var state = applet.AppletState.ActivityState;

                if (isObscured && state != obscuredState)
                {
                    applet.AppletState.ActivityState = obscuredState;
                    applet.UpdateSuspensionStateLocked(true);
                }
                else if (!isObscured && state != visibleState)
                {
                    applet.AppletState.ActivityState = visibleState;
                    applet.UpdateSuspensionStateLocked(true);
                }

                Logger.Info?.Print(LogClass.ServiceAm, $"Updating applet state for {applet.AppletId}: visible={applet.WindowVisible}, foreground={isForeground}, obscured={isObscured}, reqFState={applet.AppletState.RequestedFocusState}, ackFState={applet.AppletState.AcknowledgedFocusState}, runnable={applet.AppletState.IsRunnable()}");


                // Recurse into child applets
                foreach (var child in applet.ChildApplets)
                {
                    UpdateAppletStateLocked(child, isForeground);
                }
            }
        }
    }
}
