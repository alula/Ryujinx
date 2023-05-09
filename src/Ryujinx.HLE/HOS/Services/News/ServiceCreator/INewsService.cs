using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.News.ServiceCreator
{
    class INewsService : IpcService
    {
        public INewsService(ServiceCtx context) { }

        [CommandCmif(30100)]
        // GetSubscriptionStatus(buffer<unknown, 9>) -> u32
        public ResultCode GetSubscriptionStatus(ServiceCtx context)
        {
            context.ResponseData.Write(0);

            Logger.Stub?.PrintStub(LogClass.ServiceNews);

            return ResultCode.Success;
        }
    }
}