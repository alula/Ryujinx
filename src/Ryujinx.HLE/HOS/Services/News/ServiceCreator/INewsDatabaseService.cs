using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.News.ServiceCreator
{
    class INewsDatabaseService : IpcService
    {
        public INewsDatabaseService(ServiceCtx context) { }

        [CommandCmif(1)]
        // Count(buffer<unknown, 9>) -> u32
        public ResultCode Count(ServiceCtx context)
        {
            context.ResponseData.Write(0);

            Logger.Stub?.PrintStub(LogClass.ServiceNews);

            return ResultCode.Success;
        }
    }
}