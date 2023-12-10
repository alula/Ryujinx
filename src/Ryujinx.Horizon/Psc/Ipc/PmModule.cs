using Ryujinx.Common.Logging;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Psc;
using Ryujinx.Horizon.Sdk.Sf;
using Ryujinx.Horizon.Sdk.Sf.Hipc;
using System;

namespace Ryujinx.Horizon.Psc.Ipc
{
    partial class PmModule : IPmModule
    {
        [CmifCommand(0)]
        public Result Initialize(int ModuleId, [Buffer(HipcBufferFlags.In | HipcBufferFlags.MapAlias)] Span<byte> Dependencies)
        {
            Logger.Stub?.PrintStub(LogClass.ServicePsc);

            return Result.Success;
        }

        [CmifCommand(1)]
        public Result GetResult(out int State, out uint Flags)
        {
            Logger.Stub?.PrintStub(LogClass.ServicePsc);

            State = 0;
            Flags = 0;

            return Result.Success;
        }

        [CmifCommand(2)]
        public Result Acknowledge()
        {
            Logger.Stub?.PrintStub(LogClass.ServicePsc);

            return Result.Success;
        }

        [CmifCommand(3)]
        public Result Finalize()
        {
            Logger.Stub?.PrintStub(LogClass.ServicePsc);

            return Result.Success;
        }

        [CmifCommand(4)]
        public Result AcknowledgeEx(int State)
        {
            Logger.Stub?.PrintStub(LogClass.ServicePsc);

            return Result.Success;
        }
    }
}
