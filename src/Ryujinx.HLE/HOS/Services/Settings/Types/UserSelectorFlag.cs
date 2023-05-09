using System;

namespace Ryujinx.HLE.HOS.Services.Settings.Types
{
    [Flags]
    enum UserSelectorFlag : uint
    {
        None              = 0,
        SkipsIfSingleUser = 1 << 0,
        Unknown           = 1U << 31
    }
}