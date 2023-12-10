using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Ovln
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SourceName {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] Unknown;
    }
}
