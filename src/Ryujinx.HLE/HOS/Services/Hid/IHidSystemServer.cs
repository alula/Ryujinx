﻿using Ryujinx.Common.Logging;
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
        private KEvent _deviceRegisteredEventForControllerSupport;
        private int _deviceRegisteredEventForControllerSupportHandle;

        private KEvent _connectionTriggerTimeoutEvent;
        private int _connectionTriggerTimeoutEventHandle;

        private KEvent _joyDetachOnBluetoothOffEvent;
        private int _joyDetachOnBluetoothOffEventHandle;

        private KEvent _playReportControllerUsageUpdateEvent;
        private int _playReportControllerUsageUpdateEventHandle;

        private KEvent _playReportRegisteredDeviceUpdateEvent;
        private int _playReportRegisteredDeviceUpdateEventHandle;

        private IHidServer _hidServer;

        private int _homeButtonEventHandle;
        private int _sleepButtonEventHandle;
        private int _captureButtonEventHandle;

        public IHidSystemServer(ServiceCtx context)
        {
            _deviceRegisteredEventForControllerSupport = new KEvent(context.Device.System.KernelContext);
            _connectionTriggerTimeoutEvent = new KEvent(context.Device.System.KernelContext);
            _joyDetachOnBluetoothOffEvent = new KEvent(context.Device.System.KernelContext);
            _playReportControllerUsageUpdateEvent = new KEvent(context.Device.System.KernelContext);
            _playReportRegisteredDeviceUpdateEvent = new KEvent(context.Device.System.KernelContext);
            _hidServer = new IHidServer(context);
        }

        [CommandCmif(101)]
        // AcquireHomeButtonEventHandle() -> handle<copy>
        public ResultCode AcquireHomeButtonEventHandle(ServiceCtx context)
        {
            if (_homeButtonEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(context.Device.Hid.HomeButton.GetEvent().ReadableEvent, out _homeButtonEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_homeButtonEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(111)]
        // ActivateHomeButton(nn::applet::AppletResourceUserId, pid)
        public ResultCode ActivateHomeButton(ServiceCtx context)
        {
            context.Device.Hid.HomeButton.Active = true;

            return ResultCode.Success;
        }

        [CommandCmif(121)]
        // AcquireSleepButtonEventHandle() -> handle<copy>
        public ResultCode AcquireSleepButtonEventHandle(ServiceCtx context)
        {
            if (_sleepButtonEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(context.Device.Hid.SleepButton.GetEvent().ReadableEvent, out _sleepButtonEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_sleepButtonEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(131)]
        // ActivateSleepButton(nn::applet::AppletResourceUserId, pid)
        public ResultCode ActivateSleepButton(ServiceCtx context)
        {
            context.Device.Hid.SleepButton.Active = true;

            return ResultCode.Success;
        }

        [CommandCmif(141)]
        // AcquireCaptureButtonEventHandle() -> handle<copy>
        public ResultCode AcquireCaptureButtonEventHandle(ServiceCtx context)
        {
            if (_captureButtonEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(context.Device.Hid.CaptureButton.GetEvent().ReadableEvent, out _captureButtonEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_captureButtonEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(151)]
        // ActivateCaptureButton(nn::applet::AppletResourceUserId, pid)
        public ResultCode ActivateCaptureButton(ServiceCtx context)
        {
            context.Device.Hid.CaptureButton.Active = true;

            return ResultCode.Success;
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
        // GetLastActiveNpad() -> nn::hid::NpadIdType
        public ResultCode GetLastActiveNpad(ServiceCtx context)
        {
            // TODO: RequestData seems to have garbage data, reading an extra uint seems to fix the issue.
            context.RequestData.ReadUInt32();

            context.ResponseData.Write((byte)context.Device.Hid.Npads.GetLastActiveNpadId());

            return ResultCode.Success;
        }

        [CommandCmif(307)]
        // GetNpadSystemExtStyle(u32) -> (u64, u64)
        public ResultCode GetNpadSystemExtStyle(ServiceCtx context)
        {
            context.RequestData.ReadUInt32();

            foreach (PlayerIndex playerIndex in context.Device.Hid.Npads.GetSupportedPlayers())
            {
                if (HidUtils.GetNpadIdTypeFromIndex(playerIndex) > NpadIdType.Handheld)
                {
                    return ResultCode.InvalidNpadIdType;
                }
            }

            // todo: wrong
            // context.ResponseData.Write((ulong)context.Device.Hid.Npads.SupportedStyleSets);
            context.ResponseData.Write((ulong)AppletFooterUiType.JoyDual);
            context.ResponseData.Write((ulong)0);

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

        [CommandCmif(540)]
        // AcquirePlayReportControllerUsageUpdateEvent() -> handle<copy>
        public ResultCode AcquirePlayReportControllerUsageUpdateEvent(ServiceCtx context)
        {
            if (_playReportControllerUsageUpdateEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_playReportControllerUsageUpdateEvent.ReadableEvent, out _playReportControllerUsageUpdateEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_playReportControllerUsageUpdateEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(541)]
        // GetPlayReportControllerUsages() -> (u64, buffer<nn::hid::system::PlayReportControllerUsage[], 0xa>)
        public ResultCode GetPlayReportControllerUsages(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceHid);

            context.ResponseData.Write(0L);

            return ResultCode.Success;
        }

        [CommandCmif(542)]
        // AcquirePlayReportRegisteredDeviceUpdateEvent() -> handle<copy>
        public ResultCode AcquirePlayReportRegisteredDeviceUpdateEvent(ServiceCtx context)
        {
            if (_playReportRegisteredDeviceUpdateEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_playReportRegisteredDeviceUpdateEvent.ReadableEvent, out _playReportRegisteredDeviceUpdateEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_playReportRegisteredDeviceUpdateEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(543)]
        // GetRegisteredDevicesOld() -> (u64, buffer<nn::hid::system::RegisteredDevice[], 0xa>)
        public ResultCode GetRegisteredDevicesOld(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceHid);

            context.ResponseData.Write(0L);

            return ResultCode.Success;
        }

        [CommandCmif(544)]
        // AcquireConnectionTriggerTimeoutEvent() -> handle<copy>
        public ResultCode AcquireConnectionTriggerTimeoutEvent(ServiceCtx context)
        {
            if (_connectionTriggerTimeoutEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_connectionTriggerTimeoutEvent.ReadableEvent,
                    out _connectionTriggerTimeoutEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_connectionTriggerTimeoutEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(545)]
        // SendConnectionTrigger(nn::bluetooth::Address)
        public ResultCode SendConnectionTrigger(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceHid);

            return ResultCode.Success;
        }

        [CommandCmif(546)]
        // AcquireDeviceRegisteredEventForControllerSupport() -> handle<copy>
        public ResultCode AcquireDeviceRegisteredEventForControllerSupport(ServiceCtx context)
        {
            if (_deviceRegisteredEventForControllerSupportHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_deviceRegisteredEventForControllerSupport.ReadableEvent,
                    out _deviceRegisteredEventForControllerSupportHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_deviceRegisteredEventForControllerSupportHandle);

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
                if (context.Process.HandleTable.GenerateHandle(_joyDetachOnBluetoothOffEvent.ReadableEvent,
                    out _joyDetachOnBluetoothOffEventHandle) != Result.Success)
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
