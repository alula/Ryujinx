using Ryujinx.HLE.Utilities;
using System;
using System.IO;

namespace Ryujinx.HLE.FileSystem
{
    public class SystemVersion
    {
        public byte Major { get; }
        public byte Minor { get; }
        public byte Micro { get; }
        public byte RevisionMajor { get; }
        public byte RevisionMinor { get; }
        public string PlatformString { get; }
        public string Hex { get; }
        public string VersionString { get; }
        public string VersionTitle { get; }

        public SystemVersion(Stream systemVersionFile)
        {
            using BinaryReader reader = new(systemVersionFile);
            Major = reader.ReadByte();
            Minor = reader.ReadByte();
            Micro = reader.ReadByte();

            reader.ReadByte(); // Padding

            RevisionMajor = reader.ReadByte();
            RevisionMinor = reader.ReadByte();

            reader.ReadBytes(2); // Padding

            PlatformString = StringUtils.ReadInlinedAsciiString(reader, 0x20);
            Hex = StringUtils.ReadInlinedAsciiString(reader, 0x40);
            VersionString = StringUtils.ReadInlinedAsciiString(reader, 0x18);
            VersionTitle = StringUtils.ReadInlinedAsciiString(reader, 0x80);
        }

        public byte GetExpectedFuseCount(bool isRetail)
        {
            if (!isRetail)
            {
                return (byte)(Major >= 3 ? 1 : 0);
            }

            return (byte)(Major switch
            {
                1 => 1,
                2 => 2,
                3 => Micro switch
                {
                    0 => 3,
                    _ => 4
                },
                4 => 5,
                5 => 6,
                6 => Minor <= 1 ? 7 : 8,
                7 => 9,
                8 => Minor < 1 ? 9 : 10,
                9 => Minor < 1 ? 11 : 12,
                10 => 13,
                11 => 14,
                12 => Micro < 2 ? 14 : 15,
                13 => Minor < 2 ? 15 : Micro < 1 ? 15 : 16,
                14 => 16,
                15 => 17,
                16 => 18,
                17 => 19,
                _ => 0
            });
        }
    }
}
