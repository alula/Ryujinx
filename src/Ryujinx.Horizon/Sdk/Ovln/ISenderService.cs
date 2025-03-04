using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Sdk.Ovln
{
    interface ISenderService : IServiceObject
    {
        [CmifCommand(0)]
        Result OpenSender(out ISender service, SourceName name, ulong queueSize);
    }
}
