using Ryujinx.HLE.HOS.Applets;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy;
using System;
using System.Collections.Concurrent;

namespace Ryujinx.HLE.HOS.SystemState
{
    class AppletStateMgr
    {
        private ConcurrentQueue<AppletMessage> Messages;

        public bool ForcedSuspend { get; private set; }
        public FocusState AcknowledgedFocusState { get; private set; } = FocusState.Background;
        public FocusState RequestedFocusState { get; private set; } = FocusState.Background;

        public bool ResumeNotificationEnabled { get; set; } = false;
        public SuspendMode SuspendMode { get; set; } = SuspendMode.NoOverride;
        public ActivityState ActivityState { get; set; } = ActivityState.ForegroundVisible;

        public KEvent MessageEvent { get; }
        public KEvent OperationModeChangedEvent { get; }
        public KEvent LaunchableEvent { get; }

        public IdDictionary AppletResourceUserIds { get; }

        public IdDictionary IndirectLayerHandles { get; }

        internal bool IsApplication { get; }
        private bool _focusStateChangedNotificationEnabled = true;
        private bool _operationModeChangedNotificationEnabled = true;
        private bool _performanceModeChangedNotificationEnabled = true;
        private bool _hasRequestedExit = false;
        private bool _hasAcknowledgedExit = false;
        private bool _requestedRequestToDisplayState = false;
        private bool _acknowledgedRequestToDisplayState = false;
        private bool _hasRequestedRequestToPrepareSleep = false;
        private bool _hasAcknowledgedRequestToPrepareSleep = false;
        private bool _hasOperationModeChanged = false;
        private bool _hasPerformanceModeChanged = false;
        private bool _hasResume = false;
        private bool _hasFocusStateChanged = false;
        private bool _hasAlbumRecordingSaved = false;
        private bool _hasAlbumScreenShotTaken = false;
        private bool _hasAutoPowerDown = false;
        private bool _hasSleepRequiredByLowBattery = false;
        private bool _hasSleepRequiredByHighTemperature = false;
        private bool _hasSdCardRemoved = false;
        private bool _eventSignaled = false;
        private FocusHandlingMode _focusHandlingMode = FocusHandlingMode.NoSuspend;

        public bool HasRequestedExit => _hasRequestedExit;

        public bool FocusStateChangedNotificationEnabled
        {
            get => _focusStateChangedNotificationEnabled;
            set
            {
                _focusStateChangedNotificationEnabled = value;
                // SignalEventIfNeeded();
            }
        }

        public bool OperationModeChangedNotificationEnabled
        {
            get => _operationModeChangedNotificationEnabled;
            set
            {
                _operationModeChangedNotificationEnabled = value;
                SignalEventIfNeeded();
            }
        }

        public bool PerformanceModeChangedNotificationEnabled
        {
            get => _performanceModeChangedNotificationEnabled;
            set
            {
                _performanceModeChangedNotificationEnabled = value;
                SignalEventIfNeeded();
            }
        }

        public AppletStateMgr(Horizon system, bool isApplication)
        {
            IsApplication = isApplication;
            Messages = new ConcurrentQueue<AppletMessage>();
            MessageEvent = new KEvent(system.KernelContext);
            OperationModeChangedEvent = new KEvent(system.KernelContext);
            LaunchableEvent = new KEvent(system.KernelContext);

            AppletResourceUserIds = new IdDictionary();
            IndirectLayerHandles = new IdDictionary();
        }

        public void SetFocusState(FocusState state)
        {
            if (RequestedFocusState != state)
            {
                RequestedFocusState = state;
                _hasFocusStateChanged = true;
            }

            SignalEventIfNeeded();
        }

        public FocusState GetAndClearFocusState()
        {
            AcknowledgedFocusState = RequestedFocusState;
            return AcknowledgedFocusState;
        }

        public void PushUnorderedMessage(AppletMessage message)
        {
            Messages.Enqueue(message);

            SignalEventIfNeeded();
        }

        public bool PopMessage(out AppletMessage message)
        {
            message = GetNextMessage();
            SignalEventIfNeeded();
            return message != AppletMessage.None;
        }

        private AppletMessage GetNextMessage()
        {
            if (_hasResume)
            {
                _hasResume = false;
                return AppletMessage.Resume;
            }

            if (_hasRequestedExit != _hasAcknowledgedExit)
            {
                _hasAcknowledgedExit = _hasRequestedExit;
                return AppletMessage.Exit;
            }

            if (_focusStateChangedNotificationEnabled)
            {
                if (IsApplication)
                {
                    if (_hasFocusStateChanged)
                    {
                        _hasFocusStateChanged = false;
                        return AppletMessage.FocusStateChanged;
                    }
                }
                else
                {
                    if (RequestedFocusState != AcknowledgedFocusState)
                    {
                        AcknowledgedFocusState = RequestedFocusState;

                        switch (RequestedFocusState)
                        {
                            case FocusState.InFocus:
                                return AppletMessage.ChangeIntoForeground;
                            case FocusState.OutOfFocus:
                                return AppletMessage.ChangeIntoBackground;
                        }
                    }
                }
            }

            if (_hasRequestedRequestToPrepareSleep != _hasAcknowledgedRequestToPrepareSleep)
            {
                _hasAcknowledgedRequestToPrepareSleep = true;
                return AppletMessage.RequestToPrepareSleep;
            }

            if (_requestedRequestToDisplayState != _acknowledgedRequestToDisplayState)
            {
                _acknowledgedRequestToDisplayState = _requestedRequestToDisplayState;
                return AppletMessage.RequestToDisplay;
            }

            if (_hasOperationModeChanged)
            {
                _hasOperationModeChanged = false;
                return AppletMessage.OperationModeChanged;
            }

            if (_hasPerformanceModeChanged)
            {
                _hasPerformanceModeChanged = false;
                return AppletMessage.PerformanceModeChanged;
            }

            if (_hasSdCardRemoved)
            {
                _hasSdCardRemoved = false;
                return AppletMessage.SdCardRemoved;
            }

            if (_hasSleepRequiredByHighTemperature)
            {
                _hasSleepRequiredByHighTemperature = false;
                return AppletMessage.SleepRequiredByHighTemperature;
            }

            if (_hasSleepRequiredByLowBattery)
            {
                _hasSleepRequiredByLowBattery = false;
                return AppletMessage.SleepRequiredByLowBattery;
            }

            if (_hasAutoPowerDown)
            {
                _hasAutoPowerDown = false;
                return AppletMessage.AutoPowerDown;
            }

            if (_hasAlbumScreenShotTaken)
            {
                _hasAlbumScreenShotTaken = false;
                return AppletMessage.AlbumScreenShotTaken;
            }

            if (_hasAlbumRecordingSaved)
            {
                _hasAlbumRecordingSaved = false;
                return AppletMessage.AlbumRecordingSaved;
            }

            if (Messages.TryDequeue(out AppletMessage message))
            {
                return message;
            }

            return AppletMessage.None;
        }

        internal void SignalEventIfNeeded()
        {
            var shouldSignal = ShouldSignalEvent();

            if (_eventSignaled != shouldSignal)
            {
                if (_eventSignaled)
                {
                    MessageEvent.ReadableEvent.Clear();
                    _eventSignaled = false;
                }
                else
                {
                    MessageEvent.ReadableEvent.Signal();
                    _eventSignaled = true;
                }
            }
        }

        private bool ShouldSignalEvent()
        {
            bool focusStateChanged = false;
            if (_focusStateChangedNotificationEnabled)
            {
                if (IsApplication)
                {
                    if (_hasFocusStateChanged)
                    {
                        focusStateChanged = true;
                    }
                }
                else
                {
                    if (RequestedFocusState != AcknowledgedFocusState)
                    {
                        focusStateChanged = true;
                    }
                }
            }

            return !Messages.IsEmpty
                || focusStateChanged
                || _hasResume
                || _hasRequestedExit != _hasAcknowledgedExit
                || _hasRequestedRequestToPrepareSleep != _hasAcknowledgedRequestToPrepareSleep
                || _hasOperationModeChanged
                || _hasPerformanceModeChanged
                || _hasSdCardRemoved
                || _hasSleepRequiredByHighTemperature
                || _hasSleepRequiredByLowBattery
                || _hasAutoPowerDown
                || _requestedRequestToDisplayState != _acknowledgedRequestToDisplayState
                || _hasAlbumScreenShotTaken
                || _hasAlbumRecordingSaved;
        }

        public void OnOperationAndPerformanceModeChanged()
        {
            if (_operationModeChangedNotificationEnabled)
            {
                _hasOperationModeChanged = true;
            }

            if (_performanceModeChangedNotificationEnabled)
            {
                _hasPerformanceModeChanged = true;
            }

            OperationModeChangedEvent.ReadableEvent.Signal();
            SignalEventIfNeeded();
        }

        public void OnExitRequested()
        {
            _hasRequestedExit = true;
            SignalEventIfNeeded();
        }

        public void SetFocusHandlingMode(bool suspend)
        {
            switch (_focusHandlingMode)
            {
                case FocusHandlingMode.AlwaysSuspend:
                case FocusHandlingMode.SuspendHomeSleep:
                    if (!suspend)
                    {
                        // Disallow suspension
                        _focusHandlingMode = FocusHandlingMode.NoSuspend;
                    }
                    break;
                case FocusHandlingMode.NoSuspend:
                    if (suspend)
                    {
                        // Allow suspension temporarily.
                        _focusHandlingMode = FocusHandlingMode.SuspendHomeSleep;
                    }
                    break;
            }

            // SignalEventIfNeeded();
        }

        public void RequestResumeNotification()
        {
            // NOTE: this appears to be a bug in am.
            // If an applet makes a concurrent request to receive resume notifications
            // while it is being suspended, the first resume notification will be lost.
            // This is not the case with other notification types.
            if (ResumeNotificationEnabled)
            {
                _hasResume = true;
            }
        }

        public void SetOutOfFocusSuspendingEnabled(bool enabled)
        {
            switch (_focusHandlingMode)
            {
                case FocusHandlingMode.AlwaysSuspend:
                    if (!enabled)
                    {
                        // Allow suspension temporarily.
                        _focusHandlingMode = FocusHandlingMode.SuspendHomeSleep;
                    }
                    break;
                case FocusHandlingMode.SuspendHomeSleep:
                case FocusHandlingMode.NoSuspend:
                    if (enabled)
                    {
                        // Allow suspension
                        _focusHandlingMode = FocusHandlingMode.AlwaysSuspend;
                    }
                    break;
            }

            SignalEventIfNeeded();
        }

        public void RemoveForceResumeIfPossible()
        {
            // If resume is not forced, we have nothing to do.
            if (SuspendMode != SuspendMode.ForceResume)
            {
                return;
            }

            // Check activity state.
            // If we are already resumed, we can remove the forced state.
            switch (ActivityState)
            {
                case ActivityState.ForegroundVisible:
                case ActivityState.ForegroundObscured:
                    SuspendMode = SuspendMode.NoOverride;
                    return;
            }

            // Check focus handling mode.
            switch (_focusHandlingMode)
            {
                case FocusHandlingMode.AlwaysSuspend:
                case FocusHandlingMode.SuspendHomeSleep:
                    // If the applet allows suspension, we can remove the forced state.
                    SuspendMode = SuspendMode.NoOverride;
                    break;
                case FocusHandlingMode.NoSuspend:
                    // If the applet is not an application, we can remove the forced state.
                    // Only applications can be forced to resume.
                    if (!IsApplication)
                    {
                        SuspendMode = SuspendMode.NoOverride;
                    }
                    break;
            }
        }

        public bool IsRunnable()
        {
            // If suspend is forced, return that.
            if (ForcedSuspend)
            {
                return false;
            }

            // Check suspend mode override.
            switch (SuspendMode)
            {
                case SuspendMode.NoOverride:
                    // Continue processing.
                    break;

                case SuspendMode.ForceResume:
                    // The applet is runnable during forced resumption when its exit is requested.
                    return _hasRequestedExit;

                case SuspendMode.ForceSuspend:
                    // The applet is never runnable during forced suspension.
                    return false;
            }

            // Always run if exit is requested.
            if (_hasRequestedExit)
            {
                return true;
            }

            if (ActivityState == ActivityState.ForegroundVisible)
            {
                // The applet is runnable now.
                return true;
            }

            if (ActivityState == ActivityState.ForegroundObscured)
            {
                switch (_focusHandlingMode)
                {
                    case FocusHandlingMode.AlwaysSuspend:
                        // The applet is not runnable while running the applet.
                        return false;

                    case FocusHandlingMode.SuspendHomeSleep:
                        // The applet is runnable while running the applet.
                        return true;

                    case FocusHandlingMode.NoSuspend:
                        // The applet is always runnable.
                        return true;
                }
            }

            // The activity is a suspended one.
            // The applet should be suspended unless it has disabled suspension.
            return _focusHandlingMode == FocusHandlingMode.NoSuspend;
        }

        public FocusState GetFocusStateWhileForegroundObscured()
        {
            switch (_focusHandlingMode)
            {
                case FocusHandlingMode.AlwaysSuspend:
                    // The applet never learns it has lost focus.
                    return FocusState.InFocus;

                case FocusHandlingMode.SuspendHomeSleep:
                    // The applet learns it has lost focus when launching a child applet.
                    return FocusState.OutOfFocus;

                case FocusHandlingMode.NoSuspend:
                    // The applet always learns it has lost focus.
                    return FocusState.OutOfFocus;

                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public FocusState GetFocusStateWhileBackground(bool isObscured)
        {
            switch (_focusHandlingMode)
            {
                case FocusHandlingMode.AlwaysSuspend:
                    // The applet never learns it has lost focus.
                    return FocusState.InFocus;

                case FocusHandlingMode.SuspendHomeSleep:
                    // The applet learns it has lost focus when launching a child applet.
                    return isObscured ? FocusState.OutOfFocus : FocusState.InFocus;

                case FocusHandlingMode.NoSuspend:
                    // The applet always learns it has lost focus.
                    return IsApplication ? FocusState.Background : FocusState.OutOfFocus;

                default:
                    throw new IndexOutOfRangeException();
            }
        }

        public bool UpdateRequestedFocusState()
        {
            FocusState newState;

            if (SuspendMode == SuspendMode.NoOverride)
            {
                // With no forced suspend or resume, we take the focus state designated
                // by the combination of the activity flag and the focus handling mode.
                switch (ActivityState)
                {
                    case ActivityState.ForegroundVisible:
                        newState = FocusState.InFocus;
                        break;

                    case ActivityState.ForegroundObscured:
                        newState = GetFocusStateWhileForegroundObscured();
                        break;

                    case ActivityState.BackgroundVisible:
                        newState = GetFocusStateWhileBackground(false);
                        break;

                    case ActivityState.BackgroundObscured:
                        newState = GetFocusStateWhileBackground(true);
                        break;

                    default:
                        throw new IndexOutOfRangeException();
                }
            }
            else
            {
                // With forced suspend or resume, the applet is guaranteed to be background.
                newState = GetFocusStateWhileBackground(false);
            }

            if (newState != RequestedFocusState)
            {
                // Mark the focus state as ready for update.
                RequestedFocusState = newState;
                _hasFocusStateChanged = true;

                // We changed the focus state.
                return true;
            }

            // We didn't change the focus state.
            return false;
        }
    }
}
