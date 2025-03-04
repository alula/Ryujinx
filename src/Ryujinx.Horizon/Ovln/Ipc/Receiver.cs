using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.OsTypes;
using Ryujinx.Horizon.Sdk.Ovln;
using Ryujinx.Horizon.Sdk.Sf;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.Horizon.Ovln.Ipc
{
    partial class Receiver : IReceiver, IDisposable
    {
        private readonly MessageSourceManager _manager;
        private int _handle;
        private SystemEventType _recvEvent;
        private int _disposalState;
        private Dictionary<string, MessageSource> _sources = new();

        public Receiver(MessageSourceManager manager)
        {
            _manager = manager;
            Os.CreateSystemEvent(out _recvEvent, EventClearMode.ManualClear, true).AbortOnFailure();
        }

        private void OnMessageAvailable(object sender, EventArgs e)
        {
            Os.SignalSystemEvent(ref _recvEvent);
        }

        [CmifCommand(0)]
        public Result AddSource(SourceName name)
        {
            var sourceName = name.GetString();
            if (!_sources.ContainsKey(sourceName))
            {
                var source = _manager.AddSource(sourceName);
                source.MessageAvailable += OnMessageAvailable;
                _sources.Add(sourceName, source);
            }
            // Logger.Debug?.Print(LogClass.ServiceOvln, $"SourceName: {name.GetString()}");

            return Result.Success;
        }

        [CmifCommand(1)]
        public Result RemoveSource(SourceName name)
        {
            var sourceName = name.GetString();
            if (_sources.Remove(sourceName, out var messageSource))
            {
                messageSource.MessageAvailable -= OnMessageAvailable;
                _manager.RemoveSource(sourceName);
            }
            // Logger.Debug?.Print(LogClass.ServiceOvln, $"SourceName: {name.GetString()}");

            return Result.Success;
        }

        [CmifCommand(2)]
        public Result GetReceiveEventHandle([CopyHandle] out int handle)
        {
            if (_handle == 0)
            {
                _handle = Os.GetReadableHandleOfSystemEvent(ref _recvEvent);
            }

            handle = _handle;

            return Result.Success;
        }

        [CmifCommand(3)]
        public Result Receive(out OvlnMessage message)
        {
            return ReceiveWithTick(out message, out _);
        }

        [CmifCommand(4)]
        public Result ReceiveWithTick(out OvlnMessage message, out long tick)
        {
            ulong lowestId = ulong.MaxValue;
            MessageSource targetSource = null;

            foreach (var (name, source) in _sources)
            {
                if (source.TryPeek(out var peekMessage, out var peekId, out _) && peekId < lowestId)
                {
                    lowestId = peekId;
                    targetSource = source;
                }
            }

            if (targetSource != null)
            {
                targetSource.TryPop(out message, out _, out tick);
            }
            else
            {
                message = default;
                tick = 0;
                return OvlnResult.NoMessages;
            }

            return Result.Success;
        }

        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref _disposalState, 1, 0) == 1)
            {
                return;
            }

            if (_handle != 0 && Interlocked.Exchange(ref _disposalState, 1) == 0)
            {
                Os.DestroySystemEvent(ref _recvEvent);
            }

            foreach (var (name, source) in _sources)
            {
                source.MessageAvailable -= OnMessageAvailable;
                _manager.RemoveSource(name);
            }
        }
    }

    [ImplementApi]
    public partial class ReceiverApi : IReceiver
    {
    }
}
