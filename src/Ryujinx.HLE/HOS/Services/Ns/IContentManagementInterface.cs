using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Ns
{
    class IContentManagementInterface : IpcService
    {
        public IContentManagementInterface(ServiceCtx context) { }

        [CommandCmif(43)]
        // CheckSdCardMountStatus()
        public ResultCode CheckSdCardMountStatus(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNs);

            return ResultCode.Success;
        }

        [CommandCmif(47)]
        // GetTotalSpaceSize(u8) -> s64
        public ResultCode GetTotalSpaceSize(ServiceCtx context)
        {
            context.ResponseData.Write(0UL);

            Logger.Stub?.PrintStub(LogClass.ServiceNs);

            return ResultCode.Success;
        }

        [CommandCmif(48)]
        // GetFreeSpaceSize(u8) -> s64
        public ResultCode GetFreeSpaceSize(ServiceCtx context)
        {
            context.ResponseData.Write(0UL);

            Logger.Stub?.PrintStub(LogClass.ServiceNs);

            return ResultCode.Success;
        }
    }
}