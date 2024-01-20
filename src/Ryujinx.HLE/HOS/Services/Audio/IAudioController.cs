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

        [CommandCmif(10000)]
        // NotifyAudioOutputTargetForPlayReport()
        public ResultCode NotifyAudioOutputTargetForPlayReport(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [CommandCmif(10001)]
        // NotifyAudioOutputChannelCountForPlayReport()
        public ResultCode NotifyAudioOutputChannelCountForPlayReport(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [CommandCmif(10002)]
        // NotifyUnsupportedUsbOutputDeviceAttachedForPlayReport()
        public ResultCode NotifyUnsupportedUsbOutputDeviceAttachedForPlayReport(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [CommandCmif(10100)]
        // GetAudioVolumeDataForPlayReport()
        public ResultCode GetAudioVolumeDataForPlayReport(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [CommandCmif(10101)]
        // BindAudioVolumeUpdateEventForPlayReport()
        public ResultCode BindAudioVolumeUpdateEventForPlayReport(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [CommandCmif(10102)]
        // BindAudioOutputTargetUpdateEventForPlayReport()
        public ResultCode BindAudioOutputTargetUpdateEventForPlayReport(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [CommandCmif(10103)]
        // GetAudioOutputTargetForPlayReport()
        public ResultCode GetAudioOutputTargetForPlayReport(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [CommandCmif(10104)]
        // GetAudioOutputChannelCountForPlayReport()
        public ResultCode GetAudioOutputChannelCountForPlayReport(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [CommandCmif(10105)]
        // BindAudioOutputChannelCountUpdateEventForPlayReport()
        public ResultCode BindAudioOutputChannelCountUpdateEventForPlayReport(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [CommandCmif(10106)]
        // GetDefaultAudioOutputTargetForPlayReport()
        public ResultCode GetDefaultAudioOutputTargetForPlayReport(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }

        [CommandCmif(50000)]
        // SetAnalogInputBoostGainForPrototyping()
        public ResultCode SetAnalogInputBoostGainForPrototyping(ServiceCtx context)
        {
            Logger.Stub?.PrintStub(LogClass.ServiceAudio);

            return ResultCode.Success;
        }
    }
}
