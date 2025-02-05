using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Common;
using Ryujinx.HLE.HOS.Services.Hid.Types.SharedMemory.Button;
using System;
using Ryujinx.HLE.HOS.Kernel.Threading;

namespace Ryujinx.HLE.HOS.Services.Hid
{
    public enum ButtonDeviceType
    {
        HomeButton,
        SleepButton,
        CaptureButton,
    }

    public class ButtonDevice : BaseDevice
    {
        private ButtonDeviceType _type;
        private KEvent _event;
        private bool isDown;


        public ButtonDevice(Switch device, bool active, ButtonDeviceType type) : base(device, active)
        {
            _type = type;
            _event = new KEvent(device.System.KernelContext);
        }

        internal ref KEvent GetEvent()
        {
            return ref _event;
        }

        private ref RingLifo<ButtonState> GetButtonStateLifo()
        {
            switch (_type)
            {
                case ButtonDeviceType.HomeButton:
                    return ref _device.Hid.SharedMemory.HomeButton;
                case ButtonDeviceType.SleepButton:
                    return ref _device.Hid.SharedMemory.SleepButton;
                case ButtonDeviceType.CaptureButton:
                    return ref _device.Hid.SharedMemory.CaptureButton;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_type));
            }
        }

        public void Update(bool state)
        {
            ref RingLifo<ButtonState> lifo = ref GetButtonStateLifo();

            if (!Active)
            {
                lifo.Clear();

                return;
            }

            bool shouldSignal = state != isDown;
            isDown = state;

            if (!shouldSignal)
            {
                return;
            }

            ref ButtonState previousEntry = ref lifo.GetCurrentEntryRef();

            ButtonState newState = new()
            {
                SamplingNumber = previousEntry.SamplingNumber + 1,
                Buttons = state ? 1UL : 0UL
            };

            lifo.Write(ref newState);

            _event.ReadableEvent.Signal();
        }
    }
}
