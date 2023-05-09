using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Settings.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    struct NotificationSettings
    {
        public NotificationFlag   Flags;
        public NotificationVolume Volume;
        public NotificationTime   HeadTime;
        public NotificationTime   TailTime;
    }
}