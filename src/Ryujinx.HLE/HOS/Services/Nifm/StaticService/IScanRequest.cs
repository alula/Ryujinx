using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
using System;

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

        [CommandCmif(0)]
        // Submit()
        public ResultCode Submit(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNifm);
            _systemEvent.ReadableEvent.Signal();

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        // IsProcessing() -> bool
        public ResultCode IsProcessing(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNifm);
            context.ResponseData.Write(false);

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // GetResult() -> u32
        public ResultCode GetResult(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNifm);
            context.ResponseData.Write(0);

            return ResultCode.Success;
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
