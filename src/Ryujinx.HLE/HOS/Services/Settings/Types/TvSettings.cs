using System.Runtime.InteropServices;

namespace Ryujinx.HLE.HOS.Services.Settings.Types
{
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct TvSettings
    {
        public TvFlag          Flags;
        public TvResolution    TvResolution;
        public HdmiContentType HdmiContentType;
        public RgbRange        RgbRange;
        public CmuMode         CmuMode;
        public uint            TvUnderscan;
        public uint            TvGamma;
        public uint            ContrastRatio;
    }
}