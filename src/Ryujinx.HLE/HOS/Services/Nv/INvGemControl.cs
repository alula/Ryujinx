using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Nv
{
    [Service("nvgem:c")]
    class INvGemControl : IpcService
    {
        private KEvent _event;
        private int _eventHandle;

        public INvGemControl(ServiceCtx context)
        {
            _event = new KEvent(context.Device.System.KernelContext);
        }

        [CommandCmif(0)]
        // Initialize() -> u32
        public ResultCode Initialize(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNv);
            context.ResponseData.Write((uint)0);

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        // GetEventHandle() -> handle<copy>
        public ResultCode GetEventHandle(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNv);
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

        [CommandCmif(2)]
        // ControlNotification(bool) -> u32
        public ResultCode ControlNotification(ServiceCtx context)
        {
            bool enable = context.RequestData.ReadBoolean();
            Logger.Stub?.PrintStub(LogClass.ServiceNv, new { enable });

            context.ResponseData.Write((uint)0);

            return ResultCode.Success;
        }

        [CommandCmif(3)]
        // SetNotificationPerm(u64, bool) -> u32
        public ResultCode SetNotificationPerm(ServiceCtx context)
        {
            ulong permission = context.RequestData.ReadUInt64();
            bool enable = context.RequestData.ReadBoolean();
            Logger.Stub?.PrintStub(LogClass.ServiceNv, new { permission, enable });

            context.ResponseData.Write((uint)0);

            return ResultCode.Success;
        }

        [CommandCmif(4)]
        // SetCoreDumpPerm(u64, bool) -> u32
        public ResultCode SetCoreDumpPerm(ServiceCtx context)
        {
            ulong permission = context.RequestData.ReadUInt64();
            bool enable = context.RequestData.ReadBoolean();
            Logger.Stub?.PrintStub(LogClass.ServiceNv, new { permission, enable });

            context.ResponseData.Write((uint)0);

            return ResultCode.Success;
        }

        [CommandCmif(5)]
        // GetAruid() -> (u64, u32)
        public ResultCode GetAruid(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNv);

            context.ResponseData.Write((ulong)0);
            context.ResponseData.Write((uint)0);

            return ResultCode.Success;
        }

        [CommandCmif(6)]
        // Reset() -> u32
        public ResultCode Reset(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNv);

            context.ResponseData.Write((uint)0);

            return ResultCode.Success;
        }

        [CommandCmif(7)]
        // GetAruid2() -> (u64, bool, u32)
        public ResultCode GetAruid2(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNv);

            context.ResponseData.Write((ulong)0);
            context.ResponseData.Write(false);
            context.ResponseData.Write((uint)0);

            return ResultCode.Success;
        }
    }
}
