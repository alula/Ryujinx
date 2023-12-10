using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Hshl.Ipc;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Sdk.Hshl
{
    interface IManager : IServiceObject
    {
        Result OpenChargeSession(out ChargeSession chargeSession);
    }
}
