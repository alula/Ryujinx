using Ryujinx.Common.Logging;
using Ryujinx.Horizon.Common;
using Ryujinx.Horizon.Sdk.Lbl;
using Ryujinx.Horizon.Sdk.Sf;

namespace Ryujinx.Horizon.Lbl.Ipc
{
    partial class LblController : ILblController
    {
        private bool _vrModeEnabled;
        private float _currentBrightnessSettingForVrMode;
        private bool _dimmingEnabled = true;
        private bool _autoBrightnessControlEnabled = true;
        private uint _brightnessSetting = 50;

        [CmifCommand(0)]
        public Result SaveCurrentSetting()
        {
            Logger.Stub?.PrintStub(LogClass.ServiceLbl);
            // NOTE: Stubbed in system module.

            return Result.Success;
        }

        [CmifCommand(1)]
        public Result LoadCurrentSetting()
        {
            Logger.Stub?.PrintStub(LogClass.ServiceLbl);
            // NOTE: Stubbed in system module.

            return Result.Success;
        }

        [CmifCommand(2)]
        // SetCurrentBrightnessSetting(u32)
        public Result SetCurrentBrightnessSetting(uint brightnessSetting)
        {
            _brightnessSetting = brightnessSetting;

            return Result.Success;
        }

        [CmifCommand(3)]
        // GetCurrentBrightnessSetting() -> u32
        public Result GetCurrentBrightnessSetting(out uint brightnessSetting)
        {
            brightnessSetting = _brightnessSetting;

            return Result.Success;
        }

        [CmifCommand(4)]
        // ApplyCurrentBrightnessSettingToBacklight()
        public Result ApplyCurrentBrightnessSettingToBacklight()
        {
            return Result.Success;
        }

        [CmifCommand(5)]
        // GetBrightnessSettingAppliedToBacklight() -> u32
        public Result GetBrightnessSettingAppliedToBacklight(out uint brightnessSetting)
        {
            brightnessSetting = _brightnessSetting;

            return Result.Success;
        }

        [CmifCommand(9)]
        // EnableDimming()
        public Result EnableDimming()
        {
            _dimmingEnabled = true;

            return Result.Success;
        }

        [CmifCommand(10)]
        // DisableDimming()
        public Result DisableDimming()
        {
            _dimmingEnabled = false;

            return Result.Success;
        }

        [CmifCommand(11)]
        // IsDimmingEnabled(out bool dimmingEnabled)
        public Result IsDimmingEnabled(out bool dimmingEnabled)
        {
            dimmingEnabled = _dimmingEnabled;

            return Result.Success;
        }

        [CmifCommand(12)]
        // EnableAutoBrightnessControl()
        public Result EnableAutoBrightnessControl()
        {
            _autoBrightnessControlEnabled = true;

            return Result.Success;
        }

        [CmifCommand(13)]
        // DisableAutoBrightnessControl()
        public Result DisableAutoBrightnessControl()
        {
            _autoBrightnessControlEnabled = false;

            return Result.Success;
        }

        [CmifCommand(14)]
        // IsAutoBrightnessControlEnabled(out bool autoBrightnessControlEnabled)
        public Result IsAutoBrightnessControlEnabled(out bool autoBrightnessControlEnabled)
        {
            autoBrightnessControlEnabled = _autoBrightnessControlEnabled;

            return Result.Success;
        }

        [CmifCommand(17)]
        public Result SetBrightnessReflectionDelayLevel(float unknown0, float unknown1)
        {
            // NOTE: Stubbed in system module.

            return Result.Success;
        }

        [CmifCommand(18)]
        public Result GetBrightnessReflectionDelayLevel(out float unknown1, float unknown0)
        {
            // NOTE: Stubbed in system module.

            unknown1 = 0.0f;

            return Result.Success;
        }

        [CmifCommand(19)]
        public Result SetCurrentBrightnessMapping(float unknown0, float unknown1, float unknown2)
        {
            // NOTE: Stubbed in system module.

            return Result.Success;
        }

        [CmifCommand(20)]
        public Result GetCurrentBrightnessMapping(out float unknown0, out float unknown1, out float unknown2)
        {
            // NOTE: Stubbed in system module.

            unknown0 = 0.0f;
            unknown1 = 0.0f;
            unknown2 = 0.0f;

            return Result.Success;
        }

        [CmifCommand(21)]
        public Result SetCurrentAmbientLightSensorMapping(float unknown0, float unknown1, float unknown2)
        {
            // NOTE: Stubbed in system module.

            return Result.Success;
        }

        [CmifCommand(22)]
        public Result GetCurrentAmbientLightSensorMapping(out float unknown0, out float unknown1, out float unknown2)
        {
            // NOTE: Stubbed in system module.

            unknown0 = 0.0f;
            unknown1 = 0.0f;
            unknown2 = 0.0f;

            return Result.Success;
        }

        [CmifCommand(24)]
        public Result SetCurrentBrightnessSettingForVrMode(float currentBrightnessSettingForVrMode)
        {
            if (float.IsNaN(currentBrightnessSettingForVrMode) || float.IsInfinity(currentBrightnessSettingForVrMode))
            {
                _currentBrightnessSettingForVrMode = 0.0f;
            }
            else
            {
                _currentBrightnessSettingForVrMode = currentBrightnessSettingForVrMode;
            }

            return Result.Success;
        }

        [CmifCommand(25)]
        public Result GetCurrentBrightnessSettingForVrMode(out float currentBrightnessSettingForVrMode)
        {
            if (float.IsNaN(_currentBrightnessSettingForVrMode) || float.IsInfinity(_currentBrightnessSettingForVrMode))
            {
                currentBrightnessSettingForVrMode = 0.0f;
            }
            else
            {
                currentBrightnessSettingForVrMode = _currentBrightnessSettingForVrMode;
            }

            return Result.Success;
        }

        [CmifCommand(26)]
        public Result EnableVrMode()
        {
            _vrModeEnabled = true;

            // NOTE: The service checks _vrModeEnabled field value in a thread and then changes the screen brightness.
            //       Since we don't support that, it's fine to do nothing.

            return Result.Success;
        }

        [CmifCommand(27)]
        public Result DisableVrMode()
        {
            _vrModeEnabled = false;

            // NOTE: The service checks _vrModeEnabled field value in a thread and then changes the screen brightness.
            //       Since we don't support that, it's fine to do nothing.

            return Result.Success;
        }

        [CmifCommand(28)]
        public Result IsVrModeEnabled(out bool vrModeEnabled)
        {
            vrModeEnabled = _vrModeEnabled;

            return Result.Success;
        }
    }
}
