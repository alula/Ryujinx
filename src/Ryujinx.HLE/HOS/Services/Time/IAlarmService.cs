namespace Ryujinx.HLE.HOS.Services.Time
{
    [Service("time:al")] // 9.0.0+
    class IAlarmService : IpcService
    {
        public IAlarmService(ServiceCtx context) { }

        [CommandCmif(0)]
        // CreateWakeupAlarm() -> object<nn::time::detail::service::ISteadyClockAlarm>
        public ResultCode CreateWakeupAlarm(ServiceCtx context)
        {
            MakeObject(context, new ISteadyClockAlarm(context));

            return ResultCode.Success;
        }

        [CommandCmif(1)]
        // CreateBackgroundTaskAlarm() -> object<nn::time::detail::service::ISteadyClockAlarm>
        public ResultCode CreateBackgroundTaskAlarm(ServiceCtx context)
        {
            MakeObject(context, new ISteadyClockAlarm(context));

            return ResultCode.Success;
        }
    }
}
