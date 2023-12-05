using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Caps
{
    [Service("caps:a")]
    class IAlbumAccessorService : IpcService
    {
        public IAlbumAccessorService(ServiceCtx context) { }

        [CommandCmif(1)]
        // GetAlbumFileList(unknown<1>) -> (fileCount?, buffer<unknown, 6>)
        public ResultCode GetAlbumFileList(ServiceCtx context)
        {
            context.ResponseData.Write(0);

            return ResultCode.Success;
        }

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

        [CommandCmif(5)]
        // IsAlbumMounted(unknown<1>) -> bool
        public ResultCode IsAlbumMounted(ServiceCtx context)
        {
            context.ResponseData.Write(true);

            return ResultCode.Success;
        }

        [CommandCmif(401)]
        // GetAutoSavingStorage() -> bool
        public ResultCode GetAutoSavingStorage(ServiceCtx context)
        {
            context.ResponseData.Write(true);

            return ResultCode.Success;
        }
    }
}
