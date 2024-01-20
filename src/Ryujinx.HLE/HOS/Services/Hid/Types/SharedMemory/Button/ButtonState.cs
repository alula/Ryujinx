using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Common;
using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Button
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct ButtonState : ISampledDataStruct
    {
        public ulong SamplingNumber;
        public ulong Buttons;
    }
}
