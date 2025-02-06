using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE
{
    class ILockAccessor : IpcService
    {
        private readonly int _lockId;
        private bool _isLocked;
        private KEvent _lockEvent;
        private int _lockEventHandle;

        public ILockAccessor(int lockId, Horizon system)
        {
            _lockId = lockId;
            _isLocked = false;
            _lockEvent = new KEvent(system.KernelContext);
            _lockEvent.ReadableEvent.Signal();
        }

        [CommandCmif(1)]
        // TryLock(bool) -> (bool, IpcService)
        public ResultCode TryLock(ServiceCtx context)
        {
            bool returnHandle = context.RequestData.ReadBoolean();

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            if (_lockEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_lockEvent.ReadableEvent, out _lockEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            _isLocked = true;
            _lockEvent.ReadableEvent.Signal();

            context.ResponseData.Write(_isLocked);
            if (returnHandle)
            {
                context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_lockEventHandle);
            }

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // Unlock()
        public ResultCode Unlock(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            _isLocked = false;
            _lockEvent.ReadableEvent.Signal();

            return ResultCode.Success;
        }

        [CommandCmif(3)]
        // GetEvent() -> handle<copy>
        public ResultCode GetEvent(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            if (_lockEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_lockEvent.ReadableEvent, out _lockEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_lockEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(4)]
        // IsLocked() -> bool
        public ResultCode IsLocked(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            context.ResponseData.Write(_isLocked);

            return ResultCode.Success;
        }
    }
};
