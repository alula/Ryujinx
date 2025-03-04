using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Ovln;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Ovln.Ipc
{
    partial class Sender : ISender
    {
        private readonly MessageSource _source;

        public Sender(MessageSource source)
        {
            _source = source;
        }

        [CmifCommand(0)]
        public Result Send(OvlnMessage message, OvlnMessageFlags flags)
        {
            Logger.Debug?.PrintStub(LogClass.ServiceOvln, new { message, flags });

            long tick = PerformanceCounter.ElapsedTicks;

            if (flags.PushToBack)
            {
                _source.TryAddBack(message, tick);
            }
            else
            {
                _source.TryAddFront(message, tick);
            }

            return Result.Success;
        }

        [CmifCommand(1)]
        public Result GetUnreceivedMessageCount(out uint count)
        {
            count = (uint)_source.Count;
            return Result.Success;
        }
    }
}
