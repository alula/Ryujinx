
using Ryujinx.Common.Collections;
using Ryujinx.Horizon.Sdk.Ovln;
using System;
using System.Collections.Concurrent;

namespace Ryujinx.Horizon.Ovln
{
    public class MessageSourceManager
    {
        private ConcurrentDictionary<string, MessageSource> _sources = new();
        private ulong _idCounter;

        public MessageSource AddSource(string name)
        {
            if (_sources.TryGetValue(name, out MessageSource source))
            {
                source._refCount++;
            }
            else
            {
                source = new MessageSource(this);
                _sources.TryAdd(name, source);
            }

            return source;
        }

        public MessageSource GetSource(string name)
        {
            _sources.TryGetValue(name, out MessageSource source);
            return source;
        }

        public void RemoveSource(string name)
        {
            if (_sources.TryGetValue(name, out MessageSource source))
            {
                source._refCount--;

                if (source._refCount == 0)
                {
                    _sources.TryRemove(name, out _);
                }
            }
        }

        public ulong NextId() => _idCounter++;
    }

    public class MessageSource
    {
        private readonly MessageSourceManager _manager;
        internal int _refCount;

        public event EventHandler MessageAvailable;
        private ConcurrentLinkedList<QueuedMessage> _messages = new();

        public MessageSource(MessageSourceManager manager)
        {
            _manager = manager;
            _refCount = 0;
        }

        public bool TryAddFront(OvlnMessage message, long tick)
        {
            _messages.AddFirst(new QueuedMessage { Message = message, Id = _manager.NextId(), Tick = tick });
            OnMessageAvailable();
            return true;
        }

        public bool TryAddBack(OvlnMessage message, long tick)
        {
            _messages.AddLast(new QueuedMessage { Message = message, Id = _manager.NextId(), Tick = tick });
            OnMessageAvailable();
            return true;
        }

        public bool TryPeek(out OvlnMessage message, out ulong id, out long tick)
        {
            if (_messages.TryPeekFirst(out var queuedMessage))
            {
                message = queuedMessage.Message;
                id = queuedMessage.Id;
                tick = queuedMessage.Tick;
                return true;
            }

            message = default;
            id = 0;
            tick = 0;
            return false;
        }

        public bool TryPop(out OvlnMessage message, out ulong id, out long tick)
        {
            if (_messages.TryTake(out var queuedMessage))
            {
                message = queuedMessage.Message;
                id = queuedMessage.Id;
                tick = queuedMessage.Tick;
                return true;
            }

            message = default;
            id = 0;
            tick = 0;
            return false;
        }

        public int Count => _messages.Count;

        private void OnMessageAvailable()
        {
            MessageAvailable?.Invoke(this, null);
        }

        private struct QueuedMessage
        {
            public OvlnMessage Message;
            public ulong Id;
            public long Tick;
        }
    }
}
