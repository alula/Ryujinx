using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Vi
{
    [StructLayout(LayoutKind.Sequential, Size = 0x8, Pack = 0x1)]
    struct ProxyName
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x8)]
        public byte[] Unknown;
    }
}
