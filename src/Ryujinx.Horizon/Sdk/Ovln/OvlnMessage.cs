using Ryujinx.Common.Memory;
using System;
using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Ovln
{
    [StructLayout(LayoutKind.Sequential)]
    public struct OvlnMessage
    {
        public uint Magic;
        public uint Size;
        public Array120<byte> Data;

        public Span<byte> GetDataSpan()
        {
            return MemoryMarshal.CreateSpan(ref Data[0], (int)Size);
        }
    }
}
