using System;

namespace Ryujinx.HLE.HOS.Services.Settings.Types
{
    [Flags]
    enum InitialLaunchFlag : uint
    {
        None                          = 0,
        InitialLaunchCompletionFlag   = 1 << 0,
        InitialLaunchUserAdditionFlag = 1 << 8,
        InitialLaunchTimestampFlag    = 1 << 16
    }
}