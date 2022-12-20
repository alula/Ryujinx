using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Vi.RootService.ApplicationDisplayService
{
    class ISystemDisplayService : IpcService
    {
#pragma warning disable IDE0052 // Remove unread private member
        private readonly IApplicationDisplayService _applicationDisplayService;
#pragma warning restore IDE0052

        private KEvent _sharedFramebufferAcquirableEvent;
        private int _sharedFramebufferAcquirableEventHandle;

        public ISystemDisplayService(IApplicationDisplayService applicationDisplayService)
        {
            _applicationDisplayService = applicationDisplayService;
        }

        [CommandCmif(2205)]
        // SetLayerZ(u64, u64)
        public ResultCode SetLayerZ(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceVi);

            return ResultCode.Success;
        }

        [CommandCmif(2207)]
        // SetLayerVisibility(b8, u64)
        public ResultCode SetLayerVisibility(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceVi);

            return ResultCode.Success;
        }

        [CommandCmif(2312)] // 1.0.0-6.2.0
        // CreateStrayLayer(u32, u64) -> (u64, u64, buffer<bytes, 6>)
        public ResultCode CreateStrayLayer(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceVi);

            return _applicationDisplayService.CreateStrayLayer(context);
        }

        [CommandCmif(3200)]
        // GetDisplayMode(u64) -> nn::vi::DisplayModeInfo
        public ResultCode GetDisplayMode(ServiceCtx context)
        {
            ulong displayId = context.RequestData.ReadUInt64();

            (ulong width, ulong height) = AndroidSurfaceComposerClient.GetDisplayInfo(context, displayId);

            context.ResponseData.Write((uint)width);
            context.ResponseData.Write((uint)height);
            context.ResponseData.Write(60.0f);
            context.ResponseData.Write(0);

            Logger.Stub?.PrintStub(LogClass.ServiceVi);

            return ResultCode.Success;
        }

        [CommandCmif(8225)] // 4.0.0+
        // GetSharedBufferMemoryHandleId()
        public ResultCode GetSharedBufferMemoryHandleId(ServiceCtx context)
        {
            context.ResponseData.Write((ulong)context.Device.System.ViServerS.GetSharedBufferNvMapId());
            context.ResponseData.Write(context.Device.System.ViServerS.GetSharedBufferSize());

            (ulong mapAddress, ulong mapSize) = context.Request.GetBufferType0x22();

            context.Memory.Write(mapAddress, context.Device.System.ViServerS.GetSharedBufferMap());

            return ResultCode.Success;
        }

        [CommandCmif(8254)] // 4.0.0+
        // AcquireSharedFrameBuffer()
        public ResultCode AcquireSharedFrameBuffer(ServiceCtx context)
        {
            // 36 bytes
            context.ResponseData.Write(0UL);
            context.ResponseData.Write(0UL);
            context.ResponseData.Write(0UL);
            context.ResponseData.Write(0UL);
            context.ResponseData.Write(0);

            // 16 bytes + 4 bytes
            context.ResponseData.Write(0UL);
            context.ResponseData.Write(0UL);
            context.ResponseData.Write(0); // Padding

            context.ResponseData.Write(0UL); // MemoryHandleId (>= 0 and < 16)

            return ResultCode.Success;
        }

        [CommandCmif(8255)] // 4.0.0+
        // PresentSharedFrameBuffer()
        public ResultCode PresentSharedFrameBuffer(ServiceCtx context)
        {
            uint unknwon0 = context.RequestData.ReadUInt32();
            uint unknwon4 = context.RequestData.ReadUInt32();
            ulong fenceValue = context.RequestData.ReadUInt64(); // ?
            uint unknwon10 = context.RequestData.ReadUInt32();
            uint unknwon14 = context.RequestData.ReadUInt32();
            uint unknwon18 = context.RequestData.ReadUInt32();
            uint unknwon1C = context.RequestData.ReadUInt32();
            uint unknwon20 = context.RequestData.ReadUInt32();
            uint unknwon24 = context.RequestData.ReadUInt32();
            uint unknwon28 = context.RequestData.ReadUInt32();
            uint unknwon2C = context.RequestData.ReadUInt32();
            uint unknwon30 = context.RequestData.ReadUInt32();
            uint transformFlags = context.RequestData.ReadUInt32(); // ?
            uint unknwon38 = context.RequestData.ReadUInt32();
            uint unknwon3C = context.RequestData.ReadUInt32();

            bool flipX = (transformFlags & 1u) != 0;
            bool flipY = (transformFlags & 2u) != 0;

            context.Device.System.ViServerS.PresentFramebuffer(context.Device.Gpu, flipX, flipY);
            context.Device.Statistics.RecordGameFrameTime();

            return ResultCode.Success;
        }

        [CommandCmif(8256)] // 4.0.0+
        // GetSharedFrameBufferAcquirableEvent()
        public ResultCode GetSharedFrameBufferAcquirableEvent(ServiceCtx context)
        {
            if (_sharedFramebufferAcquirableEventHandle == 0)
            {
                _sharedFramebufferAcquirableEvent = new KEvent(context.Device.System.KernelContext);
                _sharedFramebufferAcquirableEvent.WritableEvent.Signal();

                if (context.Process.HandleTable.GenerateHandle(_sharedFramebufferAcquirableEvent.ReadableEvent, out _sharedFramebufferAcquirableEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_sharedFramebufferAcquirableEventHandle);

            return ResultCode.Success;
        }
    }
}
