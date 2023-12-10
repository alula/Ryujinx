using LibHac.Util;
using Ryujinx.Common.Logging;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.OsTypes;
using Ryujinx.Horizon.Sdk.Ovln;
using Ryujinx.Horizon.Sdk.Sf;
using System.Threading;

namespace Ryujinx.Horizon.Ovln.Ipc
{
    partial class Receiver : IReceiver
    {
        private int _handle;
        private SystemEventType _recvEvent;
        private int _disposalState;

        [CmifCommand(0)]
        public Result AddSource(SourceName name)
        {
            Logger.Debug?.Print(LogClass.ServiceOvln, $"SourceName: {name.Unknown.ToHexString()}");

            return Result.Success;
        }

        [CmifCommand(1)]
        public Result RemoveSource(SourceName name)
        {
            Logger.Debug?.Print(LogClass.ServiceOvln, $"SourceName: {name.Unknown.ToHexString()}");

            return Result.Success;
        }

        [CmifCommand(2)]
        public Result GetReceiveEventHandle([CopyHandle] out int handle)
        {
            if (_handle == 0)
            {
                Os.CreateSystemEvent(out _recvEvent, EventClearMode.ManualClear, true).AbortOnFailure();

                _handle = Os.GetReadableHandleOfSystemEvent(ref _recvEvent);

                Os.SignalSystemEvent(ref _recvEvent);
            }

            handle = _handle;

            return Result.Success;
        }


        public void Dispose()
        {
            if (_handle != 0 && Interlocked.Exchange(ref _disposalState, 1) == 0)
            {
                Os.DestroySystemEvent(ref _recvEvent);
            }
        }
    }
}
