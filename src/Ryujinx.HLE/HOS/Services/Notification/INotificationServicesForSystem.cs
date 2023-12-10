using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Notification
{
    // [Service("notif:s")] // 9.0.0+
    class INotificationServicesForSystem : IpcService
    {
        public INotificationServicesForSystem(ServiceCtx context) { }

        [CommandCmif(520)]
        // ListAlarmSettings() -> (s32, buffer<AlarmSetting, 6>)
        public ResultCode ListAlarmSettings(ServiceCtx context)
        {
            context.ResponseData.Write(0);

            Logger.Stub?.PrintStub(LogClass.ServiceNotif);

            return ResultCode.Success;
        }

        [CommandCmif(1040)]
        // OpenNotificationSystemEventAccessor() -> object<nn::notification::server::INotificationSystemEventAccessor>
        public ResultCode OpenNotificationSystemEventAccessor(ServiceCtx context)
        {
            MakeObject(context, new INotificationSystemEventAccessor(context));

            return ResultCode.Success;
        }

        [CommandCmif(1510)]
        // GetPresentationSetting() -> unknown
        public ResultCode GetPresentationSetting(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceNotif);

            return ResultCode.Success;
        }
    }
}
