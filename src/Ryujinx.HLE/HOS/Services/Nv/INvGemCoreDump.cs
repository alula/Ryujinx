using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Nv
{
    [Service("nvgem:cd")]
    class INvGemCoreDump : IpcService
    {
        public INvGemCoreDump(ServiceCtx context) { }

        [CommandCmif(0)]
        // Initialize() -> u32
        public ResultCode Initialize(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNv);
            context.ResponseData.Write((uint)0);

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        // GetAruid() -> (u64, u32)
        public ResultCode GetAruid(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNv);

            context.ResponseData.Write((ulong)0);
            context.ResponseData.Write((uint)0);

            return ResultCode.Success;
        }
    }
}
