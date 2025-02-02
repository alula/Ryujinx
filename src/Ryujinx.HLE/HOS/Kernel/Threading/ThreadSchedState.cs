using System;

namespace Ryujinx.HLE.HOS.Kernel.Threading
{
    [Flags]
    enum ThreadSchedState : ushort
    {
        LowMask = 0xf,
        HighMask = 0xfff0,
        ForcePauseMask = 0x1f0,

        ProcessPauseFlag = 1 << 4,
        ThreadPauseFlag = 1 << 5,
        ProcessDebugPauseFlag = 1 << 6,
        BacktracePauseFlag = 1 << 7,
        KernelInitPauseFlag = 1 << 8,

        None = 0,
        Paused = 1,
        Running = 2,
        TerminationPending = 3,
    }

    static class ThreadSchedStateExtensions
    {
        public static string StateString(this ThreadSchedState state)
        {
            var stateValue = state & ThreadSchedState.LowMask;
            return (stateValue) switch
            {
                ThreadSchedState.None => "None",
                ThreadSchedState.Paused => "Paused",
                ThreadSchedState.Running => "Running",
                ThreadSchedState.TerminationPending => "TerminationPending",
                _ => ((int)stateValue).ToString(),
            };
        }
    }
}
