using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Applets;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy.Types;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class ISelfController : IpcService
    {
        private readonly ulong _pid;
        private readonly RealApplet _applet;

        private int _libraryAppletLaunchableEventHandle;

        private KEvent _accumulatedSuspendedTickChangedEvent;
        private int _accumulatedSuspendedTickChangedEventHandle;

        private readonly object _fatalSectionLock = new();
        private int _fatalSectionCount;

        // TODO: Set this when the game goes in suspension (go back to home menu ect), we currently don't support that so we can keep it set to 0.
        private readonly ulong _accumulatedSuspendedTickValue = 0;

        // TODO: Determine where those fields are used.
#pragma warning disable IDE0052 // Remove unread private member
        private bool _screenShotPermission = false;
        private bool _operationModeChangedNotification = false;
        private bool _performanceModeChangedNotification = false;
        private bool _handlesRequestToDisplay = false;
#pragma warning restore IDE0052
        private bool _autoSleepDisabled = false;
#pragma warning disable IDE0052 // Remove unread private member
        private bool _albumImageTakenNotificationEnabled = false;
        private bool _recordVolumeMuted = false;

        private uint _screenShotImageOrientation = 0;
#pragma warning restore IDE0052
        private uint _idleTimeDetectionExtension = 0;

        public ISelfController(ServiceCtx context, ulong pid)
        {
            _pid = pid;
            _applet = context.Device.System.WindowSystem.GetByAruId(_pid);
        }

        [CommandCmif(0)]
        // Exit()
        public ResultCode Exit(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            _applet.ProcessHandle.Terminate();

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        // LockExit()
        public ResultCode LockExit(ServiceCtx context)
        {
            lock (_applet.Lock)
            {
                if (_applet.AppletState.HasRequestedExit)
                {
                    _applet.ProcessHandle.Terminate();
                }
                else
                {
                    _applet.ExitLocked = true;
                }
            }

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // UnlockExit()
        public ResultCode UnlockExit(ServiceCtx context)
        {
            lock (_applet.Lock)
            {
                _applet.ExitLocked = false;

                if (_applet.AppletState.HasRequestedExit)
                {
                    _applet.ProcessHandle.Terminate();
                }
            }

            return ResultCode.Success;
        }

        [CommandCmif(3)] // 2.0.0+
        // EnterFatalSection()
        public ResultCode EnterFatalSection(ServiceCtx context)
        {
            lock (_fatalSectionLock)
            {
                _fatalSectionCount++;
            }

            return ResultCode.Success;
        }

        [CommandCmif(4)] // 2.0.0+
        // LeaveFatalSection()
        public ResultCode LeaveFatalSection(ServiceCtx context)
        {
            ResultCode result = ResultCode.Success;

            lock (_fatalSectionLock)
            {
                if (_fatalSectionCount != 0)
                {
                    _fatalSectionCount--;
                }
                else
                {
                    result = ResultCode.UnbalancedFatalSection;
                }
            }

            return result;
        }

        [CommandCmif(9)]
        // GetLibraryAppletLaunchableEvent() -> handle<copy>
        public ResultCode GetLibraryAppletLaunchableEvent(ServiceCtx context)
        {
            var evnt = _applet.AppletState.LaunchableEvent;
            evnt.ReadableEvent.Signal();

            if (_libraryAppletLaunchableEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(evnt.ReadableEvent, out _libraryAppletLaunchableEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_libraryAppletLaunchableEventHandle);

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(10)]
        // SetScreenShotPermission(u32)
        public ResultCode SetScreenShotPermission(ServiceCtx context)
        {
            bool screenShotPermission = context.RequestData.ReadBoolean();

            Logger.Stub?.PrintStub(LogClass.ServiceAm, new { screenShotPermission });

            _screenShotPermission = screenShotPermission;

            return ResultCode.Success;
        }

        [CommandCmif(11)]
        // SetOperationModeChangedNotification(b8)
        public ResultCode SetOperationModeChangedNotification(ServiceCtx context)
        {
            bool operationModeChangedNotification = context.RequestData.ReadBoolean();

            Logger.Stub?.PrintStub(LogClass.ServiceAm, new { operationModeChangedNotification });

            _operationModeChangedNotification = operationModeChangedNotification;

            return ResultCode.Success;
        }

        [CommandCmif(12)]
        // SetPerformanceModeChangedNotification(b8)
        public ResultCode SetPerformanceModeChangedNotification(ServiceCtx context)
        {
            bool performanceModeChangedNotification = context.RequestData.ReadBoolean();

            Logger.Stub?.PrintStub(LogClass.ServiceAm, new { performanceModeChangedNotification });

            _performanceModeChangedNotification = performanceModeChangedNotification;

            return ResultCode.Success;
        }

        [CommandCmif(13)]
        // SetFocusHandlingMode(b8, b8, b8)
        public ResultCode SetFocusHandlingMode(ServiceCtx context)
        {
            bool notify = context.RequestData.ReadBoolean();
            bool background = context.RequestData.ReadBoolean();
            bool suspend = context.RequestData.ReadBoolean();

            Logger.Stub?.PrintStub(LogClass.ServiceAm, new { notify, background, suspend });

            lock (_applet.Lock)
            {
                _applet.AppletState.SetFocusHandlingMode(suspend);
                _applet.AppletState.FocusStateChangedNotificationEnabled = notify;
                _applet.UpdateSuspensionStateLocked(true);
            }

            return ResultCode.Success;
        }

        [CommandCmif(14)]
        // SetRestartMessageEnabled(b8)
        public ResultCode SetRestartMessageEnabled(ServiceCtx context)
        {
            bool restartMessageEnabled = context.RequestData.ReadBoolean();

            Logger.Stub?.PrintStub(LogClass.ServiceAm, new { restartMessageEnabled });

            lock (_applet.Lock)
            {
                _applet.AppletState.ResumeNotificationEnabled = restartMessageEnabled;
            }

            return ResultCode.Success;
        }

        [CommandCmif(16)] // 2.0.0+
        // SetOutOfFocusSuspendingEnabled(b8)
        public ResultCode SetOutOfFocusSuspendingEnabled(ServiceCtx context)
        {
            bool outOfFocusSuspendingEnabled = context.RequestData.ReadBoolean();

            Logger.Stub?.PrintStub(LogClass.ServiceAm, new { outOfFocusSuspendingEnabled });

            lock (_applet.Lock)
            {
                _applet.AppletState.SetOutOfFocusSuspendingEnabled(outOfFocusSuspendingEnabled);
                _applet.UpdateSuspensionStateLocked(false);
            }

            return ResultCode.Success;
        }

        [CommandCmif(19)] // 3.0.0+
        // SetScreenShotImageOrientation(u32)
        public ResultCode SetScreenShotImageOrientation(ServiceCtx context)
        {
            uint screenShotImageOrientation = context.RequestData.ReadUInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceAm, new { screenShotImageOrientation });

            _screenShotImageOrientation = screenShotImageOrientation;

            return ResultCode.Success;
        }

        [CommandCmif(40)]
        // CreateManagedDisplayLayer() -> u64
        public ResultCode CreateManagedDisplayLayer(ServiceCtx context)
        {
            context.Device.System.SurfaceFlinger.CreateLayer(out long layerId, _pid);
            // context.Device.System.SurfaceFlinger.SetRenderLayer(layerId);

            context.ResponseData.Write(layerId);

            return ResultCode.Success;
        }

        [CommandCmif(41)] // 4.0.0+
        // IsSystemBufferSharingEnabled()
        public ResultCode IsSystemBufferSharingEnabled(ServiceCtx context)
        {
            // NOTE: Service checks a private field and return an error if the SystemBufferSharing is disabled.

            // todo check if we're not an AppletId.Application   
            return ResultCode.Success;
        }

        [CommandCmif(42)] // 4.0.0+
        // GetSystemSharedLayerHandle() -> (nn::vi::fbshare::SharedBufferHandle, nn::vi::fbshare::SharedLayerHandle)
        public ResultCode GetSystemSharedLayerHandle(ServiceCtx context)
        {
            context.ResponseData.Write((ulong)context.Device.System.ViServerS.GetSharedBufferNvMapId());
            context.ResponseData.Write(context.Device.System.ViServerS.GetSharedLayerId());

            return ResultCode.Success;
        }

        [CommandCmif(44)] // 10.0.0+
        // CreateManagedDisplaySeparableLayer() -> (u64, u64)
        public ResultCode CreateManagedDisplaySeparableLayer(ServiceCtx context)
        {
            context.Device.System.SurfaceFlinger.CreateLayer(out long displayLayerId, _pid);
            context.Device.System.SurfaceFlinger.CreateLayer(out long recordingLayerId, _pid);

            context.ResponseData.Write(displayLayerId);
            context.ResponseData.Write(recordingLayerId);

            return ResultCode.Success;
        }

        [CommandCmif(50)]
        // SetHandlesRequestToDisplay(b8)
        public ResultCode SetHandlesRequestToDisplay(ServiceCtx context)
        {
            bool handlesRequestToDisplay = context.RequestData.ReadBoolean();

            Logger.Stub?.PrintStub(LogClass.ServiceAm, new { handlesRequestToDisplay });

            _handlesRequestToDisplay = handlesRequestToDisplay;

            return ResultCode.Success;
        }

        [CommandCmif(60)]
        // OverrideAutoSleepTimeAndDimmingTime(i32, i32, i32, i32)
        public ResultCode OverrideAutoSleepTimeAndDimmingTime(ServiceCtx context)
        {
            int unk1 = context.RequestData.ReadInt32();
            int unk2 = context.RequestData.ReadInt32();
            int unk3 = context.RequestData.ReadInt32();
            int unk4 = context.RequestData.ReadInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceAm, new { unk1, unk2, unk3, unk4 });

            return ResultCode.Success;
        }

        [CommandCmif(62)]
        // SetIdleTimeDetectionExtension(u32)
        public ResultCode SetIdleTimeDetectionExtension(ServiceCtx context)
        {
            uint idleTimeDetectionExtension = context.RequestData.ReadUInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceAm, new { idleTimeDetectionExtension });

            _idleTimeDetectionExtension = idleTimeDetectionExtension;

            return ResultCode.Success;
        }

        [CommandCmif(63)]
        // GetIdleTimeDetectionExtension() -> u32
        public ResultCode GetIdleTimeDetectionExtension(ServiceCtx context)
        {
            context.ResponseData.Write(_idleTimeDetectionExtension);

            Logger.Stub?.PrintStub(LogClass.ServiceAm, new { _idleTimeDetectionExtension });

            return ResultCode.Success;
        }

        [CommandCmif(64)]
        // SetInputDetectionSourceSet(u32)
        public ResultCode SetInputDetectionSourceSet(ServiceCtx context)
        {
            uint inputDetectionSourceSet = context.RequestData.ReadUInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceAm, new { inputDetectionSourceSet });

            return ResultCode.Success;
        }

        [CommandCmif(65)]
        // ReportUserIsActive()
        public ResultCode ReportUserIsActive(ServiceCtx context)
        {
            // TODO: Call idle:sys ReportUserIsActive when implemented.

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(67)] //3.0.0+
        // IsIlluminanceAvailable() -> bool
        public ResultCode IsIlluminanceAvailable(ServiceCtx context)
        {
            // NOTE: This should call IsAmbientLightSensorAvailable through to Lbl, but there's no situation where we'd want false.
            context.ResponseData.Write(true);

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(68)]
        // SetAutoSleepDisabled(u8)
        public ResultCode SetAutoSleepDisabled(ServiceCtx context)
        {
            bool autoSleepDisabled = context.RequestData.ReadBoolean();

            _autoSleepDisabled = autoSleepDisabled;

            return ResultCode.Success;
        }

        [CommandCmif(69)]
        // IsAutoSleepDisabled() -> u8
        public ResultCode IsAutoSleepDisabled(ServiceCtx context)
        {
            context.ResponseData.Write(_autoSleepDisabled);

            return ResultCode.Success;
        }

        [CommandCmif(71)] //5.0.0+
        // GetCurrentIlluminanceEx() -> (bool, f32)
        public ResultCode GetCurrentIlluminanceEx(ServiceCtx context)
        {
            // TODO: The light value should be configurable - presumably users using software that takes advantage will want control.
            context.ResponseData.Write(1); // OverLimit
            context.ResponseData.Write(10000f); // Lux - 10K lux is ambient light.

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(80)] // 4.0.0+
        // SetWirelessPriorityMode(s32 wireless_priority_mode)
        public ResultCode SetWirelessPriorityMode(ServiceCtx context)
        {
            WirelessPriorityMode wirelessPriorityMode = (WirelessPriorityMode)context.RequestData.ReadInt32();

            if (wirelessPriorityMode > WirelessPriorityMode.Unknown2)
            {
                return ResultCode.InvalidParameters;
            }

            Logger.Stub?.PrintStub(LogClass.ServiceAm, new { wirelessPriorityMode });

            return ResultCode.Success;
        }

        [CommandCmif(90)] // 6.0.0+
        // GetAccumulatedSuspendedTickValue() -> u64
        public ResultCode GetAccumulatedSuspendedTickValue(ServiceCtx context)
        {
            context.ResponseData.Write(_accumulatedSuspendedTickValue);

            return ResultCode.Success;
        }

        [CommandCmif(91)] // 6.0.0+
        // GetAccumulatedSuspendedTickChangedEvent() -> handle<copy>
        public ResultCode GetAccumulatedSuspendedTickChangedEvent(ServiceCtx context)
        {
            if (_accumulatedSuspendedTickChangedEventHandle == 0)
            {
                _accumulatedSuspendedTickChangedEvent = new KEvent(context.Device.System.KernelContext);

                _accumulatedSuspendedTickChangedEvent.ReadableEvent.Signal();

                if (context.Process.HandleTable.GenerateHandle(_accumulatedSuspendedTickChangedEvent.ReadableEvent, out _accumulatedSuspendedTickChangedEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_accumulatedSuspendedTickChangedEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(100)] // 7.0.0+
        // SetAlbumImageTakenNotificationEnabled(u8)
        public ResultCode SetAlbumImageTakenNotificationEnabled(ServiceCtx context)
        {
            bool albumImageTakenNotificationEnabled = context.RequestData.ReadBoolean();

            _albumImageTakenNotificationEnabled = albumImageTakenNotificationEnabled;

            return ResultCode.Success;
        }

        [CommandCmif(120)] // 11.0.0+
        // SaveCurrentScreenshot(s32 album_report_option)
        public ResultCode SaveCurrentScreenshot(ServiceCtx context)
        {
            AlbumReportOption albumReportOption = (AlbumReportOption)context.RequestData.ReadInt32();

            if (albumReportOption > AlbumReportOption.Unknown3)
            {
                return ResultCode.InvalidParameters;
            }

            Logger.Stub?.PrintStub(LogClass.ServiceAm, new { albumReportOption });

            return ResultCode.Success;
        }

        [CommandCmif(130)] // 13.0.0+
        // SetRecordVolumeMuted(b8)
        public ResultCode SetRecordVolumeMuted(ServiceCtx context)
        {
            bool recordVolumeMuted = context.RequestData.ReadBoolean();

            Logger.Stub?.PrintStub(LogClass.ServiceAm, new { recordVolumeMuted });

            _recordVolumeMuted = recordVolumeMuted;

            return ResultCode.Success;
        }
    }
}
