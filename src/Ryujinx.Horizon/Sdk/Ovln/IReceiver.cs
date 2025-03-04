using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Sdk.Ovln
{
    interface IReceiver : IServiceObject
    {
        [CmifCommand(0)]
        Result AddSource(SourceName name);

        [CmifCommand(1)]
        Result RemoveSource(SourceName name);

        [CmifCommand(2)]
        Result GetReceiveEventHandle([CopyHandle] out int handle);

        [CmifCommand(3)]
        Result Receive(out OvlnMessage message);

        [CmifCommand(4)]
        Result ReceiveWithTick(out OvlnMessage message, out long tick);
    }
}
