using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Hshl;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Hshl.Ipc
{
    partial class Manager : IManager
    {
        [CmifCommand(2)]
        public Result OpenChargeSession(out ChargeSession chargeSession)
        {
            chargeSession = new ChargeSession();
            return Result.Success;
        }
    }
}
