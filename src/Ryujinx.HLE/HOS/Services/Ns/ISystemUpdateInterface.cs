using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Ns
{
    [Service("ns:su")]
    class ISystemUpdateInterface : IpcService
    {
        private KEvent _systemUpdateNotificationEvent;
        private int    _systemUpdateNotificationEventHandle;

        public ISystemUpdateInterface(ServiceCtx context)
        {
            _systemUpdateNotificationEvent = new KEvent(context.Device.System.KernelContext);
        }

        [CommandCmif(9)]
        // GetSystemUpdateNotificationEventForContentDelivery() -> handle<copy>
        public ResultCode GetSystemUpdateNotificationEventForContentDelivery(ServiceCtx context)
        {
            if (_systemUpdateNotificationEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_systemUpdateNotificationEvent.ReadableEvent, out _systemUpdateNotificationEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_systemUpdateNotificationEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(10)]
        // NotifySystemUpdateForContentDelivery()
        public ResultCode NotifySystemUpdateForContentDelivery(ServiceCtx context)
        {
            _systemUpdateNotificationEvent.ReadableEvent.Signal();

            return ResultCode.Success;
        }
    }
}
