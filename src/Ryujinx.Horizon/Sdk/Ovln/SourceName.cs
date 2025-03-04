using Ryujinx.Common.Memory;
using System.Runtime.InteropServices;
using System.Text;

namespace Ryujinx.Horizon.Sdk.Ovln
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SourceName
    {
        Array16<byte> StringData;

        public string GetString()
        {
            int i;
            for (i = 0; i < StringData.Length; i++)
            {
                if (StringData[i] == 0)
                {
                    break;
                }
            }

            return Encoding.ASCII.GetString(StringData.AsSpan().Slice(0, i));
        }
    }
}
