using Ryujinx.Common;
using Ryujinx.Graphics.Gpu;
using Ryujinx.HLE.HOS.Kernel;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvMap;
using Ryujinx.HLE.HOS.Services.Vi.Types;

namespace Ryujinx.HLE.HOS.Services
{
    class ViServer : ServerBase
    {
        private const int TotalFramebuffers = 16;

        private readonly uint _fbWidth;
        private readonly uint _fbHeight;

        private ulong _pid;
        private ulong _fbsBaseAddress;
        private int _bufferNvMapId;
        private SharedBufferMap _bufferMap;

        public ViServer(KernelContext context, string name) : base(context, name)
        {
            _fbWidth = 1280;
            _fbHeight = 720;
        }

        protected override ulong CalculateRequiredHeapSize()
        {
            return GetSharedBufferSize();
        }

        protected override void CustomInit(KernelContext context, ulong pid, ulong heapAddress)
        {
            _pid = pid;
            _fbsBaseAddress = heapAddress;

            context.Device.Gpu.RegisterProcess(pid, KernelStatic.GetCurrentProcess().CpuMemory);

            ulong bufferSize = CalculateFramebufferSize();
            ulong totalSize = bufferSize * TotalFramebuffers;

            KernelStatic.GetCurrentProcess().CpuMemory.Fill(heapAddress, totalSize, 0xff);

            int mapId = NvMapDeviceFile.CreateMap(pid, heapAddress, (int)totalSize);

            _bufferNvMapId = mapId;
            _bufferMap.Count = TotalFramebuffers;

            ulong offset = 0;

            for (int i = 0; i < TotalFramebuffers; i++)
            {
                System.Console.WriteLine($"fb {i} at 0x{(heapAddress + offset):X}");
                _bufferMap.SharedBuffers[i].Offset = offset;
                _bufferMap.SharedBuffers[i].Size = bufferSize;
                _bufferMap.SharedBuffers[i].Width = _fbWidth;
                _bufferMap.SharedBuffers[i].Height = _fbHeight;

                offset += bufferSize;
            }
        }

        public int GetSharedBufferNvMapId()
        {
            return _bufferNvMapId;
        }

        public ulong GetSharedBufferSize()
        {
            return CalculateFramebufferSize() * TotalFramebuffers;
        }

        private ulong CalculateFramebufferSize()
        {
            // Each GOB dimension is 512 bytes x 8 lines.
            // Assume 16 GOBs, for a total of 16 x 8 = 128 lines.
            return BitUtils.AlignUp(_fbWidth * 4, 512u) * BitUtils.AlignUp(_fbHeight, 128u);
        }

        public SharedBufferMap GetSharedBufferMap()
        {
            return _bufferMap;
        }

        public void PresentFramebuffer(GpuContext gpuContext, bool flipX, bool flipY)
        {
            gpuContext.Window.EnqueueFrameThreadSafe(
                _pid,
                _fbsBaseAddress,
                (int)_fbWidth,
                (int)_fbHeight,
                0,
                false,
                16,
                Graphics.GAL.Format.R8G8B8A8Unorm,
                4,
                new Graphics.GAL.ImageCrop(0, (int)_fbWidth, 0, (int)_fbHeight, flipX, flipY, false, 16.0f, 9.0f),
                AcquireFramebuffer,
                ReleaseFramebuffer,
                null);

            gpuContext.Window.SignalFrameReady();
            gpuContext.GPFifo.Interrupt();
        }

        public int GetApplicationLastPresentedFrameHandle(GpuContext gpuContext)
        {
            var texture = gpuContext.Window.GetLastPresentedData();

            var selfAs = KernelStatic.GetProcessByPid(_pid).CpuMemory;

            selfAs.Write(_fbsBaseAddress + _bufferMap.SharedBuffers[1].Offset, texture.Data);

            return 1;
        }

        private static void AcquireFramebuffer(GpuContext context, object data)
        {

        }

        private static void ReleaseFramebuffer(object data)
        {

        }
    }
}