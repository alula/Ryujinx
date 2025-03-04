using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;

namespace Ryujinx.Horizon.Sdk.Ovln
{
    [StructLayout(LayoutKind.Sequential)]
    public struct OvlnMessageFlags
    {
        public bool PushToBack;
        public byte QueueType;
        public Array6<byte> Padding;

        public override string ToString()
        {
            return $"PushToBack: {PushToBack}, QueueType: {QueueType}";
        }
    }
}
