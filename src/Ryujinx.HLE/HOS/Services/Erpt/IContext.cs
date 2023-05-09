using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Erpt
{
    [Service("erpt:c")]
    class IContext : IpcService
    {
        public IContext(ServiceCtx context) { }

        [CommandCmif(0)]
        // SubmitContext(buffer<ContextEntry, 5>, buffer<FieldList, 5>)
        public ResultCode SubmitContext(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceErpt);

            return ResultCode.Success;
        }
    }
}
