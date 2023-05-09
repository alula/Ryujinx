using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Olsc.OlscServiceForSystemService
{
    class ITransferTaskListController : IpcService
    {
        public ITransferTaskListController(ServiceCtx context) { }

        [CommandCmif(5)]
        // Unknown5() -> object<nn::olsc::srv::INativeHandleHolder>
        public ResultCode Unknown5(ServiceCtx context)
        {
            MakeObject(context, new INativeHandleHolder(context));

            Logger.Stub?.PrintStub(LogClass.ServiceOlsc);

            return ResultCode.Success;
        }

        [CommandCmif(9)]
        // Unknown9() -> object<nn::olsc::srv::INativeHandleHolder>
        public ResultCode Unknown9(ServiceCtx context)
        {
            MakeObject(context, new INativeHandleHolder(context));

            Logger.Stub?.PrintStub(LogClass.ServiceOlsc);

            return ResultCode.Success;
        }
    }
}