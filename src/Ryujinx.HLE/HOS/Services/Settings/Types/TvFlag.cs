using System;

namespace Ryujinx.HLE.HOS.Services.Settings.Types
{
    [Flags]
    enum TvFlag : uint
    {
        None                    = 0,
        Allows4k                = 1 << 0,
        Allows3d                = 1 << 1,
        AllowsCec               = 1 << 2,
        PreventsScreenBurnIn    = 1 << 3
    }
}