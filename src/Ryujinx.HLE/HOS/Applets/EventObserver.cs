using Ryujinx.HLE.HOS.Applets.Browser;
using Ryujinx.HLE.HOS.Applets.Error;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Services.Am;
using Ryujinx.HLE.HOS.Services.Am.AppletAE;
using Ryujinx.HLE.HOS.SystemState;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.HLE.HOS.Applets
{
    public class EventObserver
    {
        private Horizon _system;
        private WindowSystem _windowSystem;

        // Processing thread
        readonly Thread _thread;
        readonly CancellationTokenSource _cts = new();

        public EventObserver(Horizon system, WindowSystem windowSystem)
        {
            _system = system;
            _windowSystem = windowSystem;

            _windowSystem.SetEventObserver(this);

            _thread = new Thread(ThreadFunc)
            {
                Name = "am:EventObserver",
                IsBackground = true
            };
        }

        public void Dispose()
        {
            _cts.Cancel();
        }

        // private MultiWaitHolderBase WaitSignaled() {
        //     while (true)
        //     {
        //         if (_cts.Token.IsCancellationRequested)
        //         {
        //             return null;
        //         }
        //     }
        // }

        private void ThreadFunc()
        {
            
        }
    }
}
