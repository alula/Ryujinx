using Ryujinx.Common.Logging;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Ovln;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Ovln.Ipc
{
    partial class ReceiverService : IReceiverService
    {
        [CmifCommand(0)]
        // OpenReceiver() -> object<nn::ovln::IReceiver>
        public Result OpenReceiver(out IReceiver service)
        {
            service = new Receiver();

            Logger.Stub?.PrintStub(LogClass.ServiceOvln);

            return Result.Success;
        }
    }
}
