using Ryujinx.HLE.HOS.Services.Time.Clock;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Settings.Types
{
    [StructLayout(LayoutKind.Sequential)]
    struct EulaVersion
    {
        public uint Version;
        public uint RegionCode;
        public uint ClockType;
        public uint Reserved;
        public long NetworkSystemClock;
        public SteadyClockTimePoint SteadyClock;
    }
}
