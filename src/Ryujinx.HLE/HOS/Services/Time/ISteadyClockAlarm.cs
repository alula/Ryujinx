using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Time
{
    class ISteadyClockAlarm : IpcService
    {
        private readonly KEvent _alarmEvent;
        private int _alarmEventHandle;

        public ISteadyClockAlarm(ServiceCtx context)
        {
            _alarmEvent = new KEvent(context.Device.System.KernelContext);
        }

        [CommandCmif(0)]
        // GetAlarmEvent() -> handle<copy>
        public ResultCode GetAlarmEvent(ServiceCtx context)
        {
            if (_alarmEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_alarmEvent.ReadableEvent, out _alarmEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_alarmEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        // Enable(u64)
        public ResultCode Enable(ServiceCtx context)
        {
            ulong alarmTime = context.RequestData.ReadUInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceTime, new { alarmTime });

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // Disable()
        public ResultCode Disable(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceTime);

            return ResultCode.Success;
        }

        [CommandCmif(3)]
        // IsEnabled() -> bool
        public ResultCode IsEnabled(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceTime);

            context.ResponseData.Write(false);

            return ResultCode.Success;
        }
    }
}
