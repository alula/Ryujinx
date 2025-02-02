using Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService;
using Ryujinx.HLE.HOS.Services.Am.AppletOE.ApplicationProxyService;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE
{
    [Service("appletAE")]
    class IAllSystemAppletProxiesService : IpcService
    {
        public IAllSystemAppletProxiesService(ServiceCtx context) { }

        [CommandCmif(100)]
        // OpenSystemAppletProxy(pid, handle<copy>) -> object<nn::am::service::ISystemAppletProxy>
        public ResultCode OpenSystemAppletProxy(ServiceCtx context)
        {
            context.Device.System.WindowSystem.TrackProcess(context.Request.HandleDesc.PId, 0, false);
            MakeObject(context, new ISystemAppletProxy(context.Request.HandleDesc.PId));

            return ResultCode.Success;
        }

        [CommandCmif(200)]
        [CommandCmif(201)] // 3.0.0+
        // OpenLibraryAppletProxy(pid, handle<copy>) -> object<nn::am::service::ILibraryAppletProxy>
        public ResultCode OpenLibraryAppletProxy(ServiceCtx context)
        {
            context.Device.System.WindowSystem.TrackProcess(context.Request.HandleDesc.PId, 0, false);
            MakeObject(context, new ILibraryAppletProxy(context, context.Request.HandleDesc.PId));

            return ResultCode.Success;
        }

        [CommandCmif(300)]
        // OpenOverlayAppletProxy(pid, handle<copy>) -> object<nn::am::service::IOverlayAppletProxy>
        public ResultCode OpenOverlayAppletProxy(ServiceCtx context)
        {
            context.Device.System.WindowSystem.TrackProcess(context.Request.HandleDesc.PId, 0, false);
            MakeObject(context, new IOverlayAppletProxy(context.Request.HandleDesc.PId));

            return ResultCode.Success;
        }

        [CommandCmif(350)]
        // OpenSystemApplicationProxy(pid, handle<copy>) -> object<nn::am::service::IApplicationProxy>
        public ResultCode OpenSystemApplicationProxy(ServiceCtx context)
        {
            context.Device.System.WindowSystem.TrackProcess(context.Request.HandleDesc.PId, 0, false);
            MakeObject(context, new IApplicationProxy(context.Request.HandleDesc.PId));

            return ResultCode.Success;
        }
    }
}
