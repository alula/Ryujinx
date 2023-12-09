using LibHac;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ns.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x50)]
    struct ApplicationView
    {
        public ApplicationId AppId;
        public uint Unknown1;
        public uint Flags;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x40)]
        public byte[] Unknown2;
    }
}
