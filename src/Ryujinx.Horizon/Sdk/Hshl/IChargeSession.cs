using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Sdk.Hshl
{
    interface IChargeSession : IServiceObject
    {
        Result GetEvent(out int handle);
        Result Unk01(int unknown);
    }
}
