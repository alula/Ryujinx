using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Ldn.MonitorServiceCreator
{
    class IMonitorService : IpcService
    {
        public IMonitorService(ServiceCtx context) { }

        [CommandCmif(0)]
        // GetStateForMonitor() -> u32
        public ResultCode GetStateForMonitor(ServiceCtx context)
        {
            context.ResponseData.Write(0);
            
            Logger.Stub?.PrintStub(LogClass.ServiceLdn);

            return ResultCode.Success;
        }

        [CommandCmif(100)]
        // InitializeMonitor()
        public ResultCode InitializeMonitor(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceLdn);

            return ResultCode.Success;
        }

        [CommandCmif(101)]
        // FinalizeMonitor()
        public ResultCode FinalizeMonitor(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceLdn);

            return ResultCode.Success;
        }
    }
}