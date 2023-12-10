using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Pcv.Rtc
{
    [Service("rtc")] // 8.0.0+
    class IRtcManager : IpcService
    {
        public IRtcManager(ServiceCtx context) { }

        [CommandCmif(0)]
        // GetTimeInSeconds(u32) -> u64
        public ResultCode GetTimeInSeconds(ServiceCtx context)
        {
            ResultCode result = Bpc.IRtcManager.GetExternalRtcValue(out ulong rtcValue);

            if (result == ResultCode.Success)
            {
                context.ResponseData.Write(rtcValue);
            }

            return result;
        }

        [CommandCmif(1)]
        // SetTimeInSeconds(u32)
        public ResultCode SetTimeInSeconds(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServicePcv);

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // SetResetOnShutdown(bool, u32)
        public ResultCode SetResetOnShutdown(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServicePcv);

            return ResultCode.Success;
        }

        [CommandCmif(3)]
        // GetResetDetected(u32) -> bool
        public ResultCode GetResetDetected(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServicePcv);

            context.ResponseData.Write(false);

            return ResultCode.Success;
        }

        [CommandCmif(4)]
        // ClearResetDetected(u32)
        public ResultCode ClearResetDetected(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServicePcv);

            return ResultCode.Success;
        }

        [CommandCmif(5)]
        // EnableAlarm(u32, u64)
        public ResultCode EnableAlarm(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServicePcv);

            return ResultCode.Success;
        }

        [CommandCmif(6)]
        // DisableAlarm(u32)
        public ResultCode DisableAlarm(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServicePcv);

            return ResultCode.Success;
        }
    }
}
