using Ryujinx.Horizon.Sdk.OsTypes.Impl;
using System.Collections.Generic;

namespace Ryujinx.Horizon.Sdk.OsTypes
{
    public class MultiWaitHolderOfInterProcessEvent : MultiWaitHolder
    {
        private readonly InterProcessEventType _event;
        private LinkedListNode<MultiWaitHolderBase> _node;

        public override TriBool Signaled
        {
            get
            {
                return TriBool.Undefined;
            }
        }

        public override int Handle => _event.ReadableHandle;

        public MultiWaitHolderOfInterProcessEvent(InterProcessEventType evnt)
        {
            _event = evnt;
        }
    }
}
