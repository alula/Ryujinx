using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Cec
{
    [Service("cec-mgr")]
    class ICecManager : IpcService
    {
        public ICecManager(ServiceCtx context) { }

        [CommandCmif(0)]
        // Unknown0() -> u32
        public ResultCode Unknown0(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceCec, "Stubbed.");
            return ResultCode.Success;
        }

        [CommandCmif(1)]
        // Unknown1(u32) -> u32
        public ResultCode Unknown1(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceCec, "Stubbed.");
            context.ResponseData.Write(0);

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // Unknown2(u32)
        public ResultCode Unknown2(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceCec, "Stubbed.");
            return ResultCode.Success;
        }

        [CommandCmif(3)]
        // Unknown3()
        public ResultCode Unknown3(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceCec, "Stubbed.");
            return ResultCode.Success;
        }

        [CommandCmif(4)]
        // Unknown4()
        public ResultCode Unknown4(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceCec, "Stubbed.");
            return ResultCode.Success;
        }

        [CommandCmif(5)]
        // Unknown5()
        public ResultCode Unknown5(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceCec, "Stubbed.");
            return ResultCode.Success;
        }

        [CommandCmif(6)]
        // Unknown6()
        public ResultCode Unknown6(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceCec, "Stubbed.");
            return ResultCode.Success;
        }
    }
}
