using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.LibraryAppletProxy
{
    class AppletStandalone
    {
        public AppletId AppletId;
        public LibraryAppletMode LibraryAppletMode;
        public LinkedList<byte[]> InputData;

        public AppletStandalone()
        {
            InputData = new LinkedList<byte[]>();
        }
    }
}
