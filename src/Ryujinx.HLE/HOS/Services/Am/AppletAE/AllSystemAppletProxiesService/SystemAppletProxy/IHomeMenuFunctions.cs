using LibHac.Util;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class IHomeMenuFunctions : IpcService
    {
        private int _channelEventHandle;

        public IHomeMenuFunctions(Horizon system)
        {
        }

        [CommandCmif(10)]
        // RequestToGetForeground()
        public ResultCode RequestToGetForeground(ServiceCtx context)
        {
            // Logger.Stub?.PrintStub(LogClass.ServiceAm);
            context.Device.System.WindowSystem.RequestHomeMenuToGetForeground();

            return ResultCode.Success;
        }

        [CommandCmif(11)]
        // LockForeground()
        public ResultCode LockForeground(ServiceCtx context)
        {
            // Logger.Stub?.PrintStub(LogClass.ServiceAm);
            context.Device.System.WindowSystem.RequestLockHomeMenuIntoForeground();

            return ResultCode.Success;
        }

        [CommandCmif(12)]
        // UnlockForeground()
        public ResultCode UnlockForeground(ServiceCtx context)
        {
            // Logger.Stub?.PrintStub(LogClass.ServiceAm);
            context.Device.System.WindowSystem.RequestUnlockHomeMenuFromForeground();

            return ResultCode.Success;
        }

        [CommandCmif(20)]
        // PopFromGeneralChannel() -> object<nn::am::service::IStorage>
        public ResultCode PopFromGeneralChannel(ServiceCtx context)
        {
            bool pop = context.Device.System.GeneralChannelData.TryDequeue(out byte[] data);
            if (!pop)
            {
                return ResultCode.NotAvailable;
            }

            // Logger.Debug?.Print(LogClass.ServiceAm, $"Data size: {data.Length}");
            Logger.Info?.Print(LogClass.ServiceAm, $"GeneralChannel data: {data.ToHexString()}");

            MakeObject(context, new IStorage(data));

            return ResultCode.Success;
        }

        [CommandCmif(21)]
        // GetPopFromGeneralChannelEvent() -> handle<copy>
        public ResultCode GetPopFromGeneralChannelEvent(ServiceCtx context)
        {
            if (_channelEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(
                    context.Device.System.GeneralChannelEvent.ReadableEvent,
                    out _channelEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_channelEventHandle);

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(31)]
        // GetWriterLockAccessorEx(i32) -> object<nn::am::service::ILockAccessor>
        public ResultCode GetWriterLockAccessorEx(ServiceCtx context)
        {
            int lockId = context.RequestData.ReadInt32();

            MakeObject(context, new ILockAccessor(lockId, context.Device.System));

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }
    }
}
