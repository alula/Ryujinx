using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class IGlobalStateController : IpcService
    {
        private KEvent _hdcpAuthenticationFailedEvent;
        private int    _hdcpAuthenticationFailedEventHandle;

        public IGlobalStateController(ServiceCtx context)
        {
            _hdcpAuthenticationFailedEvent = new KEvent(context.Device.System.KernelContext);
        }

        [CommandCmif(15)]
        // GetHdcpAuthenticationFailedEvent() -> handle<copy>
        public ResultCode GetHdcpAuthenticationFailedEvent(ServiceCtx context)
        {
            if (_hdcpAuthenticationFailedEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_hdcpAuthenticationFailedEvent.ReadableEvent, out _hdcpAuthenticationFailedEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_hdcpAuthenticationFailedEventHandle);

            return ResultCode.Success;
        }
    }
}
