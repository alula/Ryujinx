using Ryujinx.Common.Logging;

namespace Ryujinx.HLE.HOS.Services.Audio
{
    [Service("audctl")]
    class IAudioController : IpcService
    {
        public IAudioController(ServiceCtx context) { }

        [CommandCmif(9)]
        // GetAudioOutputMode(s32) -> s32
        public ResultCode GetAudioOutputMode(ServiceCtx context)
        {
            context.ResponseData.Write(0); // todo?

            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [CommandCmif(10)]
        // SetAudioOutputMode(s32, s32)
        public ResultCode SetAudioOutputMode(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [CommandCmif(11)]
        // SetForceMutePolicy(u32)
        public ResultCode SetForceMutePolicy(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [CommandCmif(12)]
        // GetForceMutePolicy() -> u32
        public ResultCode GetForceMutePolicy(ServiceCtx context)
        {
            context.ResponseData.Write(0);

            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [CommandCmif(13)]
        // GetOutputModeSetting(u32) -> u32
        public ResultCode GetOutputModeSetting(ServiceCtx context)
        {
            context.ResponseData.Write(0);

            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [CommandCmif(18)] // 3.0.0+
        // GetHeadphoneOutputLevelMode() -> u32
        public ResultCode GetHeadphoneOutputLevelMode(ServiceCtx context)
        {
            context.ResponseData.Write(0);

            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [CommandCmif(31)] // 13.0.0+
        // IsSpeakerAutoMuteEnabled() -> b8
        public ResultCode IsSpeakerAutoMuteEnabled(ServiceCtx context)
        {
            context.ResponseData.Write(false);

            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }
    }
}
