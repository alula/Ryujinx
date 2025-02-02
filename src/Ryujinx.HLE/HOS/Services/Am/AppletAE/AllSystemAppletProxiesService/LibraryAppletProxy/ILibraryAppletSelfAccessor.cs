using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Applets;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.LibraryAppletProxy
{
    class ILibraryAppletSelfAccessor : DisposableIpcService
    {
        private readonly KernelContext _kernelContext;
        private readonly RealApplet _applet;

        private readonly KEvent _normalInDataEvent;
        private readonly KEvent _interactiveInDataEvent;
        private int _normalInDataEventHandle;
        private int _interactiveInDataEventHandle;

        public ILibraryAppletSelfAccessor(ServiceCtx context, ulong pid)
        {
            var system = context.Device.System;

            _kernelContext = system.KernelContext;
            _applet = system.WindowSystem.GetByAruId(pid);
            _normalInDataEvent = new KEvent(system.KernelContext);
            _interactiveInDataEvent = new KEvent(system.KernelContext);

            _applet.NormalSession.InDataAvailable += OnNormalInData;
            _applet.InteractiveSession.InDataAvailable += OnInteractiveInData;
        }

        private void OnNormalInData(object sender, EventArgs e)
        {
            _normalInDataEvent.WritableEvent.Signal();
        }

        private void OnInteractiveInData(object sender, EventArgs e)
        {
            _interactiveInDataEvent.WritableEvent.Signal();
        }

        [CommandCmif(0)]
        // PopInData() -> object<nn::am::service::IStorage>
        public ResultCode PopInData(ServiceCtx context)
        {
            byte[] appletData;

            if (!_applet.NormalSession.TryPopInData(out appletData))
            {
                return ResultCode.NotAvailable;
            }

            if (appletData.Length == 0)
            {
                return ResultCode.NotAvailable;
            }

            MakeObject(context, new IStorage(appletData));

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        public ResultCode PushOutData(ServiceCtx context)
        {
            if (_applet != null)
            {
                IStorage data = GetObject<IStorage>(context, 0);
                _applet.NormalSession.PushOutData(data.Data);
            }

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        public ResultCode PopInteractiveInData(ServiceCtx context)
        {
            byte[] appletData;

            if (!_applet.InteractiveSession.TryPopInData(out appletData))
            {
                return ResultCode.NotAvailable;
            }

            if (appletData.Length == 0)
            {
                return ResultCode.NotAvailable;
            }

            MakeObject(context, new IStorage(appletData));

            return ResultCode.Success;
        }

        [CommandCmif(3)]
        public ResultCode PushInteractiveOutData(ServiceCtx context)
        {
            if (_applet != null)
            {
                IStorage data = GetObject<IStorage>(context, 0);
                _applet.InteractiveSession.PushOutData(data.Data);
            }

            return ResultCode.Success;
        }

        [CommandCmif(5)]
        public ResultCode GetPopInDataEvent(ServiceCtx context)
        {
            if (_normalInDataEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_normalInDataEvent.ReadableEvent, out _normalInDataEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_normalInDataEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(6)]
        public ResultCode GetPopInteractiveInDataEvent(ServiceCtx context)
        {
            if (_interactiveInDataEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_interactiveInDataEvent.ReadableEvent, out _interactiveInDataEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_interactiveInDataEventHandle);

            return ResultCode.Success;
        }


        [CommandCmif(10)]
        public ResultCode ExitProcessAndReturn(ServiceCtx context)
        {
            _applet.ProcessHandle.Terminate();

            return ResultCode.Success;
        }

        [CommandCmif(11)]
        // GetLibraryAppletInfo() -> nn::am::service::LibraryAppletInfo
        public ResultCode GetLibraryAppletInfo(ServiceCtx context)
        {
            LibraryAppletInfo libraryAppletInfo = new();

            libraryAppletInfo.AppletId = _applet.AppletId;
            libraryAppletInfo.LibraryAppletMode = _applet.LibraryAppletMode;

            context.ResponseData.WriteStruct(libraryAppletInfo);

            return ResultCode.Success;
        }

        [CommandCmif(12)]
        // GetMainAppletIdentityInfo() -> nn::am::service::AppletIdentityInfo
        public ResultCode GetMainAppletIdentityInfo(ServiceCtx context)
        {
            AppletIdentifyInfo appletIdentifyInfo = new()
            {
                AppletId = AppletId.QLaunch,
                TitleId = 0x0100000000001000,
            };

            context.ResponseData.WriteStruct(appletIdentifyInfo);

            return ResultCode.Success;
        }

        [CommandCmif(13)]
        // CanUseApplicationCore() -> bool
        public ResultCode CanUseApplicationCore(ServiceCtx context)
        {
            context.ResponseData.Write(false);

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(14)]
        // GetCallerAppletIdentityInfo() -> nn::am::service::AppletIdentityInfo
        public ResultCode GetCallerAppletIdentityInfo(ServiceCtx context)
        {
            context.ResponseData.WriteStruct(GetCallerIdentity(_applet));

            return ResultCode.Success;
        }

        [CommandCmif(30)]
        // UnpopInData(nn::am::service::IStorage)
        public ResultCode UnpopInData(ServiceCtx context)
        {
            IStorage data = GetObject<IStorage>(context, 0);

            _applet.NormalSession.InsertFrontInData(data.Data);

            return ResultCode.Success;
        }

        [CommandCmif(50)]
        // ReportVisibleError(nn::err::ErrorCode)
        public ResultCode ReportVisibleError(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAm);
            return ResultCode.Success;
        }

        private static AppletIdentifyInfo GetCallerIdentity(RealApplet applet)
        {
            if (applet.CallerApplet != null)
            {
                return new AppletIdentifyInfo
                {
                    AppletId = applet.CallerApplet.AppletId,
                    TitleId = applet.CallerApplet.ProcessHandle.TitleId,
                };
            }
            else
            {
                return new AppletIdentifyInfo
                {
                    AppletId = AppletId.QLaunch,
                    TitleId = 0x0100000000001000
                };
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (_normalInDataEventHandle != 0)
                {
                    _kernelContext.Syscall.CloseHandle(_normalInDataEventHandle);
                }

                if (_interactiveInDataEventHandle != 0)
                {
                    _kernelContext.Syscall.CloseHandle(_interactiveInDataEventHandle);
                }
            }
        }
    }
}
