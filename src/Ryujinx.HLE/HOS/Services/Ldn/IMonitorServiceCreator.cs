using Ryujinx.HLE.HOS.Services.Ldn.MonitorServiceCreator;

namespace Ryujinx.HLE.HOS.Services.Ldn
{
    [Service("ldn:m")]
    class IMonitorServiceCreator : IpcService
    {
        public IMonitorServiceCreator(ServiceCtx context) { }

        [CommandCmif(0)]
        // CreateMonitorService() -> object<nn::ldn::detail::IMonitorService>
        public ResultCode CreateMonitorService(ServiceCtx context)
        {
            MakeObject(context, new IMonitorService(context));

            return ResultCode.Success;
        }
    }
}
