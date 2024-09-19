using Ryujinx.HLE.HOS.Applets.Browser;
using Ryujinx.HLE.HOS.Applets.Error;
using Ryujinx.HLE.HOS.Services.Am.AppletAE;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Applets
{
    public class WindowSystem
    {
        private Horizon _system;

        public WindowSystem(Horizon system)
        {
            _system = system;
        }
    }
}
