using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Vi.Types
{

    [StructLayout(LayoutKind.Sequential, Size = 0x18)]
    public struct SharedMemorySlot
    {
        public ulong buffer_offset;
        public ulong size;
        public int width;
        public int height;
    };
}
