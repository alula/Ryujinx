namespace Ryujinx.HLE.HOS.Services.Ns.Types
{
    enum ApplicationRecordType : byte
    {
        // TODO: confirm this
        Installing = 0x2,
        // Also GameCardInserted
        Installed = 0x3,
        GameCardNotInserted = 0x5,
        Archived = 0xB
    }
}
