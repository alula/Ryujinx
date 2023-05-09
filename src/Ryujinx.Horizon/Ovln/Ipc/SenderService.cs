using Ryujinx.Common.Logging;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Ovln;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Ovln.Ipc
{
    partial class SenderService : ISenderService
    {
        [CmifCommand(0)]
        // OpenSender() -> object<nn::ovln::ISender>
        public Result OpenSender(out ISender service)
        {
            service = new Sender();

            Logger.Stub?.PrintStub(LogClass.ServiceOvln);

            return Result.Success;
        }
    }
}
