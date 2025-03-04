using Ryujinx.Common.Logging;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Ovln;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Ovln.Ipc
{
    partial class ReceiverService : IReceiverService
    {
        private readonly MessageSourceManager _manager;

        public ReceiverService(MessageSourceManager manager)
        {
            _manager = manager;
        }

        [CmifCommand(0)]
        public Result OpenReceiver(out IReceiver service)
        {
            service = new Receiver(_manager);

            // Logger.Stub?.PrintStub(LogClass.ServiceOvln);

            return Result.Success;
        }
    }
}
