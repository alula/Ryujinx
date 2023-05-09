using Ryujinx.HLE.HOS.Services.Time.Clock;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Settings.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    struct InitialLaunchSettings
    {
        public InitialLaunchFlag    Flags;
        public uint                 Reserved;
        public SteadyClockTimePoint TimeStamp;
    }
}