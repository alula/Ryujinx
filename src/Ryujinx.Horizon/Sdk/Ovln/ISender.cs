using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Sdk.Ovln
{
    interface ISender : IServiceObject
    {
        [CmifCommand(0)]
        Result Send(OvlnMessage message, OvlnMessageFlags flags);

        [CmifCommand(1)]
        Result GetUnreceivedMessageCount(out uint count);
    }
}
