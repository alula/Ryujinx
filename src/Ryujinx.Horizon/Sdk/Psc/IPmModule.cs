using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;
using System;

namespace Ryujinx.Horizon.Sdk.Psc
{
    interface IPmModule : IServiceObject
    {
        Result Initialize(int ModuleId, Span<byte> Dependencies);
        Result GetResult(out int State, out uint Flags);
        Result Acknowledge();
        Result Finalize();
        Result AcknowledgeEx(int State);
    }
}
