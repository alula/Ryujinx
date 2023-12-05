using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
using System;
using static Microsoft.IO.RecyclableMemoryStreamManager;

namespace Ryujinx.HLE.HOS.Services.Nifm.StaticService
{
    class IScanRequest : IpcService
    {
        private readonly KEvent _systemEvent;
        private int _systemEventHandle;

        public IScanRequest(Horizon system)
        {
            _systemEvent = new KEvent(system.KernelContext);
        }

        [CommandCmif(3)]
        // GetSystemEventReadableHandle() -> (handle<copy>)
        public ResultCode GetSystemEventReadableHandle(ServiceCtx context)
        {
            if (_systemEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_systemEvent.ReadableEvent, out _systemEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_systemEventHandle);

            return ResultCode.Success;
        }
    }
}
