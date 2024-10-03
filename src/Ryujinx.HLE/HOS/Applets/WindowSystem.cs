using Ryujinx.HLE.HOS.Applets.Browser;
using Ryujinx.HLE.HOS.Applets.Error;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Services.Am;
using Ryujinx.HLE.HOS.Services.Am.AppletAE;
using Ryujinx.HLE.HOS.SystemState;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Applets
{
    public class WindowSystem
    {
        private Horizon _system;
        private readonly object _lock = new();
        Dictionary<ulong, RealApplet> _applets = new();
        RealApplet _homeMenu = null;
        RealApplet _application = null;
        RealApplet _foregroundRequestedApplet = null;
        EventObserver _eventObserver = null;

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
            lock (_lock)
            {
                PruneTerminatedAppletsLocked();

                if (LockHomeMenuIntoForegroundLocked())
                {
                    return;
                }

                // UpdateAppletStateLocked(_home_menu, _system.ForegroundRequestedApplet == _home_menu);
                // UpdateAppletStateLocked(_application, _system.ForegroundRequestedApplet == _application);
            }
        }

        internal void TrackApplet(RealApplet applet, bool isApplication)
        {
            lock (_lock)
            {
                if (applet.AppletId == AppletId.QLaunch)
                {
                    _homeMenu = applet;
                }
                else if (isApplication)
                {
                    _application = applet;
                }

                _applets.Add(applet.AppletResourceUserId, applet);
            }
        }

        internal RealApplet GetByAruId(ulong aruid)
        {
            lock (_lock)
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
            lock (_lock)
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

        public void RequestHomeMenuToGetForeground()
        {
            lock (_lock)
            {
                _foregroundRequestedApplet = _homeMenu;
            }
        }

        private void PruneTerminatedAppletsLocked()
        {
            foreach (var (aruid, applet) in _applets)
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
                    _foregroundRequestedApplet = _application;
                }

                if (applet == _application)
                {
                    _application = null;
                    _foregroundRequestedApplet = _homeMenu;

                    if (_homeMenu != null)
                    {
                        // TODO: Push AppletMessage.ApplicationExited
                    }
                }
            }
        }

        private bool LockHomeMenuIntoForegroundLocked()
        {
            return false;
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
        }
    }
}
