using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Hshl.Ipc;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Sdk.Hshl
{
    interface IManager : IServiceObject
    {
        Result Unk00(out int unk);
        Result Unk01(out int unk);
        Result OpenChargeSession(out ChargeSession chargeSession);
    }
}
