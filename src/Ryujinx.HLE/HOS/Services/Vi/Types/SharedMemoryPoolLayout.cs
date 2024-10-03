using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Vi.Types
{

    [StructLayout(LayoutKind.Sequential, Size = 0x188)]
    public struct SharedMemoryPoolLayout
    {
        public int NumSlots;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public SharedMemorySlot[] Slots;
    };
}
