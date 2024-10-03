using Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService;
using Ryujinx.HLE.HOS.Services.Am.AppletOE.ApplicationProxyService;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE
{
    [Service("appletAE")]
    class IAllSystemAppletProxiesService : IpcService
    {
        public IAllSystemAppletProxiesService(ServiceCtx context) { }

        [CommandCmif(100)]
        // OpenSystemAppletProxy(u64, pid, handle<copy>) -> object<nn::am::service::ISystemAppletProxy>
        public ResultCode OpenSystemAppletProxy(ServiceCtx context)
        {
            MakeObject(context, new ISystemAppletProxy(context.Request.HandleDesc.PId));

            return ResultCode.Success;
        }

        [CommandCmif(200)]
        [CommandCmif(201)] // 3.0.0+
        // OpenLibraryAppletProxy(u64, pid, handle<copy>) -> object<nn::am::service::ILibraryAppletProxy>
        public ResultCode OpenLibraryAppletProxy(ServiceCtx context)
        {
            MakeObject(context, new ILibraryAppletProxy(context.Request.HandleDesc.PId));

            return ResultCode.Success;
        }

        [CommandCmif(300)]
        // OpenOverlayAppletProxy(u64, pid, handle<copy>) -> object<nn::am::service::IOverlayAppletProxy>
        public ResultCode OpenOverlayAppletProxy(ServiceCtx context)
        {
            MakeObject(context, new IOverlayAppletProxy(context.Request.HandleDesc.PId));

            return ResultCode.Success;
        }

        [CommandCmif(350)]
        // OpenApplicationProxy(u64, pid, handle<copy>) -> object<nn::am::service::IApplicationProxy>
        public ResultCode OpenApplicationProxy(ServiceCtx context)
        {
            MakeObject(context, new IApplicationProxy(context.Request.HandleDesc.PId));

            return ResultCode.Success;
        }
    }
}
