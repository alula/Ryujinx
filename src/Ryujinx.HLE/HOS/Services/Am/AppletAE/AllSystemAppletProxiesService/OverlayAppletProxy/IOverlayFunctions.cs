using LibHac.Util;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.OverlayAppletProxy
{
    // Cmd 	Name
    // 0 	#BeginToWatchShortHomeButtonMessage
    // 1 	#EndToWatchShortHomeButtonMessage
    // 2 	#GetApplicationIdForLogo
    // 3 	#SetGpuTimeSliceBoost
    // 4 	[2.0.0+] #SetAutoSleepTimeAndDimmingTimeEnabled
    // 5 	[2.0.0+] #TerminateApplicationAndSetReason
    // 6 	[3.0.0+] #SetScreenShotPermissionGlobally
    // 10 	[6.0.0+] #StartShutdownSequenceForOverlay
    // 11 	[6.0.0+] #StartRebootSequenceForOverlay
    // 20 	[8.0.0+] #SetHandlingHomeButtonShortPressedEnabled
    // 21 	[14.0.0+] SetHandlingTouchScreenInputEnabled
    // 30 	[9.0.0+] #SetHealthWarningShowingState
    // 31 	[10.0.0+] #IsHealthWarningRequired
    // 40 	[18.0.0+] GetApplicationNintendoLogo
    // 41 	[18.0.0+] GetApplicationStartupMovie
    // 50 	[19.0.0+] SetGpuTimeSliceBoostForApplication
    // 60 	[19.0.0+]
    // 90 	[7.0.0+] #SetRequiresGpuResourceUse
    // 101 	[5.0.0+] #BeginToObserveHidInputForDevelop
    class IOverlayFunctions : IpcService
    {

        public IOverlayFunctions(Horizon system)
        {
        }

        [CommandCmif(0)]
        // BeginToWatchShortHomeButtonMessage()
        public ResultCode BeginToWatchShortHomeButtonMessage(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        // EndToWatchShortHomeButtonMessage()
        public ResultCode EndToWatchShortHomeButtonMessage(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // GetApplicationIdForLogo() -> nn::ncm::ApplicationId
        public ResultCode GetApplicationIdForLogo(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            context.ResponseData.Write(0L);

            return ResultCode.Success;
        }

        [CommandCmif(3)]
        // SetGpuTimeSliceBoost(u64)
        public ResultCode SetGpuTimeSliceBoost(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(4)]
        // SetAutoSleepTimeAndDimmingTimeEnabled(u8)
        public ResultCode SetAutoSleepTimeAndDimmingTimeEnabled(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(5)]
        // TerminateApplicationAndSetReason(u32)
        public ResultCode TerminateApplicationAndSetReason(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(6)]
        // SetScreenShotPermissionGlobally(u8)
        public ResultCode SetScreenShotPermissionGlobally(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(10)]
        // StartShutdownSequenceForOverlay()
        public ResultCode StartShutdownSequenceForOverlay(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(11)]
        // StartRebootSequenceForOverlay()
        public ResultCode StartRebootSequenceForOverlay(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(20)]
        // SetHandlingHomeButtonShortPressedEnabled(u8)
        public ResultCode SetHandlingHomeButtonShortPressedEnabled(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(21)]
        // SetHandlingTouchScreenInputEnabled(u8)
        public ResultCode SetHandlingTouchScreenInputEnabled(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(30)]
        // SetHealthWarningShowingState(u8)
        public ResultCode SetHealthWarningShowingState(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(31)]
        // IsHealthWarningRequired() -> bool
        public ResultCode IsHealthWarningRequired(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            context.ResponseData.Write(false);

            return ResultCode.Success;
        }

        [CommandCmif(90)]
        // SetRequiresGpuResourceUse(u8)
        public ResultCode SetRequiresGpuResourceUse(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(101)]
        // BeginToObserveHidInputForDevelop()
        public ResultCode BeginToObserveHidInputForDevelop(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }
    }
}
