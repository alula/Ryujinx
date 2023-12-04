using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class IApplicationAccessor : IpcService
    {
        private readonly KernelContext _kernelContext;
        private readonly ulong _applicationId;
        private readonly string _contentPath;

        private readonly KEvent _stateChangedEvent;
        private int _stateChangedEventHandle;

        public IApplicationAccessor(ulong applicationId, string contentPath, Horizon system)
        {
            _kernelContext = system.KernelContext;
            _applicationId = applicationId;
            _contentPath = contentPath;

            _stateChangedEvent = new KEvent(system.KernelContext);
        }


        [CommandCmif(0)]
        // GetAppletStateChangedEvent() -> handle<copy>
        public ResultCode GetAppletStateChangedEvent(ServiceCtx context)
        {
            if (_stateChangedEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_stateChangedEvent.ReadableEvent, out _stateChangedEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_stateChangedEventHandle);

            return ResultCode.Success;
        }


        [CommandCmif(10)]
        // Start()
        public ResultCode Start(ServiceCtx context)
        {
            _stateChangedEvent.ReadableEvent.Signal();

            Logger.Debug?.Print(LogClass.ServiceAm, $"Application 0x{_applicationId:X} start requested.");
            
            context.Device.LoadNca(_contentPath);

            return ResultCode.Success;
        }

        [CommandCmif(101)]
        // RequestForApplicationToGetForeground()
        public ResultCode RequestForApplicationToGetForeground(ServiceCtx context)
        {
            // _stateChangedEvent.ReadableEvent.Signal();
            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }
    }
}
