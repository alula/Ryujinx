using Ryujinx.HLE.HOS.Services.Am.AppletAE;
using Ryujinx.HLE.UI;
using Ryujinx.Memory;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Applets
{
    interface IApplet
    {
        event EventHandler AppletStateChanged;

        ResultCode Start(AppletChannel inChannel,
                         AppletChannel outChannel,
                         AppletChannel interactiveInChannel,
                         AppletChannel interactiveOutChannel,
                         AppletChannel contextChannel);

        ResultCode GetResult();

        bool DrawTo(RenderingSurfaceInfo surfaceInfo, IVirtualMemoryManager destination, ulong position)
        {
            return false;
        }

        static T ReadStruct<T>(ReadOnlySpan<byte> data) where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(data)[0];
        }
    }
}
