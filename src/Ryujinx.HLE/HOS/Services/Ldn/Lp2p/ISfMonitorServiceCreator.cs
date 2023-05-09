using Ryujinx.Common.Logging;
using Ryujinx.HLE.HOS.Services.Ldn.Lp2p.SfMonitorServiceCreator;

namespace Ryujinx.HLE.HOS.Services.Ldn.Lp2p
{
    [Service("lp2p:m")] // 9.1.0+
    class ISfMonitorServiceCreator : IpcService
    {
        public ISfMonitorServiceCreator(ServiceCtx context) { }

        [CommandCmif(0)]
        // CreateMonitorService(pid, u64, u64) -> object<nn::lp2p::monitor::detail::ISfMonitorService>
        public ResultCode CreateMonitorService(ServiceCtx context)
        {
            MakeObject(context, new ISfMonitorService(context));

            Logger.Stub?.PrintStub(LogClass.ServiceLdn);

            return ResultCode.Success;
        }
    }
}