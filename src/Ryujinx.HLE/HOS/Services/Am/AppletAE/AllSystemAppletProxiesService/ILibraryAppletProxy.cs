using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Applets;
using Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.LibraryAppletProxy;
using Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService
{
    class ILibraryAppletProxy : IpcService
    {
        private readonly ulong _pid;
        private readonly ServiceCtx _context;

        public ILibraryAppletProxy(ServiceCtx context, ulong pid)
        {
            _context = context;
            _pid = pid;
        }

        private RealApplet GetApplet()
        {
            return _context.Device.System.WindowSystem.GetByAruId(_pid);
        }

        [CommandCmif(0)]
        // GetCommonStateGetter() -> object<nn::am::service::ICommonStateGetter>
        public ResultCode GetCommonStateGetter(ServiceCtx context)
        {
            MakeObject(context, new ICommonStateGetter(context, _pid));

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        // GetSelfController() -> object<nn::am::service::ISelfController>
        public ResultCode GetSelfController(ServiceCtx context)
        {
            MakeObject(context, new ISelfController(context, _pid));

            return ResultCode.Success;
        }

        [CommandCmif(2)]
        // GetWindowController() -> object<nn::am::service::IWindowController>
        public ResultCode GetWindowController(ServiceCtx context)
        {
            MakeObject(context, new IWindowController(_pid));

            return ResultCode.Success;
        }

        [CommandCmif(3)]
        // GetAudioController() -> object<nn::am::service::IAudioController>
        public ResultCode GetAudioController(ServiceCtx context)
        {
            MakeObject(context, new IAudioController());

            return ResultCode.Success;
        }

        [CommandCmif(4)]
        // GetDisplayController() -> object<nn::am::service::IDisplayController>
        public ResultCode GetDisplayController(ServiceCtx context)
        {
            MakeObject(context, new IDisplayController(context));

            return ResultCode.Success;
        }

        [CommandCmif(10)]
        // GetProcessWindingController() -> object<nn::am::service::IProcessWindingController>
        public ResultCode GetProcessWindingController(ServiceCtx context)
        {
            MakeObject(context, new IProcessWindingController(context, _pid));

            return ResultCode.Success;
        }

        [CommandCmif(11)]
        // GetLibraryAppletCreator() -> object<nn::am::service::ILibraryAppletCreator>
        public ResultCode GetLibraryAppletCreator(ServiceCtx context)
        {
            MakeObject(context, new ILibraryAppletCreator(context, _pid));

            return ResultCode.Success;
        }

        [CommandCmif(20)]
        // OpenLibraryAppletSelfAccessor() -> object<nn::am::service::ILibraryAppletSelfAccessor>
        public ResultCode OpenLibraryAppletSelfAccessor(ServiceCtx context)
        {
            MakeObject(context, new ILibraryAppletSelfAccessor(context, _pid));

            return ResultCode.Success;
        }

        [CommandCmif(21)]
        // GetAppletCommonFunctions() -> object<nn::am::service::IAppletCommonFunctions>
        public ResultCode GetAppletCommonFunctions(ServiceCtx context)
        {
            MakeObject(context, new IAppletCommonFunctions());

            return ResultCode.Success;
        }

        [CommandCmif(22)]
        // GetHomeMenuFunctions() -> object<nn::am::service::IHomeMenuFunctions>
        public ResultCode GetHomeMenuFunctions(ServiceCtx context)
        {
            MakeObject(context, new IHomeMenuFunctions(context.Device.System));

            return ResultCode.Success;
        }

        [CommandCmif(23)]
        // GetGlobalStateController() -> object<nn::am::service::IGlobalStateController>
        public ResultCode GetGlobalStateController(ServiceCtx context)
        {
            MakeObject(context, new IGlobalStateController(context));

            return ResultCode.Success;
        }

        [CommandCmif(1000)]
        // GetDebugFunctions() -> object<nn::am::service::IDebugFunctions>
        public ResultCode GetDebugFunctions(ServiceCtx context)
        {
            MakeObject(context, new IDebugFunctions());

            return ResultCode.Success;
        }
    }
}
