﻿using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Sdk.Ovln
{
    interface ISenderService : IServiceObject
    {
        Result OpenSender(out ISender service);
    }
}
