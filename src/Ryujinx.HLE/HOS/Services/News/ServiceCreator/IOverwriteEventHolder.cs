using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.News.ServiceCreator
{
    class IOverwriteEventHolder : IpcService
    {
        private KEvent _event;
        private int   _eventHandle;

        public IOverwriteEventHolder(ServiceCtx context)
        {
            _event = new KEvent(context.Device.System.KernelContext);
        }

        [CommandCmif(0)]
        // Get() -> handle<copy>
        public ResultCode Get(ServiceCtx context)
        {
            if (_eventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_event.ReadableEvent, out _eventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_eventHandle);

            return ResultCode.Success;
        }
    }
}