using System.Collections.Generic;

namespace Ryujinx.Horizon.Sdk.Sf
{
    interface IServiceObject
    {
    }

    interface IServiceObjectCommandHandlers : IServiceObject
    {
        IReadOnlyDictionary<int, CommandHandler> GetCommandHandlers();
    }
}
