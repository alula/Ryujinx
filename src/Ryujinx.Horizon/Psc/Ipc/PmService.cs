using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Psc;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Psc.Ipc
{
    partial class PmService : IPmService
    {
        [CmifCommand(0)]
        public Result GetPmModule(out PmModule module)
        {
            module = new PmModule();

            return Result.Success;
        }
    }
}
