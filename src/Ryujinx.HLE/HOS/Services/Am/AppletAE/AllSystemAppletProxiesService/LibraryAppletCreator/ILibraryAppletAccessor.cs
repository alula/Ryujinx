using LibHac.Ncm;
using LibHac.Tools.FsSystem.NcaUtils;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Applets;
using Ryujinx.HLE.HOS.Ipc;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.Horizon.Common;
using System;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.LibraryAppletCreator
{
    class ILibraryAppletAccessor : DisposableIpcService
    {
        private readonly KernelContext _kernelContext;
        private readonly ulong _callerPid;

        private readonly AppletId _appletId;
        private RealApplet _applet;

        private readonly AppletSession _normalSession;
        private readonly AppletSession _interactiveSession;

        private readonly KEvent _stateChangedEvent;
        private readonly KEvent _normalOutDataEvent;
        private readonly KEvent _interactiveOutDataEvent;

        private int _stateChangedEventHandle;
        private int _normalOutDataEventHandle;
        private int _interactiveOutDataEventHandle;

        private int _indirectLayerHandle;

        private ResultCode StartAppletProcess(Horizon system)
        {
            // TODO: use ns
            var programId = RealApplet.GetProgramIdFromAppletId(_appletId);

            string contentPath = system.ContentManager.GetInstalledContentPath(programId, StorageId.BuiltInSystem, NcaContentType.Program);

            if (contentPath.Length == 0)
            {
                return ResultCode.AppletLaunchFailed;
            }

            if (contentPath.StartsWith("@SystemContent"))
            {
                contentPath = FileSystem.VirtualFileSystem.SwitchPathToSystemPath(contentPath);
            }

            if (!system.Device.Processes.LoadNca(contentPath, out var Process))
            {
                return ResultCode.AppletLaunchFailed;
            }

            _applet = system.WindowSystem.TrackProcess(Process.ProcessId, _callerPid, false);
            _applet.AppletStateChanged += OnAppletStateChanged;

            _applet.AppletState.LaunchableEvent.ReadableEvent.Signal();

            return ResultCode.Success;
        }

        public ILibraryAppletAccessor(AppletId appletId, Horizon system, ulong callerPid)
        {
            _kernelContext = system.KernelContext;

            _stateChangedEvent = new KEvent(system.KernelContext);
            _normalOutDataEvent = new KEvent(system.KernelContext);
            _interactiveOutDataEvent = new KEvent(system.KernelContext);

            _callerPid = callerPid;
            _appletId = appletId;

            _normalSession = new AppletSession();
            _interactiveSession = new AppletSession();

            _normalSession.OutDataAvailable += OnNormalOutData;
            _interactiveSession.OutDataAvailable += OnInteractiveOutData;
        }

        private void OnAppletStateChanged(object sender, EventArgs e)
        {
            _stateChangedEvent.WritableEvent.Signal();
        }

        private void OnNormalOutData(object sender, EventArgs e)
        {
            _normalOutDataEvent.WritableEvent.Signal();
        }

        private void OnInteractiveOutData(object sender, EventArgs e)
        {
            _interactiveOutDataEvent.WritableEvent.Signal();
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
            var result = StartAppletProcess(context.Device.System);
            if (result != ResultCode.Success)
            {
                return result;
            }

            return (ResultCode)_applet.Start(_normalSession, _interactiveSession);
        }

        [CommandCmif(20)]
        // RequestExit()
        public ResultCode RequestExit(ServiceCtx context)
        {
            _applet.AppletState.OnExitRequested();

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(25)]
        // Terminate()
        public ResultCode Terminate(ServiceCtx context)
        {
            _applet?.ProcessHandle.Terminate();

            return ResultCode.Success;
        }

        [CommandCmif(30)]
        // GetResult()
        public ResultCode GetResult(ServiceCtx context)
        {
            if (_applet == null)
            {
                return ResultCode.LibraryAppletTerminated;
            }

            return (ResultCode)_applet.GetResult();
        }

        [CommandCmif(60)]
        // PresetLibraryAppletGpuTimeSliceZero()
        public ResultCode PresetLibraryAppletGpuTimeSliceZero(ServiceCtx context)
        {
            // NOTE: This call reset two internal fields to 0 and one internal field to "true".
            //       It seems to be used only with software keyboard inline.
            //       Since we doesn't support applets for now, it's fine to stub it.

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }

        [CommandCmif(100)]
        // PushInData(object<nn::am::service::IStorage>)
        public ResultCode PushInData(ServiceCtx context)
        {
            IStorage data = GetObject<IStorage>(context, 0);

            _normalSession.PushInData(data.Data);

            return ResultCode.Success;
        }

        [CommandCmif(101)]
        // PopOutData() -> object<nn::am::service::IStorage>
        public ResultCode PopOutData(ServiceCtx context)
        {
            if (_normalSession.TryPopOutData(out byte[] data))
            {
                MakeObject(context, new IStorage(data));

                _normalOutDataEvent.WritableEvent.Clear();

                return ResultCode.Success;
            }

            return ResultCode.NotAvailable;
        }

        [CommandCmif(103)]
        // PushInteractiveInData(object<nn::am::service::IStorage>)
        public ResultCode PushInteractiveInData(ServiceCtx context)
        {
            IStorage data = GetObject<IStorage>(context, 0);

            _interactiveSession.PushInData(data.Data);

            return ResultCode.Success;
        }

        [CommandCmif(104)]
        // PopInteractiveOutData() -> object<nn::am::service::IStorage>
        public ResultCode PopInteractiveOutData(ServiceCtx context)
        {
            if (_interactiveSession.TryPopOutData(out byte[] data))
            {
                MakeObject(context, new IStorage(data));

                _interactiveOutDataEvent.WritableEvent.Clear();

                return ResultCode.Success;
            }

            return ResultCode.NotAvailable;
        }

        [CommandCmif(105)]
        // GetPopOutDataEvent() -> handle<copy>
        public ResultCode GetPopOutDataEvent(ServiceCtx context)
        {
            if (_normalOutDataEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_normalOutDataEvent.ReadableEvent, out _normalOutDataEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_normalOutDataEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(106)]
        // GetPopInteractiveOutDataEvent() -> handle<copy>
        public ResultCode GetPopInteractiveOutDataEvent(ServiceCtx context)
        {
            if (_interactiveOutDataEventHandle == 0)
            {
                if (context.Process.HandleTable.GenerateHandle(_interactiveOutDataEvent.ReadableEvent, out _interactiveOutDataEventHandle) != Result.Success)
                {
                    throw new InvalidOperationException("Out of handles!");
                }
            }

            context.Response.HandleDesc = IpcHandleDesc.MakeCopy(_interactiveOutDataEventHandle);

            return ResultCode.Success;
        }

        [CommandCmif(110)]
        // NeedsToExitProcess()
        public ResultCode NeedsToExitProcess(ServiceCtx context)
        {
            return ResultCode.Stubbed;
        }

        [CommandCmif(150)]
        // RequestForAppletToGetForeground()
        public ResultCode RequestForAppletToGetForeground(ServiceCtx context)
        {
            return ResultCode.Stubbed;
        }

        [CommandCmif(160)] // 2.0.0+
        // GetIndirectLayerConsumerHandle() -> u64 indirect_layer_consumer_handle
        public ResultCode GetIndirectLayerConsumerHandle(ServiceCtx context)
        {
            if (_applet == null)
            {
                return ResultCode.LibraryAppletTerminated;
            }

            _indirectLayerHandle = _applet.AppletState.IndirectLayerHandles.Add(_applet);

            context.ResponseData.Write((ulong)_indirectLayerHandle);

            return ResultCode.Success;
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                if (_stateChangedEventHandle != 0)
                {
                    _kernelContext.Syscall.CloseHandle(_stateChangedEventHandle);
                }

                if (_normalOutDataEventHandle != 0)
                {
                    _kernelContext.Syscall.CloseHandle(_normalOutDataEventHandle);
                }

                if (_interactiveOutDataEventHandle != 0)
                {
                    _kernelContext.Syscall.CloseHandle(_interactiveOutDataEventHandle);
                }

                _applet?.AppletState.IndirectLayerHandles.Delete(_indirectLayerHandle);
            }
        }
    }
}
