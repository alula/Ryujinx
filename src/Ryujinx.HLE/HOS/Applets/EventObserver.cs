using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon;
using Ryujinx.Horizon.Sdk.OsTypes;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.HLE.HOS.Applets
{
    enum UserDataTag : uint
    {
        WakeupEvent,
        AppletProcess,
    }

    public class EventObserver
    {
        private readonly object _lock = new();

        private Horizon _system;
        private WindowSystem _windowSystem;

        // Guest event handle to wake up the event loop
        private SystemEventType _wakeupEvent;
        private MultiWaitHolder _wakeupHolder;
        private KWritableEvent _wakeupEventObj;

        // List of owned process holders
        private readonly LinkedList<ProcessHolder> _processHolders = new();

        // Multi-wait objects for new tasks
        private readonly MultiWait _multiWait;
        private readonly MultiWait _deferredWaitList;

        // Processing thread
        private KThread _thread;
        private readonly CancellationTokenSource _cts = new();
        private bool _initialized;

        public EventObserver(Horizon system, WindowSystem windowSystem)
        {
            _system = system;
            _windowSystem = windowSystem;

            _multiWait = new();
            _deferredWaitList = new();

            _windowSystem.SetEventObserver(this);
        }

        public void Dispose()
        {
            if (!_initialized)
                return;

            // Signal and wait for the thread to finish
            _cts.Cancel();
            // Os.SignalSystemEvent(ref _wakeupEvent);
            _wakeupEventObj?.Signal();
            // _thread.Join();

            _processHolders.Clear();
        }

        private void Initialize()
        {
            if (_initialized)
            {
                return;
            }
            _initialized = true;

            var holderProcess = _system.ViServer.SelfProcess;
            _thread = new KThread(_system.KernelContext);
            _thread.SetName("am:EventObserver");
            _thread.Initialize(
                    0,
                    0,
                    0,
                    1,
                    3,
                    holderProcess,
                    ThreadType.Kernel,
                    ThreadFunc);
            _thread.Owner.HandleTable.GenerateHandle(_thread, out int threadHandle).AbortOnFailure();
            _thread.SetEntryArguments(0, threadHandle);
            _thread.Start();
        }

        internal void TrackAppletProcess(RealApplet applet)
        {
            Initialize();

            if (applet.ProcessHandle == null)
            {
                return;
            }

            _thread.Owner.HandleTable.GenerateHandle(applet.ProcessHandle, out int processHandle).AbortOnFailure();
            ProcessHolder holder = new(applet, applet.ProcessHandle, processHandle);
            holder.UserData = UserDataTag.AppletProcess;

            // lock (_lock)
            {
                _processHolders.AddLast(holder);
                _deferredWaitList.LinkMultiWaitHolder(holder);
            }

            RequestUpdate();
        }

        public void RequestUpdate()
        {
            Initialize();

            // Os.SignalSystemEvent(ref _wakeupEvent);
            // lock (_lock)
            {
                _wakeupEventObj?.Signal();
            }
        }

        public void LinkDeferred()
        {
            // lock (_lock)
            {
                _multiWait.MoveAllFrom(_deferredWaitList);
            }
        }

        private MultiWaitHolder WaitSignaled()
        {
            while (true)
            {
                LinkDeferred();

                if (_cts.Token.IsCancellationRequested)
                {
                    return null;
                }

                var selected = _multiWait.WaitAny();
                // Logger.Warning?.Print(LogClass.ServiceAm, $"*** Selected={selected}");
                if (selected != _wakeupHolder)
                {
                    selected.UnlinkFromMultiWaitHolder();
                }

                return selected;
            }
        }

        private void Process(MultiWaitHolder holder)
        {
            switch (holder.UserData)
            {
                case UserDataTag.WakeupEvent:
                    OnWakeupEvent(holder);
                    break;
                case UserDataTag.AppletProcess:
                    OnProcessEvent((ProcessHolder)holder);
                    break;
            }
        }

        private void OnWakeupEvent(MultiWaitHolder holder)
        {
            // Os.ClearSystemEvent(ref _wakeupEvent);
            Logger.Warning?.Print(LogClass.ServiceAm, "*** Wakeup event");
            // lock (_lock)
            {
                _wakeupEventObj?.Clear();
            }
            _windowSystem.Update();
        }

        private void OnProcessEvent(ProcessHolder holder)
        {
            var applet = holder.Applet;
            var process = holder.ProcessHandle;
            Logger.Warning?.Print(LogClass.ServiceAm, $"*** Applet={applet.AppletId}, Process={process.Pid}, State={process.State}");

            // lock (_lock)
            {
                if (process.State == ProcessState.Exited)
                {
                    _processHolders.Remove(holder);
                }
                else
                {
                    process.ClearIfNotExited();
                    _deferredWaitList.LinkMultiWaitHolder(holder);
                }

                applet.IsProcessRunning = process.IsRunning();
            }
            _windowSystem.Update();
        }

        private void ThreadFunc()
        {
            HorizonStatic.Register(
                default,
                _system.KernelContext.Syscall,
                null,
                _thread.ThreadContext,
                (int)_thread.ThreadContext.GetX(1));

            // lock (_lock)
            {
                Os.CreateSystemEvent(out _wakeupEvent, EventClearMode.ManualClear, true).AbortOnFailure();
                _wakeupEventObj = _thread.Owner.HandleTable.GetObject<KWritableEvent>(Os.GetWritableHandleOfSystemEvent(ref _wakeupEvent));

                _wakeupHolder = new MultiWaitHolderOfInterProcessEvent(_wakeupEvent.InterProcessEvent);
                _wakeupHolder.UserData = UserDataTag.WakeupEvent;
                _multiWait.LinkMultiWaitHolder(_wakeupHolder);
            }

            while (!_cts.Token.IsCancellationRequested)
            {
                var holder = WaitSignaled();
                if (holder == null)
                {
                    break;
                }

                Process(holder);
            }
        }
    }
}
