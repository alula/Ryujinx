using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Ldn.Lp2p.SfMonitorServiceCreator
{
    class ISfMonitorService : IpcService
    {
        public ISfMonitorService(ServiceCtx context) { }

        [CommandCmif(0)]
        // Initialize()
        public ResultCode Initialize(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceLdn);

            return ResultCode.Success;
        }

        [CommandCmif(288)]
        // GetGroupInfo() -> buffer<nn::lp2p::GroupInfo, 0x32>
        public ResultCode GetGroupInfo(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceLdn);

            return ResultCode.Success;
        }
    }
}