using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Olsc.OlscServiceForSystemService;

namespace Ryujinx.HLE.HOS.Services.Olsc
{
    [Service("olsc:s")] // 4.0.0+
    class IOlscServiceForSystemService : IpcService
    {
        public IOlscServiceForSystemService(ServiceCtx context) { }

        [CommandCmif(0)]
        // Unknown0() -> object<nn::olsc::srv::ITransferTaskListController>
        public ResultCode Unknown0(ServiceCtx context)
        {
            MakeObject(context, new ITransferTaskListController(context));

            Logger.Stub?.PrintStub(LogClass.ServiceOlsc);

            return ResultCode.Success;
        }
    }
}
