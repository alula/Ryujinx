using Ryujinx.Common;
using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Applets;
using Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.LibraryAppletCreator;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.LibraryAppletProxy
{
    class IProcessWindingController : IpcService
    {
        private RealApplet _applet;

        public IProcessWindingController(ServiceCtx context, ulong pid)
        {
            _applet = context.Device.System.WindowSystem.GetByAruId(pid);
        }

        [CommandCmif(0)]
        // GetLaunchReason() -> nn::am::service::AppletProcessLaunchReason
        public ResultCode GetLaunchReason(ServiceCtx context)
        {
            context.ResponseData.WriteStruct(_applet.LaunchReason);

            return ResultCode.Success;
        }

        [CommandCmif(21)]
        // PushContext(object<nn::am::service::IStorage>)
        public ResultCode PushContext(ServiceCtx context)
        {
            if (_applet != null)
            {
                IStorage data = GetObject<IStorage>(context, 0);
                _applet.ContextChannel.PushData(data.Data);
            }

            return ResultCode.Success;
        }

        [CommandCmif(22)]
        // PopContext() -> object<nn::am::service::IStorage>
        public ResultCode PopContext(ServiceCtx context)
        {
            byte[] appletData;

            if (!_applet.ContextChannel.TryPopData(out appletData))
            {
                return ResultCode.NotAvailable;
            }

            MakeObject(context, new IStorage(appletData));

            return ResultCode.Success;
        }

        [CommandCmif(30)]
        // WindAndDoReserved()
        public ResultCode WindAndDoReserved(ServiceCtx context)
        {
            _applet.ExitLocked = false;
            _applet.ProcessHandle.Terminate();

            return ResultCode.Success;
        }

        [CommandCmif(40)]
        // ReserveToStartAndWaitAndUnwindThis(object<ILibraryAppletAccessor>)
        public ResultCode ReserveToStartAndWaitAndUnwindThis(ServiceCtx context)
        {
            ILibraryAppletAccessor libraryAppletAccessor = GetObject<ILibraryAppletAccessor>(context, 0);

            Logger.Stub?.PrintStub(LogClass.ServiceAm);

            return ResultCode.Success;
        }
    }
}
