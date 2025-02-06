using Ryujinx.Horizon.Common;
using System;
using System.Collections.Generic;

namespace Ryujinx.Horizon.Sdk.Sf.Cmif
{
    class ServiceDispatchTable : ServiceDispatchTableBase
    {
        private readonly string _objectName;
        private readonly IReadOnlyDictionary<int, CommandHandler> _entries;

        public ServiceDispatchTable(string objectName, IReadOnlyDictionary<int, CommandHandler> entries)
        {
            _objectName = objectName;
            _entries = entries;
        }

        public override Result ProcessMessage(ref ServiceDispatchContext context, ReadOnlySpan<byte> inRawData)
        {
            return ProcessMessageImpl(ref context, inRawData, _entries, _objectName);
        }

        public static ServiceDispatchTableBase Create(IServiceObject instance)
        {
            if (instance is DomainServiceObject)
            {
                return new DomainServiceObjectDispatchTable();
            }

            if (instance is IServiceObjectCommandHandlers serviceInstance)
            {
                return new ServiceDispatchTable(serviceInstance.GetType().Name, serviceInstance.GetCommandHandlers());
            }

            throw new ArgumentException($"Invalid service object type: {instance.GetType().Name}");
        }
    }
}
