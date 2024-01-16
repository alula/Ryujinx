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
using Ryujinx.HLE.HOS.Services.Apm;
using Ryujinx.HLE.HOS.Services.Hid;
using Ryujinx.HLE.HOS.Services.Sm;
using Ryujinx.HLE.Loaders.Processes;
using Ryujinx.HLE.UI;
using Ryujinx.Memory;
using System;
using System.Collections.Generic;

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

        public static readonly HashSet<string> ServiceLLEBlacklist = new() {
            "bgtc:t",
            "notif:a",
            "notif:s",
            "npns:s",
            "npns:u",
            "set:cal",
            "set:fd",
            "set:sys",
            "acc:su",
            "acc:u0",
            "acc:u1",
            "acc:aa",
        };

        public void BootSystem()
        {
            if (!EnableServiceLLE)
                return;

            Logger.Info?.Print(LogClass.Application, "Booting services...");

            LoadSystemTitleId(SystemProgramId.Settings.Value);
            WaitServiceRegistered("set:sys");
            // LoadSystemTitleId(SystemProgramId.Pgl.Value);
            // WaitServiceRegistered("pgl");
            // LoadSystemTitleId(SystemProgramId.Ns.Value);
            // WaitServiceRegistered("ns:am");
            // LoadSystemTitleId(SystemProgramId.Ssl.Value);
            // WaitServiceRegistered("ssl");
            LoadSystemTitleId(SystemProgramId.Glue.Value);
            WaitServiceRegistered("bgtc:t");
            LoadSystemTitleId(SystemProgramId.Account.Value);
            WaitServiceRegistered("acc:aa");
            LoadSystemTitleId(SystemProgramId.Npns.Value);
            // LoadSystemTitleId(0x0100000000001000); // qlaunch
        }

        public bool LoadCart(string exeFsDir, string romFsFile = null)
        {
            return Processes.LoadUnpackedNca(exeFsDir, romFsFile);
        }

        public bool LoadXci(string xciFile)
        {
            return Processes.LoadXci(xciFile);
        }

        public bool LoadNca(string ncaFile)
        {
            return Processes.LoadNca(ncaFile);
        }

        public bool LoadNsp(string nspFile)
        {
            return Processes.LoadNsp(nspFile);
        }

        public bool LoadProgram(string fileName)
        {
            return Processes.LoadNxo(fileName);
        }

        public bool LoadSystemTitleId(ulong titleId)
        {
            string jitPath = System.ContentManager.GetInstalledContentPath(titleId, StorageId.BuiltInSystem, NcaContentType.Program);
            string filePath = VirtualFileSystem.SwitchPathToSystemPath(jitPath);

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
            System.SetVolume(Math.Clamp(volume, 0, 1));
        }

        public float GetVolume()
        {
            return System.GetVolume();
        }

        public void EnableCheats()
        {
            ModLoader.EnableCheats(Processes.ActiveApplication.ProgramId, TamperMachine);
        }

        public bool IsAudioMuted()
        {
            return System.GetVolume() == 0;
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
