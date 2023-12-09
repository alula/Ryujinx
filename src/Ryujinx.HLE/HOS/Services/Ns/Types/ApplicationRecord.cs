using LibHac;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Ns.Types
{
    [StructLayout(LayoutKind.Sequential, Size = 0x18)]
    struct ApplicationRecord
    {
        public ApplicationId AppId;
        public ApplicationRecordType Type;
        public byte Unknown1;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] Unknown2;
        public byte Unknown3;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public byte[] Unknown4;
    }
}
