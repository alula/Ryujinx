using LibHac;
using Ryujinx.Audio.Integration;
using Ryujinx.Cpu;
using Ryujinx.Horizon.Sdk.Account;
using Ryujinx.Horizon.Sdk.Fs;

namespace Ryujinx.Horizon
{
    public readonly struct HorizonOptions
    {
        public bool IgnoreMissingServices { get; }
        public bool ThrowOnInvalidCommandIds { get; }
        public bool EnableServiceLLE { get; }

        public HorizonClient BcatClient { get; }
        public IFsClient FsClient { get; }
        public IEmulatorAccountManager AccountManager { get; }
        public IHardwareDeviceDriver AudioDeviceDriver { get; }
        public ITickSource TickSource { get; }

        public HorizonOptions(
            bool ignoreMissingServices,
            bool enableServiceLLE,
            HorizonClient bcatClient,
            IFsClient fsClient,
            IEmulatorAccountManager accountManager,
            IHardwareDeviceDriver audioDeviceDriver,
            ITickSource tickSource)
        {
            IgnoreMissingServices = ignoreMissingServices;
            EnableServiceLLE = enableServiceLLE;
            ThrowOnInvalidCommandIds = true;
            BcatClient = bcatClient;
            FsClient = fsClient;
            AccountManager = accountManager;
            AudioDeviceDriver = audioDeviceDriver;
            TickSource = tickSource;
        }
    }
}
