using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Applets;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Settings.Types;
using Ryujinx.HLE.HOS.Services.Vi.RootService.ApplicationDisplayService;
using Ryujinx.HLE.HOS.SystemState;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Lbl;
using System;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class ICommonStateGetter : DisposableIpcService
    {
        private readonly ServiceCtx _context;

        private readonly Apm.ManagerServer _apmManagerServer;
        private readonly Apm.SystemManagerServer _apmSystemManagerServer;
        private readonly RealApplet _applet;

        private bool _vrModeEnabled;
#pragma warning disable CS0414, IDE0052 // Remove unread private member
        private bool _lcdBacklighOffEnabled;
        private bool _requestExitToLibraryAppletAtExecuteNextProgramEnabled;
#pragma warning restore CS0414, IDE0052
        private int _messageEventHandle;
        private int _displayResolutionChangedEventHandle;

        private readonly KEvent _acquiredSleepLockEvent;
        private int _acquiredSleepLockEventHandle;

        public ICommonStateGetter(ServiceCtx context, ulong pid)
        {
            _context = context;
            _applet = context.Device.System.WindowSystem.GetByAruId(pid);

            _apmManagerServer = new Apm.ManagerServer(context);
            _apmSystemManagerServer = new Apm.SystemManagerServer(context);

            _acquiredSleepLockEvent = new KEvent(context.Device.System.KernelContext);
        }

        [CommandCmif(0)]
        // GetEventHandle() -> handle<copy>
        public ResultCode GetEventHandle(ServiceCtx context)
        {
            KEvent messageEvent = _applet.AppletState.MessageEvent;

            if (_messageEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(messageEvent.ReadableEvent, out _messageEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_messageEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        // ReceiveMessage() -> nn::am::AppletMessage
        public ResultCode ReceiveMessage(ServiceCtx context)
        {
            if (!_applet.AppletState.PopMessage(out AppletMessage message))
            {
                return ResultCode.NoMessages;
            }

            Logger.Info?.Print(LogClass.ServiceAm, $"pid: {_applet.ProcessHandle.Pid}, msg={message}");
            context.ResponseData.Write((int)message);

            return ResultCode.Success;
        }

        [CommandCmif(5)]
        // GetOperationMode() -> u8
        public ResultCode GetOperationMode(ServiceCtx context)
        {
            OperationMode mode = context.Device.System.State.DockedMode
                ? OperationMode.Docked
                : OperationMode.Handheld;

            context.ResponseData.Write((byte)mode);

            return ResultCode.Success;
        }

        [CommandCmif(6)]
        // GetPerformanceMode() -> nn::apm::PerformanceMode
        public ResultCode GetPerformanceMode(ServiceCtx context)
        {
            return (ResultCode)_apmManagerServer.GetPerformanceMode(context);
        }

        [CommandCmif(8)]
        // GetBootMode() -> u8
        public ResultCode GetBootMode(ServiceCtx context)
        {
            context.ResponseData.Write((byte)0); // PmBootMode_Normal

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(9)]
        // GetCurrentFocusState() -> u8
        public ResultCode GetCurrentFocusState(ServiceCtx context)
        {
            FocusState focusState;
            lock (_applet.Lock)
            {
                focusState = _applet.AppletState.GetAndClearFocusState();
            }
            ;

            Logger.Info?.Print(LogClass.ServiceAm, $"pid: {_applet.ProcessHandle.Pid}, GetCurrentFocusState():{focusState}");
            context.ResponseData.Write((byte)focusState);

            return ResultCode.Success;
        }

        [CommandCmif(10)]
        // RequestToAcquireSleepLock()
        public ResultCode RequestToAcquireSleepLock(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            _acquiredSleepLockEvent.ReadableEvent.Signal();

            return ResultCode.Success;
        }

        [CommandCmif(13)]
        // GetAcquiredSleepLockEvent() -> handle<copy>
        public ResultCode GetAcquiredSleepLockEvent(ServiceCtx context)
        {
            if (_acquiredSleepLockEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_acquiredSleepLockEvent.ReadableEvent, out _acquiredSleepLockEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_acquiredSleepLockEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(20)]
        // PushToGeneralChannel(object<nn::am::service::IStorage>)
        public ResultCode PushInData(ServiceCtx context)
        {
            IStorage data = GetObject<IStorage>(context, 0);

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(31)]
        // GetReaderLockAccessorEx(u32) -> object<nn::am::service::ILockAccessor>
        public ResultCode GetReaderLockAccessorEx(ServiceCtx context)
        {
            int lockId = context.RequestData.ReadInt32();

            MakeObject(context, new ILockAccessor(lockId, context.Device.System));

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(50)] // 3.0.0+
        // IsVrModeEnabled() -> b8
        public ResultCode IsVrModeEnabled(ServiceCtx context)
        {
            context.ResponseData.Write(_vrModeEnabled);

            return ResultCode.Success;
        }

        [CommandCmif(51)] // 3.0.0+
        // SetVrModeEnabled(b8)
        public ResultCode SetVrModeEnabled(ServiceCtx context)
        {
            bool vrModeEnabled = context.RequestData.ReadBoolean();

            UpdateVrMode(vrModeEnabled);

            return ResultCode.Success;
        }

        [CommandCmif(52)] // 4.0.0+
        // SetLcdBacklighOffEnabled(b8)
        public ResultCode SetLcdBacklighOffEnabled(ServiceCtx context)
        {
            // NOTE: Service sets a private field here, maybe this field is used somewhere else to turned off the backlight.
            //       Since we don't support backlight, it's fine to do nothing.

            _lcdBacklighOffEnabled = context.RequestData.ReadBoolean();

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(53)] // 7.0.0+
        // BeginVrModeEx()
        public ResultCode BeginVrModeEx(ServiceCtx context)
        {
            UpdateVrMode(true);

            return ResultCode.Success;
        }

        [CommandCmif(54)] // 7.0.0+
        // EndVrModeEx()
        public ResultCode EndVrModeEx(ServiceCtx context)
        {
            UpdateVrMode(false);

            return ResultCode.Success;
        }

        private void UpdateVrMode(bool vrModeEnabled)
        {
            if (_vrModeEnabled == vrModeEnabled)
            {
                return;
            }

            _vrModeEnabled = vrModeEnabled;

            using var lblApi = new LblApi();

            if (vrModeEnabled)
            {
                lblApi.EnableVrMode().AbortOnFailure();
            }
            else
            {
                lblApi.DisableVrMode().AbortOnFailure();
            }

            // TODO: It signals an internal event of ICommonStateGetter. We have to determine where this event is used.
        }

        [CommandCmif(60)] // 3.0.0+
        // GetDefaultDisplayResolution() -> (u32, u32)
        public ResultCode GetDefaultDisplayResolution(ServiceCtx context)
        {
            // NOTE: Original service calls IOperationModeManager::GetDefaultDisplayResolution of omm service.
            //       IOperationModeManager::GetDefaultDisplayResolution of omm service call IManagerDisplayService::GetDisplayResolution of vi service.
            (ulong width, ulong height) = AndroidSurfaceComposerClient.GetDisplayInfo(context);

            context.ResponseData.Write((uint)width);
            context.ResponseData.Write((uint)height);

            return ResultCode.Success;
        }

        [CommandCmif(61)] // 3.0.0+
        // GetDefaultDisplayResolutionChangeEvent() -> handle<copy>
        public ResultCode GetDefaultDisplayResolutionChangeEvent(ServiceCtx context)
        {
            // NOTE: Original service calls IOperationModeManager::GetDefaultDisplayResolutionChangeEvent of omm service.
            if (_displayResolutionChangedEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(context.Device.System.DisplayResolutionChangeEvent.ReadableEvent, out _displayResolutionChangedEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_displayResolutionChangedEventHandle);

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(62)] // 4.0.0+
        // GetHdcpAuthenticationState() -> s32 state
        public ResultCode GetHdcpAuthenticationState(ServiceCtx context)
        {
            context.ResponseData.Write(0);

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(66)] // 6.0.0+
        // SetCpuBoostMode(u32 cpu_boost_mode)
        public ResultCode SetCpuBoostMode(ServiceCtx context)
        {
            uint cpuBoostMode = context.RequestData.ReadUInt32();

            if (cpuBoostMode > 1)
            {
                return ResultCode.InvalidParameters;
            }

            _apmSystemManagerServer.SetCpuBoostMode((Apm.CpuBoostMode)cpuBoostMode);

            // TODO: It signals an internal event of ICommonStateGetter. We have to determine where this event is used.

            return ResultCode.Success;
        }

        [CommandCmif(91)] // 7.0.0+
        // GetCurrentPerformanceConfiguration() -> nn::apm::PerformanceConfiguration
        public ResultCode GetCurrentPerformanceConfiguration(ServiceCtx context)
        {
            return (ResultCode)_apmSystemManagerServer.GetCurrentPerformanceConfiguration(context);
        }

        [CommandCmif(300)] // 9.0.0+
        // GetSettingsPlatformRegion() -> u8
        public ResultCode GetSettingsPlatformRegion(ServiceCtx context)
        {
            PlatformRegion platformRegion = context.Device.System.State.DesiredRegionCode == (uint)RegionCode.China ? PlatformRegion.China : PlatformRegion.Global;

            // FIXME: Call set:sys GetPlatformRegion
            context.ResponseData.Write((byte)platformRegion);

            return ResultCode.Success;
        }

        [CommandCmif(900)] // 11.0.0+
        // SetRequestExitToLibraryAppletAtExecuteNextProgramEnabled()
        public ResultCode SetRequestExitToLibraryAppletAtExecuteNextProgramEnabled(ServiceCtx context)
        {
            // TODO : Find where the field is used.
            _requestExitToLibraryAppletAtExecuteNextProgramEnabled = true;

            return ResultCode.Success;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (_acquiredSleepLockEventHandle != 0)
                {
                    _context.Process.HandleTable.CloseHandle(_acquiredSleepLockEventHandle);
                    _acquiredSleepLockEventHandle = 0;
                }
            }
        }
    }
}
