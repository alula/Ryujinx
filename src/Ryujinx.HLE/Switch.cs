using LibHac.Ncm;
using LibHac.Tools.FsSystem.NcaUtils;
using Ryujinx.Audio.Backends.CompatLayer;
using Ryujinx.Audio.Integration;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Graphics.Gpu;
using Ryujinx.HLE.Exceptions;
using Ryujinx.HLE.FileSystem;
using Ryujinx.HLE.HOS;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Kernel.Threading;
using Ryujinx.HLE.HOS.Services.Apm;
using Ryujinx.HLE.HOS.Services.Hid;
using Ryujinx.HLE.HOS.Services.Sm;
using Ryujinx.HLE.Loaders.Processes;
using Ryujinx.HLE.UI;
using Ryujinx.Memory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.HLE
{
    public class Switch : IDisposable
    {
        public HLEConfiguration Configuration { get; }
        public IHardwareDeviceDriver AudioDeviceDriver { get; }
        public MemoryBlock Memory { get; }
        public GpuContext Gpu { get; }
        public VirtualFileSystem FileSystem { get; }
        public HOS.Horizon System { get; }
        public ProcessLoader Processes { get; }
        public PerformanceStatistics Statistics { get; }
        public Hid Hid { get; }
        public TamperMachine TamperMachine { get; }
        public IHostUIHandler UIHandler { get; }

        public bool EnableDeviceVsync { get; set; } = true;
        public bool EnableServiceLLE { get; set; } = true;
        public int LLESystemVersionMajor { get; private set; } = 0;

        public bool IsFrameAvailable => Gpu.Window.IsFrameAvailable;

        public Switch(HLEConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration.GpuRenderer);
            ArgumentNullException.ThrowIfNull(configuration.AudioDeviceDriver);
            ArgumentNullException.ThrowIfNull(configuration.UserChannelPersistence);

            Configuration = configuration;
            FileSystem = Configuration.VirtualFileSystem;
            UIHandler = Configuration.HostUIHandler;

            MemoryAllocationFlags memoryAllocationFlags = configuration.MemoryManagerMode == MemoryManagerMode.SoftwarePageTable
                ? MemoryAllocationFlags.Reserve
                : MemoryAllocationFlags.Reserve | MemoryAllocationFlags.Mirrorable;

#pragma warning disable IDE0055 // Disable formatting
            AudioDeviceDriver = new CompatLayerHardwareDeviceDriver(Configuration.AudioDeviceDriver);
            Memory            = new MemoryBlock(Configuration.MemoryConfiguration.ToDramSize(), memoryAllocationFlags);
            Gpu               = new GpuContext(Configuration.GpuRenderer);
            System            = new HOS.Horizon(this);
            Statistics        = new PerformanceStatistics();
            Hid               = new Hid(this, System.HidStorage);
            Processes         = new ProcessLoader(this);
            TamperMachine     = new TamperMachine();

            System.InitializeServices();
            System.State.SetLanguage(Configuration.SystemLanguage);
            System.State.SetRegion(Configuration.Region);

            EnableDeviceVsync                       = Configuration.EnableVsync;
            EnableServiceLLE                        = Configuration.EnableServiceLLE;
            System.State.DockedMode                 = Configuration.EnableDockedMode;
            System.PerformanceState.PerformanceMode = System.State.DockedMode ? PerformanceMode.Boost : PerformanceMode.Default;
            System.EnablePtc                        = Configuration.EnablePtc;
            System.FsIntegrityCheckLevel            = Configuration.FsIntegrityCheckLevel;
            System.GlobalAccessLogMode              = Configuration.FsGlobalAccessLogMode;
#pragma warning restore IDE0055
        }

        public static bool IsSystemProgramLaunchEnabled(ulong programId, int majorFirmwareVersion)
        {
            if (programId == SystemProgramId.Ns.Value)
            {
                return false; // todo: enable when pm is implemented
            }

            return true;
        }

        public static bool IsBlacklistedForHLE(string serviceName, int majorFirmwareVersion)
        {
            bool glue = IsSystemProgramLaunchEnabled(SystemProgramId.Glue.Value, majorFirmwareVersion);
            bool npns = IsSystemProgramLaunchEnabled(SystemProgramId.Npns.Value, majorFirmwareVersion);
            bool settings = IsSystemProgramLaunchEnabled(SystemProgramId.Settings.Value, majorFirmwareVersion);
            bool account = IsSystemProgramLaunchEnabled(SystemProgramId.Account.Value, majorFirmwareVersion);
            bool ns = IsSystemProgramLaunchEnabled(SystemProgramId.Ns.Value, majorFirmwareVersion);

            bool isBlacklisted = serviceName switch
            {
                // glue
                "bgtc:t" => glue,
                "bgtc:sc" => glue,
                "time:a" => glue && majorFirmwareVersion >= 9,
                "time:r" => glue && majorFirmwareVersion >= 9,
                "time:u" => glue && majorFirmwareVersion >= 9,
                "notif:a" => glue,
                "notif:s" => glue,
                "ectx:w" => glue,
                "ectx:r" => glue,
                "ectx:aw" => glue,
                "pl:u" => glue && majorFirmwareVersion >= 16,
                "arp:w" => glue,
                "arp:r" => glue,
                // npns
                "npns:s" => npns,
                "npns:u" => npns,
                // settings
                "set:cal" => settings,
                "set:fd" => settings,
                "set:sys" => settings,
                // account
                "acc:su" => majorFirmwareVersion >= 13 ? ns : account,
                "acc:u0" => majorFirmwareVersion >= 13 ? ns : account,
                "acc:u1" => majorFirmwareVersion >= 13 ? ns : account,
                "acc:aa" => account,
                "acc:e" => account,
                "acc:e:su" => account,
                "acc:e:u1" => account,
                "acc:e:u2" => account,
                "dauth:0" => account,
                // ns
                "aoc:u" => ns,
                "ns:am2" => ns,
                "ns:dev" => ns,
                "ns:ec" => ns,
                "ns:rid" => ns,
                "ns:rt" => ns,
                "ns:su" => ns,
                "ns:vm" => ns,
                "ns:web" => ns,
                "ovln:rcv" => ns && majorFirmwareVersion < 8,
                "ovln:snd" => ns && majorFirmwareVersion < 8,
                "ns:ro" => ns && majorFirmwareVersion >= 11,
                "ns:sweb" => ns && majorFirmwareVersion >= 15,
                // // fatal
                // "fatal:u",
                // "fatal:p",
                // // am
                // "appletAE",
                // "appletOE",
                // // "idle:sys",
                // // "omm",
                // // "spsm",
                // "tcap",
                // "caps:su",
                // "apm"
                _ => false
            };

            if (isBlacklisted)
                Logger.Info?.Print(LogClass.Application, $"Service {serviceName} is blacklisted for HLE.");
            return isBlacklisted;
        }

        public void BootSystem()
        {
            if (!EnableServiceLLE)
                return;

            LLESystemVersionMajor = System.ContentManager.GetCurrentFirmwareVersion().Major;

            Logger.Info?.Print(LogClass.Application, "Booting services...");

            // TODO: LLE boot2

            LoadSystemProgramIfSupportedOnCurrentFirmware(SystemProgramId.Settings.Value, "set:sys");
            // LoadSystemProgramIfSupportedOnCurrentFirmware(SystemProgramId.Am.Value, "appletAE");
            // LoadSystemProgramIfSupportedOnCurrentFirmware(SystemProgramId.Pgl.Value, "pgl");
            // LoadSystemProgramIfSupportedOnCurrentFirmware(SystemProgramId.Ssl.Value, "ssl");
            LoadSystemProgramIfSupportedOnCurrentFirmware(SystemProgramId.Glue.Value, "bgtc:t");
            LoadSystemProgramIfSupportedOnCurrentFirmware(SystemProgramId.Account.Value, "acc:aa");
            LoadSystemProgramIfSupportedOnCurrentFirmware(SystemProgramId.Ns.Value, "ns:am");
            LoadSystemProgramIfSupportedOnCurrentFirmware(SystemProgramId.Npns.Value, "npns:s");
            // LoadSystemProgramIfSupportedOnCurrentFirmware(SystemProgramId.Fatal.Value, "fatal:u"); // requires bus reimpl

            LoadSystemProgramId(0x010000000000100C);
        }

        public bool LoadCart(string exeFsDir, string romFsFile = null)
        {
            return Processes.LoadUnpackedNca(exeFsDir, romFsFile);
        }

        public bool LoadXci(string xciFile, ulong applicationId = 0)
        {
            return Processes.LoadXci(xciFile, applicationId);
        }

        public bool LoadNca(string ncaFile)
        {
            return Processes.LoadNca(ncaFile);
        }

        public bool LoadNsp(string nspFile, ulong applicationId = 0)
        {
            return Processes.LoadNsp(nspFile, applicationId);
        }

        public bool LoadProgram(string fileName)
        {
            return Processes.LoadNxo(fileName);
        }

        public bool LoadSystemProgramId(ulong programId)
        {
            string contentPath = System.ContentManager.GetInstalledContentPath(programId, StorageId.BuiltInSystem, NcaContentType.Program);
            string filePath = VirtualFileSystem.SwitchPathToSystemPath(contentPath);

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new InvalidSystemResourceException("Specified title ID is not installed on the system.");
            }

            return Processes.LoadNca(filePath);
        }

        public void WaitServiceRegistered(string serviceName)
        {
            Logger.Debug?.Print(LogClass.Service, $"Waiting for {serviceName} to be registered...");
            while (!System.SmRegistry.IsServiceRegistered(serviceName))
            {
                System.SmRegistry.WaitForServiceRegistration();
            }
        }

        private void LoadSystemProgramIfSupportedOnCurrentFirmware(ulong programId, string serviceNameToWaitFor)
        {
            if (IsSystemProgramLaunchEnabled(programId, LLESystemVersionMajor))
            {
                LoadSystemProgramId(programId);
                WaitServiceRegistered(serviceNameToWaitFor);
            }
        }

        public bool WaitFifo()
        {
            return Gpu.GPFifo.WaitForCommands();
        }

        public void ProcessFrame()
        {
            Gpu.ProcessShaderCacheQueue();
            Gpu.Renderer.PreFrame();
            Gpu.GPFifo.DispatchCalls();
        }

        public bool ConsumeFrameAvailable()
        {
            return Gpu.Window.ConsumeFrameAvailable();
        }

        public void PresentFrame(Action swapBuffersCallback)
        {
            Gpu.Window.Present(swapBuffersCallback);
        }

        public void SetVolume(float volume)
        {
            AudioDeviceDriver.Volume = Math.Clamp(volume, 0f, 1f);
        }

        public float GetVolume()
        {
            return AudioDeviceDriver.Volume;
        }

        public void EnableCheats()
        {
            ModLoader.EnableCheats(Processes.ActiveApplication.ProgramId, TamperMachine);
        }

        public bool IsAudioMuted()
        {
            return AudioDeviceDriver.Volume == 0;
        }

        public void UpdateWindowSystemInput()
        {
            System.WindowSystem.ButtonPressTracker.Update();
        }

        public void DumpProcessExecutionState()
        {
            Logger.Info?.Print(LogClass.Application, "--- Process Execution State ---");
            foreach (var process in System.KernelContext.Processes.Values)
            {
                Logger.Info?.Print(LogClass.Application, $"PID {process.Pid}: name='{process.Name}' programID={process.TitleId:X16} state={process.State}");

                var ownThreads = process.HandleTable.GetObjects<KThread>().Where(thread => thread.Owner.Pid == process.Pid);

                foreach (var thread in ownThreads)
                {
                    Logger.Info?.Print(LogClass.Application, $"  UID {thread.ThreadUid}: currCore={thread.CurrentCore} prio={thread.DynamicPriority} aff={thread.AffinityMask} waitingSync={thread.WaitingSync} syncCancelled={thread.SyncCancelled} schedFlags={thread.SchedFlags.StateString()}");
                    thread.PrintGuestStackTrace();
                    thread.PrintGuestRegisterPrintout();
                }

            }
            Logger.Info?.Print(LogClass.Application, "------------------------------");
        }

        public void DisposeGpu()
        {
            Gpu.Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                System.Dispose();
                AudioDeviceDriver.Dispose();
                FileSystem.Dispose();
                Memory.Dispose();
            }
        }
    }
}
