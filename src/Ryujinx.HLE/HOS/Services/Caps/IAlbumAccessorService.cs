using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Caps
{
    [Service("caps:a")]
    class IAlbumAccessorService : IpcService
    {
        public IAlbumAccessorService(ServiceCtx context) { }

        [CommandCmif(18)]
        // GetAppletProgramIdTable(buffer<unknown, 70>) -> bool
        public ResultCode GetAppletProgramIdTable(ServiceCtx context)
        {
            ulong tableBufPos = context.Request.ReceiveBuff[0].Position;
            ulong tableBufSize = context.Request.ReceiveBuff[0].Size;

            if (tableBufPos == 0)
            {
                return ResultCode.NullOutputBuffer;
            }

            context.Memory.Write(tableBufPos, 0x0100000000001000UL);
            context.Memory.Write(tableBufPos + 8, 0x0100000000001fffUL);

            context.ResponseData.Write(true);

            return ResultCode.Success;
        }
    }
}
