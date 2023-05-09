using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Settings.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct NotificationTime
    {
        public uint Hour;
        public uint Minute;
    }
}