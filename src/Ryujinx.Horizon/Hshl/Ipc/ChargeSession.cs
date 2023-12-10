using Ryujinx.Horizon.Sdk.Hshl;
using Ryujinx.Common.Logging;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.OsTypes;
using Ryujinx.Horizon.Sdk.Sf;
using System.Threading;

namespace Ryujinx.Horizon.Hshl.Ipc
{
    partial class ChargeSession : IChargeSession
    {
        private int _handle;
        private SystemEventType _systemEvent;
        private int _disposalState;

        [CmifCommand(0)]
        public Result GetEvent([CopyHandle] out int handle)
        {
            if (_handle == 0)
            {
                Os.CreateSystemEvent(out _systemEvent, EventClearMode.ManualClear, true).AbortOnFailure();

                _handle = Os.GetReadableHandleOfSystemEvent(ref _systemEvent);
            }

            handle = _handle;

            Logger.Stub?.PrintStub(LogClass.ServiceHshl);

            return Result.Success;
        }

        [CmifCommand(1)]
        public Result Unk01(int unknown)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceHshl);

            return Result.Success;
        }

        public void Dispose()
        {
            if (_handle != 0 && Interlocked.Exchange(ref _disposalState, 1) == 0)
            {
                Os.DestroySystemEvent(ref _systemEvent);
            }
        }
    }
}
