using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Hid.HidServer;
using Ryujinx.HLE.HOS.Services.Hid.Types;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    [Service("hid:sys")]
    class IHidSystemServer : IpcService
    {
        private KEvent _joyDetachOnBluetoothOffEvent;
        private int    _joyDetachOnBluetoothOffEventHandle;

        private IHidServer _hidServer;

        public IHidSystemServer(ServiceCtx context)
        {
            _joyDetachOnBluetoothOffEvent = new KEvent(context.Device.System.KernelContext);
            _hidServer                    = new IHidServer(context);
        }

        [CommandCmif(301)]
        // ActivateNpadSystem(u32)
        public ResultCode ActivateNpadSystem(ServiceCtx context)
        {
            uint npadSystem = context.RequestData.ReadUInt32();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { npadSystem });

            return ResultCode.Success;
        }

        [CommandCmif(303)]
        // ApplyNpadSystemCommonPolicy(u64)
        public ResultCode ApplyNpadSystemCommonPolicy(ServiceCtx context)
        {
            ulong commonPolicy = context.RequestData.ReadUInt64();

            Logger.Stub?.PrintStub(LogClass.ServiceHid, new { commonPolicy });

            return ResultCode.Success;
        }

        [CommandCmif(306)]
        // GetLastActiveNpad(u32) -> u8, u8
        public ResultCode GetLastActiveNpad(ServiceCtx context)
        {
            // TODO: RequestData seems to have garbage data, reading an extra uint seems to fix the issue.
            context.RequestData.ReadUInt32();

            ResultCode resultCode = GetAppletFooterUiTypeImpl(context, out AppletFooterUiType appletFooterUiType);

            context.ResponseData.Write((byte)appletFooterUiType);
            context.ResponseData.Write((byte)0);

            return resultCode;
        }

        [CommandCmif(307)]
        // GetNpadSystemExtStyle() -> u64
        public ResultCode GetNpadSystemExtStyle(ServiceCtx context)
        {
            foreach (PlayerIndex playerIndex in context.Device.Hid.Npads.GetSupportedPlayers())
            {
                if (HidUtils.GetNpadIdTypeFromIndex(playerIndex) > NpadIdType.Handheld)
                {
                    return ResultCode.InvalidNpadIdType;
                }
            }

            context.ResponseData.Write((ulong)context.Device.Hid.Npads.SupportedStyleSets);

            return ResultCode.Success;
        }

        [CommandCmif(314)] // 9.0.0+
        // GetAppletFooterUiType(u32) -> u8
        public ResultCode GetAppletFooterUiType(ServiceCtx context)
        {
            ResultCode resultCode = GetAppletFooterUiTypeImpl(context, out AppletFooterUiType appletFooterUiType);

            context.ResponseData.Write((byte)appletFooterUiType);

            return resultCode;
        }

        [CommandCmif(321)]
        // GetUniquePadsFromNpad(u32) -> (u64, buffer<nn::hid::system::UniquePadId[], 0xa>)
        public ResultCode GetUniquePadsFromNpad(ServiceCtx context)
        {
            context.ResponseData.Write(0L);

            return ResultCode.Success;
        }

        [CommandCmif(703)]
        // GetUniquePadIds() -> (u64, buffer<nn::hid::system::UniquePadId[], 0xa>)
        public ResultCode GetUniquePadIds(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceHid);

            context.ResponseData.Write(0L);

            return ResultCode.Success;
        }

        [CommandCmif(751)]
        // AcquireJoyDetachOnBluetoothOffEventHandle(nn::applet::AppletResourceUserId, pid) -> handle<copy>
        public ResultCode AcquireJoyDetachOnBluetoothOffEventHandle(ServiceCtx context)
        {
            if (_joyDetachOnBluetoothOffEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_joyDetachOnBluetoothOffEvent.ReadableEvent, out _joyDetachOnBluetoothOffEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_joyDetachOnBluetoothOffEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(850)]
        // IsUsbFullKeyControllerEnabled() -> bool
        public ResultCode IsUsbFullKeyControllerEnabled(ServiceCtx context)
        {
            context.ResponseData.Write(false);

            Logger.Stub?.PrintStub(LogClass.ServiceHid);

            return ResultCode.Success;
        }

        [CommandCmif(1153)]
        // GetTouchScreenDefaultConfiguration() -> unknown
        public ResultCode GetTouchScreenDefaultConfiguration(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceHid);

            return ResultCode.Success;
        }

        private ResultCode GetAppletFooterUiTypeImpl(ServiceCtx context, out AppletFooterUiType appletFooterUiType)
        {
            NpadIdType npadIdType = (NpadIdType)context.RequestData.ReadUInt32();
            PlayerIndex playerIndex = HidUtils.GetIndexFromNpadIdType(npadIdType);

            appletFooterUiType = context.Device.Hid.SharedMemory.Npads[(int)playerIndex].InternalState.AppletFooterUiType;

            return ResultCode.Success;
        }
    }
}
