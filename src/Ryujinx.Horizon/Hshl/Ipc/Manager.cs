using Ryujinx.Common.Logging;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Hshl;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Hshl.Ipc
{
    partial class Manager : IManager
    {
        [CmifCommand(0)]
        public Result Unk00(out int unk)
        {
            unk = 69;

            Logger.Stub?.PrintStub(LogClass.ServiceHshl);

            return Result.Success;
        }


        [CmifCommand(1)]
        public Result Unk01(out int unk)
        {
            unk = 45;

            Logger.Stub?.PrintStub(LogClass.ServiceHshl);

            return Result.Success;
        }

        [CmifCommand(2)]
        public Result OpenChargeSession(out ChargeSession chargeSession)
        {
            chargeSession = new ChargeSession();
            return Result.Success;
        }
    }
}
