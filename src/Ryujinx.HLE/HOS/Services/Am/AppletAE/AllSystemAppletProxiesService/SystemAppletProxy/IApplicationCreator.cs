using LibHac.Ncm;
using LibHac.Tools.FsSystem.NcaUtils;
using Ryujinx.HLE.FileSystem;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE.AllSystemAppletProxiesService.SystemAppletProxy
{
    class IApplicationCreator : IpcService
    {
        public IApplicationCreator() { }

        [CommandCmif(10)]
        // CreateSystemApplication(nn::ncm::SystemApplicationId) -> object<nn::am::service::IApplicationAccessor>
        public ResultCode CreateSystemApplication(ServiceCtx context)
        {
            var applicationId = context.RequestData.ReadUInt64();
            var system = context.Device.System;
            string contentPath = system.ContentManager.GetInstalledContentPath(applicationId, StorageId.BuiltInSystem, NcaContentType.Program);

            if (contentPath.Length == 0)
            {
                return ResultCode.TitleIdNotFound;
            }

            if (contentPath.StartsWith("@SystemContent"))
            {
                contentPath = VirtualFileSystem.SwitchPathToSystemPath(contentPath);
            }

            MakeObject(context, new IApplicationAccessor(applicationId, contentPath, context.Device.System));

            return ResultCode.Success;
        }
    }
}
