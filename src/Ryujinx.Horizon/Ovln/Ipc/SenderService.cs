using Ryujinx.Common.Logging;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Ovln;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Ovln.Ipc
{
    partial class SenderService : ISenderService
    {
        private readonly MessageSourceManager _messageSourceManager;

        public SenderService(MessageSourceManager messageSourceManager)
        {
            _messageSourceManager = messageSourceManager;
        }

        [CmifCommand(0)]
        public Result OpenSender(out ISender service, SourceName name, ulong queueSize)
        {
            var source = _messageSourceManager.AddSource(name.GetString());

            service = new Sender(source);

            Logger.Stub?.PrintStub(LogClass.ServiceOvln, new { name = name.GetString(), queueSize });

            return Result.Success;
        }
    }
}
