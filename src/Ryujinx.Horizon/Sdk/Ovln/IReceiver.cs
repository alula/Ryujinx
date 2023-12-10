using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Sdk.Ovln
{
    interface IReceiver : IServiceObject
    {
        Result AddSource(SourceName name);
        Result RemoveSource(SourceName name);
        Result GetReceiveEventHandle(out int handle);
    }
}
