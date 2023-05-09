using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Settings.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct SleepSettings
    {
        public uint              Flags;
        public HandheldSleepPlan HandheldSleepPlan;
        public ConsoleSleepPlan  ConsoleSleepPlan;
    }
}