using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Sdk.Ovln
{
    interface IReceiverService : IServiceObject
    {
        Result OpenReceiver(out IReceiver service);
    }
}
