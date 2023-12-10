using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Account.Acc.AccountService
{
    class INotifier : IpcService
    {
        public KEvent SystemEvent;

        public INotifier(ServiceCtx context, KEvent systemEvent)
        {
            SystemEvent = systemEvent;
        }

        [CommandCmif(0)]
        // GetSystemEvent() -> handle<copy>
        public ResultCode GetSystemEvent(ServiceCtx context)
        {
            if (context.Process.HandleTable.GenerateHandle(SystemEvent.ReadableEvent, out int systemEventHandle) != Result.Success)
            {
                throw new InvalidOperationException("Out of handles!");
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(systemEventHandle);

            return ResultCode.Success;
        }
    }
}
