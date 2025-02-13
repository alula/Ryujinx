using Ryujinx.Common;
using Ryujinx.Common.Configuration.Hid;
using Ryujinx.Common.Memory;
using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.HOS.Kernel.Memory;
using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory;
using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Button;
using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Common;
using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.DebugPad;
using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Keyboard;
using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Mouse;
using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Npad;
using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.TouchScreen;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    public class Hid
    {
        private readonly Switch _device;

        private readonly SharedMemoryStorage _storage;

        internal ref SharedMemory SharedMemory => ref _storage.GetRef<SharedMemory>(0);

        internal const int SharedMemEntryCount = 16;

        public DebugPadDevice DebugPad;
        public TouchDevice Touchscreen;
        public MouseDevice Mouse;
        public KeyboardDevice Keyboard;
        public NpadDevices Npads;
        public ButtonDevice HomeButton;
        public ButtonDevice SleepButton;
        public ButtonDevice CaptureButton;

        private static void CheckTypeSizeOrThrow<T>(int expectedSize)
        {
            if (Unsafe.SizeOf<T>() != expectedSize)
            {
                throw new InvalidStructLayoutException<T>(expectedSize);
            }
        }

        static Hid()
        {
            CheckTypeSizeOrThrow<RingLifo<DebugPadState>>(0x2c8);
            CheckTypeSizeOrThrow<RingLifo<TouchScreenState>>(0x2C38);
            CheckTypeSizeOrThrow<RingLifo<MouseState>>(0x350);
            CheckTypeSizeOrThrow<RingLifo<KeyboardState>>(0x3D8);
            CheckTypeSizeOrThrow<RingLifo<ButtonState>>(0x1B8);
            CheckTypeSizeOrThrow<Array10<NpadState>>(0x32000);
            CheckTypeSizeOrThrow<SharedMemory>(Horizon.HidSize);
        }

        internal Hid(in Switch device, SharedMemoryStorage storage)
        {
            _device = device;
            _storage = storage;

            SharedMemory = SharedMemory.Create();

            InitDevices();
        }

        private void InitDevices()
        {
            DebugPad = new DebugPadDevice(_device, true);
            Touchscreen = new TouchDevice(_device, true);
            Mouse = new MouseDevice(_device, false);
            Keyboard = new KeyboardDevice(_device, false);
            Npads = new NpadDevices(_device, true);
            HomeButton = new ButtonDevice(_device, true, ButtonDeviceType.HomeButton);
            SleepButton = new ButtonDevice(_device, true, ButtonDeviceType.SleepButton);
            CaptureButton = new ButtonDevice(_device, true, ButtonDeviceType.CaptureButton);
        }

        public void RefreshInputConfig(List<InputConfig> inputConfig)
        {
            ControllerConfig[] npadConfig = new ControllerConfig[inputConfig.Count];

            for (int i = 0; i < npadConfig.Length; ++i)
            {
                npadConfig[i].Player = (PlayerIndex)inputConfig[i].PlayerIndex;
                npadConfig[i].Type = (ControllerType)inputConfig[i].ControllerType;
            }

            _device.Hid.Npads.Configure(npadConfig);
        }

        public ControllerKeys UpdateStickButtons(JoystickPosition leftStick, JoystickPosition rightStick)
        {
            const int StickButtonThreshold = short.MaxValue / 2;
            ControllerKeys result = 0;

#pragma warning disable IDE0055 // Disable formatting
            result |= (leftStick.Dx < -StickButtonThreshold) ? ControllerKeys.LStickLeft  : result;
            result |= (leftStick.Dx > StickButtonThreshold)  ? ControllerKeys.LStickRight : result;
            result |= (leftStick.Dy < -StickButtonThreshold) ? ControllerKeys.LStickDown  : result;
            result |= (leftStick.Dy > StickButtonThreshold)  ? ControllerKeys.LStickUp    : result;

            result |= (rightStick.Dx < -StickButtonThreshold) ? ControllerKeys.RStickLeft  : result;
            result |= (rightStick.Dx > StickButtonThreshold)  ? ControllerKeys.RStickRight : result;
            result |= (rightStick.Dy < -StickButtonThreshold) ? ControllerKeys.RStickDown  : result;
            result |= (rightStick.Dy > StickButtonThreshold)  ? ControllerKeys.RStickUp    : result;
#pragma warning restore IDE0055

            return result;
        }

        internal ulong GetTimestampTicks()
        {
            return (ulong)PerformanceCounter.ElapsedMilliseconds * 19200;
        }
    }
}
