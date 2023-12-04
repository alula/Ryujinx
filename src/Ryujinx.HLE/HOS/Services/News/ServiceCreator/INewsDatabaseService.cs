using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.News.ServiceCreator
{
    class INewsDatabaseService : IpcService
    {
        public INewsDatabaseService(ServiceCtx context) { }

        [CommandCmif(0)]
        // GetListV1(unknown<0x4>, buffer<unknown, 0x9>, buffer<unknown, 0x9>) -> (unknown<0x4>, buffer<unknown, 0x6>)
        public ResultCode GetListV1(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNews);

            context.ResponseData.Write(0);

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        // Count(buffer<unknown, 9>) -> u32
        public ResultCode Count(ServiceCtx context)
        {
            context.ResponseData.Write(0);

            Logger.Stub?.PrintStub(LogClass.ServiceNews);

            return ResultCode.Success;
        }

        [CommandCmif(4)]
        // UpdateIntegerValueWithAddition(unknown<0x4>, buffer<unknown, 0x9>, buffer<unknown, 0x9>)
        public ResultCode UpdateIntegerValueWithAddition(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNews);

            return ResultCode.Success;
        }
    }
}
