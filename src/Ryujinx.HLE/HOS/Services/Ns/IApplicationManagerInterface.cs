using LibHac.Ns;
using Ryujinx.Common.Logging;
using Ryujinx.Common.Utilities;
using Ryujinx.Horizon.Common;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Ns.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using ApplicationId = LibHac.ApplicationId;

namespace Ryujinx.HLE.HOS.Services.Ns
{
    [Service("ns:am")]
    class IApplicationManagerInterface : IpcService
    {
        // FIXME: Remove this
        private static byte[] StructToBytes<T>(T structure)
        {
            byte[] array = new byte[Marshal.SizeOf(structure)];
            GCHandle handle = default;
            try
            {
                handle = GCHandle.Alloc(array, GCHandleType.Pinned);
                Marshal.StructureToPtr(structure, handle.AddrOfPinnedObject(), false);
            }
            finally
            {
                if (handle.IsAllocated)
                {
                    handle.Free();
                }
            }

            return array;
        }

        private KEvent _applicationRecordUpdateSystemEvent;
        private int _applicationRecordUpdateSystemEventHandle;

        private KEvent _sdCardMountStatusChangedEvent;
        private int    _sdCardMountStatusChangedEventHandle;

        private KEvent _gameCardUpdateDetectionEvent;
        private int    _gameCardUpdateDetectionEventHandle;

        private KEvent _gameCardMountFailureEvent;
        private int    _gameCardMountFailureEventHandle;

        public IApplicationManagerInterface(ServiceCtx context)
        {
            _applicationRecordUpdateSystemEvent = new KEvent(context.Device.System.KernelContext);
            _sdCardMountStatusChangedEvent = new KEvent(context.Device.System.KernelContext);
            _gameCardUpdateDetectionEvent = new KEvent(context.Device.System.KernelContext);
            _gameCardMountFailureEvent = new KEvent(context.Device.System.KernelContext);
        }


        [CommandCmif(0)]
        // ListApplicationRecord(s32) -> (s32, buffer<ApplicationRecord[], 6>)
        // entry_offset -> (out_entrycount, ApplicationRecord[])
        public ResultCode ListApplicationRecord(ServiceCtx ctx)
        {
            int entryOffset = ctx.RequestData.ReadInt32();
            ulong position = ctx.Request.ReceiveBuff[0].Position;
            List<ApplicationRecord> records = new();

            foreach (ApplicationId appId in ctx.Device.Configuration.Titles)
            {
                records.Add(new ApplicationRecord()
                {
                    Type = ApplicationRecordType.Installed,
                    AppId = appId,
                    Unknown1 = 0x2,
                    Unknown2 = new byte[6],
                    Unknown3 = 0,
                    Unknown4 = new byte[7]
                });
            }
            // TODO: Confirm this is correct and works
            records.Sort((x, y) => (int)(x.AppId.Value - y.AppId.Value));
            if (entryOffset > 0)
            {
                records = records.Skip(entryOffset - 1).ToList();
            }

            ctx.ResponseData.Write(records.Count);
            foreach (var record in records)
            {
                ctx.Memory.Write(position, StructToBytes(record));
                position += (ulong)Marshal.SizeOf<ApplicationRecord>();
            }

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // GetApplicationRecordUpdateSystemEvent() -> handle<copy>
        public ResultCode GetApplicationRecordUpdateSystemEvent(ServiceCtx context)
        {
            if (_applicationRecordUpdateSystemEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_applicationRecordUpdateSystemEvent.ReadableEvent, out _applicationRecordUpdateSystemEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_applicationRecordUpdateSystemEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(44)]
        // GetSdCardMountStatusChangedEvent() -> handle<copy>
        public ResultCode GetSdCardMountStatusChangedEvent(ServiceCtx context)
        {
            if (_sdCardMountStatusChangedEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_sdCardMountStatusChangedEvent.ReadableEvent, out _sdCardMountStatusChangedEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_sdCardMountStatusChangedEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(52)]
        // GetGameCardUpdateDetectionEvent() -> handle<copy>
        public ResultCode GetGameCardUpdateDetectionEvent(ServiceCtx context)
        {
            if (_gameCardUpdateDetectionEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_gameCardUpdateDetectionEvent.ReadableEvent, out _gameCardUpdateDetectionEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_gameCardUpdateDetectionEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(70)]
        // ResumeAll()
        public ResultCode ResumeAll(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNs);

            return ResultCode.Success;
        }

        [CommandCmif(505)]
        // GetGameCardMountFailureEvent() -> handle<copy>
        public ResultCode GetGameCardMountFailureEvent(ServiceCtx context)
        {
            if (_gameCardMountFailureEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_gameCardMountFailureEvent.ReadableEvent, out _gameCardMountFailureEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_gameCardMountFailureEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(400)]
        // GetApplicationControlData(u8, u64) -> (unknown<4>, buffer<unknown, 6>)
        public ResultCode GetApplicationControlData(ServiceCtx context)
        {
#pragma warning disable IDE0059 // Remove unnecessary value assignment
            byte source = (byte)context.RequestData.ReadInt64();
            ulong titleId = context.RequestData.ReadUInt64();
#pragma warning restore IDE0059

            ulong position = context.Request.ReceiveBuff[0].Position;

            ApplicationControlProperty nacp = context.Device.Processes.ActiveApplication.ApplicationControlProperties;

            context.Memory.Write(position, SpanHelpers.AsByteSpan(ref nacp).ToArray());

            return ResultCode.Success;
        }
    }
}
