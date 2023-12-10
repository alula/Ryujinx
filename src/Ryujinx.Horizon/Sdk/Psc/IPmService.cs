using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Psc.Ipc;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Sdk.Psc
{
    interface IPmService : IServiceObject
    {
        Result GetPmModule(out PmModule module);
    }
}
