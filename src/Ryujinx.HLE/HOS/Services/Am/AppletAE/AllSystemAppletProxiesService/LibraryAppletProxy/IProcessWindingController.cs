using Ryujinx.Common;
using Ryujinx.HLE.HOS.Applets;

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
    }
}
