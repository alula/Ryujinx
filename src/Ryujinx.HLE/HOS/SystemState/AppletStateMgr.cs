using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy;
using System.Collections.Concurrent;

namespace Ryujinx.HLE.HOS.SystemState
{
    class AppletStateMgr
    {
        public ConcurrentQueue<AppletMessage> Messages { get; }

        public FocusState AcknowledgedFocusState { get; private set; }
        public FocusState RequestedFocusState { get; private set; }

        public KEvent MessageEvent { get; }

        public IdDictionary AppletResourceUserIds { get; }

        public IdDictionary IndirectLayerHandles { get; }

        private bool _isApplication;
        private bool _focusStateChangedNotificationEnabled = true;
        private bool _operationModeChangedNotificationEnabled = true;
        private bool _performanceModeChangedNotificationEnabled = true;
        private bool _resumeNotificationEnabled;
        private bool _requestedRequestToDisplayState;
        private bool _acknowledgedRequestToDisplayState;
        private bool _hasResume;
        private bool _hasFocusStateChanged = true;
        private bool _hasAlbumRecordingSaved;
        private bool _hasAlbumScreenShotTaken;
        private bool _hasAutoPowerDown;
        private bool _hasSleepRequiredByLowBattery;
        private bool _hasSleepRequiredByHighTemperature;
        private bool _hasSdCardRemoved;
        private bool _hasPerformanceModeChanged;
        private bool _hasOperationModeChanged;
        private bool _hasRequestedRequestToPrepareSleep;
        private bool _hasAcknowledgedRequestToPrepareSleep;
        private bool _hasRequestedExit;
        private bool _hasAcknowledgedExit;
        private bool _appletMessageAvailable;

        public AppletStateMgr(Horizon system)
        {
            Messages = new ConcurrentQueue<AppletMessage>();
            MessageEvent = new KEvent(system.KernelContext);

            AppletResourceUserIds = new IdDictionary();
            IndirectLayerHandles = new IdDictionary();
        }

        public void SetFocus(bool isFocused)
        {
            AcknowledgedFocusState = isFocused ? FocusState.InFocus : FocusState.OutOfFocus;

            Messages.Enqueue(AppletMessage.FocusStateChanged);

            if (isFocused)
            {
                Messages.Enqueue(AppletMessage.ChangeIntoForeground);
            }

            SignalEventIfNeeded();
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
                if (!_isApplication)
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
                else if (_hasFocusStateChanged)
                {
                    _hasFocusStateChanged = false;
                    return AppletMessage.FocusStateChanged;
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

        private void SignalEventIfNeeded()
        {
            var available = _appletMessageAvailable;

            if (available != ShouldSignalEvent())
            {
                if (!available)
                {
                    MessageEvent.ReadableEvent.Signal();
                    _appletMessageAvailable = true;
                }
                else
                {
                    MessageEvent.ReadableEvent.Clear();
                    _appletMessageAvailable = false;
                }
            }
        }

        private bool ShouldSignalEvent()
        {
            if (_focusStateChangedNotificationEnabled)
            {
                if (!_isApplication)
                {
                    if (RequestedFocusState != AcknowledgedFocusState)
                    {
                        return true;
                    }
                }
                else if (_hasFocusStateChanged)
                {
                    return true;
                }
            }

            return !Messages.IsEmpty
                || _hasResume
                || _hasRequestedExit != _hasAcknowledgedExit
                || _hasRequestedRequestToPrepareSleep != _hasAcknowledgedRequestToPrepareSleep
                || _hasOperationModeChanged
                || _hasPerformanceModeChanged
                || _hasSleepRequiredByLowBattery
                || _hasSleepRequiredByHighTemperature
                || _hasAutoPowerDown
                || _hasAlbumScreenShotTaken
                || _hasAlbumRecordingSaved
                || _hasSdCardRemoved
                || _requestedRequestToDisplayState != _acknowledgedRequestToDisplayState;
        }
    }
}
