using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.Horizon.Sdk.OsTypes;

namespace Ryujinx.HLE.HOS.Applets
{
    internal class ProcessHolder : MultiWaitHolderOfHandle
    {
        public RealApplet Applet { get; private set; }
        public KProcess ProcessHandle { get; private set; }

        public ProcessHolder(RealApplet applet, KProcess kProcess, int processHandle)
            : base(processHandle)
        {
            Applet = applet;
            ProcessHandle = kProcess;
        }
    }
}
