using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Settings.Types
{
    [StructLayout(LayoutKind.Sequential)]
    struct UserSelectorSettings
    {
        public UserSelectorFlag UserSelectorFlag;
    }
}