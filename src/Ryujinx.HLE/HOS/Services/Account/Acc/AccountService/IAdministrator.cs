using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Account.Acc.AccountService
{
    class IAdministrator : IpcService
    {
        public IAdministrator(UserId userId)
        {
            
        }

        [CommandCmif(0)]
        // CheckAvailability()
        public ResultCode CheckAvailability(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAcc);

            return ResultCode.Success;
        }

        [CommandCmif(250)]
        // IsLinkedWithNintendoAccount() -> bool
        public ResultCode IsLinkedWithNintendoAccount(ServiceCtx context)
        {
            context.ResponseData.Write(0);

            Logger.Stub?.PrintStub(LogClass.ServiceAcc);

            return ResultCode.Success;
        }
    }
}
