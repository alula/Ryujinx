using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.News.ServiceCreator
{
    class INewsService : IpcService
    {
        public INewsService(ServiceCtx context) { }

        [CommandCmif(10100)]
        // PostLocalNews(buffer<unknown, 5>)
        public ResultCode PostLocalNews(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNews);

            return ResultCode.Success;
        }

        [CommandCmif(20100)]
        // SetPassphrase(u64, buffer<unknown, 9>)
        public ResultCode SetPassphrase(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNews);

            return ResultCode.Success;
        }

        [CommandCmif(30100)]
        // GetSubscriptionStatus(buffer<unknown, 9>) -> u32
        public ResultCode GetSubscriptionStatus(ServiceCtx context)
        {
            context.ResponseData.Write(0);

            Logger.Stub?.PrintStub(LogClass.ServiceNews);

            return ResultCode.Success;
        }

        [CommandCmif(30200)]
        // IsSystemUpdateRequired() -> bool
        public ResultCode IsSystemUpdateRequired(ServiceCtx context)
        {
            context.ResponseData.Write(0);

            Logger.Stub?.PrintStub(LogClass.ServiceNews);

            return ResultCode.Success;
        }

        [CommandCmif(40100)]
        // SetSubscriptionStatus()
        public ResultCode SetSubscriptionStatus(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNews);

            return ResultCode.Success;
        }

        [CommandCmif(40101)]
        // RequestAutoSubscription(u64)
        public ResultCode RequestAutoSubscription(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNews);

            return ResultCode.Success;
        }

        [CommandCmif(40201)]
        // ClearSubscriptionStatusAll()
        public ResultCode ClearSubscriptionStatusAll(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNews);

            return ResultCode.Success;
        }
    }
}
