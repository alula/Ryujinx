using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Sdk.Ovln
{
    interface IReceiverService : IServiceObject
    {
        [CmifCommand(0)]
        Result OpenReceiver(out IReceiver service);
    }
}
