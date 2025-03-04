using Ryujinx.Common;
using Ryujinx.Common.Configuration;
using Ryujinx.Common.Logging;
using Ryujinx.Common.PreciseSleep;
using Ryujinx.Graphics.GAL;
using Ryujinx.Graphics.Gpu;
using Ryujinx.HLE.HOS.Kernel.Process;
using Ryujinx.HLE.HOS.Services.Nv.NvDrvServices.NvMap;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Ryujinx.HLE.HOS.Services.SurfaceFlinger
{
    class SurfaceFlinger : IConsumerListener, IDisposable
    {
        private const int TargetFps = 60;

        private readonly Switch _device;

        private readonly Dictionary<long, Layer> _layers;

        private bool _isRunning;

        private readonly Thread _composerThread;

        private readonly AutoResetEvent _event = new(false);
        private readonly AutoResetEvent _nextFrameEvent = new(true);
        private long _ticks;
        private long _ticksPerFrame;
        private readonly long _spinTicks;
        private readonly long _1msTicks;

        private int _swapInterval;
        private int _swapIntervalDelay;

        private readonly object _lock = new();

        private class Layer
        {
            public int ProducerBinderId;
            public IGraphicBufferProducer Producer;
            public BufferItemConsumer Consumer;
            public BufferQueueCore Core;
            public ulong Owner;
            public LayerState State;
            public long ZIndex;
            public bool Visible;
        }

        private class TextureCallbackInformation
        {
            public Layer Layer;
            public BufferItem Item;
        }

        public SurfaceFlinger(Switch device)
        {
            _device = device;
            _layers = new Dictionary<long, Layer>();

            _composerThread = new Thread(HandleComposition)
            {
                Name = "SurfaceFlinger.Composer",
                Priority = ThreadPriority.AboveNormal
            };

            _ticks = 0;
            _spinTicks = Stopwatch.Frequency / 500;
            _1msTicks = Stopwatch.Frequency / 1000;

            UpdateSwapInterval(1);

            _composerThread.Start();
        }

        private void UpdateSwapInterval(int swapInterval)
        {
            _swapInterval = swapInterval;

            // If the swap interval is 0, Game VSync is disabled.
            if (_swapInterval == 0)
            {
                _nextFrameEvent.Set();
                _ticksPerFrame = 1;
            }
            else
            {
                _ticksPerFrame = Stopwatch.Frequency / TargetFps;
            }
        }

        public IGraphicBufferProducer CreateLayer(out long layerId, ulong pid, LayerState initialState = LayerState.ManagedClosed)
        {
            layerId = 1;

            lock (_lock)
            {
                foreach (KeyValuePair<long, Layer> pair in _layers)
                {
                    if (pair.Key >= layerId)
                    {
                        layerId = pair.Key + 1;
                    }
                }
            }

            CreateLayerFromId(pid, layerId, initialState);

            return GetProducerByLayerId(layerId);
        }

        private void CreateLayerFromId(ulong pid, long layerId, LayerState initialState)
        {
            lock (_lock)
            {
                BufferQueueCore core = BufferQueue.CreateBufferQueue(_device, pid, out BufferQueueProducer producer, out BufferQueueConsumer consumer);

                core.BufferQueued += () =>
                {
                    _nextFrameEvent.Set();
                };

                _layers.Add(layerId, new Layer
                {
                    ProducerBinderId = HOSBinderDriverServer.RegisterBinderObject(producer),
                    Producer = producer,
                    Consumer = new BufferItemConsumer(_device, consumer, 0, -1, false, this),
                    Core = core,
                    Owner = pid,
                    State = initialState,
                    ZIndex = 0,
                    Visible = true
                });
            }
        }

        public Vi.ResultCode OpenLayer(ulong pid, long layerId, out IBinder producer)
        {
            Layer layer = GetLayerByIdLocked(layerId);

            if (layer == null || layer.State != LayerState.ManagedClosed)
            {
                Logger.Warning?.Print(LogClass.SurfaceFlinger, $"Layer {layerId} not found or not closed {layer?.State}");
                producer = null;

                return Vi.ResultCode.InvalidArguments;
            }

            layer.State = LayerState.ManagedOpened;
            producer = layer.Producer;

            return Vi.ResultCode.Success;
        }

        public Vi.ResultCode CloseLayer(long layerId)
        {
            lock (_lock)
            {
                Layer layer = GetLayerByIdLocked(layerId);

                if (layer == null)
                {
                    Logger.Error?.Print(LogClass.SurfaceFlinger, $"Failed to close layer {layerId}");

                    return Vi.ResultCode.InvalidValue;
                }

                CloseLayer(layerId, layer);

                return Vi.ResultCode.Success;
            }
        }

        public Vi.ResultCode DestroyManagedLayer(long layerId)
        {
            lock (_lock)
            {
                Layer layer = GetLayerByIdLocked(layerId);

                if (layer == null)
                {
                    Logger.Error?.Print(LogClass.SurfaceFlinger, $"Failed to destroy managed layer {layerId} (not found)");

                    return Vi.ResultCode.InvalidValue;
                }

                if (layer.State != LayerState.ManagedClosed && layer.State != LayerState.ManagedOpened)
                {
                    Logger.Error?.Print(LogClass.SurfaceFlinger, $"Failed to destroy managed layer {layerId} (permission denied)");

                    return Vi.ResultCode.PermissionDenied;
                }

                HOSBinderDriverServer.UnregisterBinderObject(layer.ProducerBinderId);

                if (_layers.Remove(layerId) && layer.State == LayerState.ManagedOpened)
                {
                    CloseLayer(layerId, layer);
                }

                return Vi.ResultCode.Success;
            }
        }

        public Vi.ResultCode DestroyStrayLayer(long layerId)
        {
            lock (_lock)
            {
                Layer layer = GetLayerByIdLocked(layerId);

                if (layer == null)
                {
                    Logger.Error?.Print(LogClass.SurfaceFlinger, $"Failed to destroy stray layer {layerId} (not found)");

                    return Vi.ResultCode.InvalidValue;
                }

                if (layer.State != LayerState.Stray)
                {
                    Logger.Error?.Print(LogClass.SurfaceFlinger, $"Failed to destroy stray layer {layerId} (permission denied)");

                    return Vi.ResultCode.PermissionDenied;
                }

                HOSBinderDriverServer.UnregisterBinderObject(layer.ProducerBinderId);

                if (_layers.Remove(layerId))
                {
                    CloseLayer(layerId, layer);
                }

                return Vi.ResultCode.Success;
            }
        }

        private void CloseLayer(long layerId, Layer layer)
        {
            Logger.Info?.Print(LogClass.SurfaceFlinger, $"Closing layer {layerId}");

            if (layer.State == LayerState.ManagedOpened)
            {
                layer.State = LayerState.ManagedClosed;
            }
        }

        private Layer GetLayerByIdLocked(long layerId)
        {
            foreach (KeyValuePair<long, Layer> pair in _layers)
            {
                if (pair.Key == layerId)
                {
                    return pair.Value;
                }
            }

            return null;
        }

        public IGraphicBufferProducer GetProducerByLayerId(long layerId)
        {
            lock (_lock)
            {
                Layer layer = GetLayerByIdLocked(layerId);

                if (layer != null)
                {
                    return layer.Producer;
                }
            }

            return null;
        }

        public void SetLayerZ(long layerId, long zIndex)
        {
            lock (_lock)
            {
                if (_layers.TryGetValue(layerId, out Layer layer))
                {
                    layer.ZIndex = zIndex;
                }
            }
        }

        public long GetLayerZ(long layerId)
        {
            lock (_lock)
            {
                if (_layers.TryGetValue(layerId, out Layer layer))
                {
                    return layer.ZIndex;
                }
            }

            return 0;
        }

        public void SetLayerVisibility(long layerId, bool visible)
        {
            lock (_lock)
            {
                if (_layers.TryGetValue(layerId, out Layer layer))
                {
                    layer.Visible = visible;
                }
            }
        }

        private void HandleComposition()
        {
            _isRunning = true;

            long lastTicks = PerformanceCounter.ElapsedTicks;

            while (_isRunning)
            {
                long ticks = PerformanceCounter.ElapsedTicks;

                if (_swapInterval == 0)
                {
                    Compose();

                    _device.System?.SignalVsync();

                    _nextFrameEvent.WaitOne(17);
                    lastTicks = ticks;
                }
                else
                {
                    _ticks += ticks - lastTicks;
                    lastTicks = ticks;

                    if (_ticks >= _ticksPerFrame)
                    {
                        if (_swapIntervalDelay-- == 0)
                        {
                            Compose();

                            // When a frame is presented, delay the next one by its swap interval value.
                            _swapIntervalDelay = Math.Max(0, _swapInterval - 1);
                        }

                        _device.System?.SignalVsync();

                        // Apply a maximum bound of 3 frames to the tick remainder, in case some event causes Ryujinx to pause for a long time or messes with the timer.
                        _ticks = Math.Min(_ticks - _ticksPerFrame, _ticksPerFrame * 3);
                    }

                    // Sleep if possible. If the time til the next frame is too low, spin wait instead.
                    long diff = _ticksPerFrame - (_ticks + PerformanceCounter.ElapsedTicks - ticks);
                    if (diff > 0)
                    {
                        PreciseSleepHelper.SleepUntilTimePoint(_event, PerformanceCounter.ElapsedTicks + diff);

                        diff = _ticksPerFrame - (_ticks + PerformanceCounter.ElapsedTicks - ticks);

                        if (diff < _spinTicks)
                        {
                            PreciseSleepHelper.SpinWaitUntilTimePoint(PerformanceCounter.ElapsedTicks + diff);
                        }
                        else
                        {
                            _event.WaitOne((int)(diff / _1msTicks));
                        }
                    }
                }
            }
        }

        public void Compose()
        {
            int swapInterval = 0;

            lock (_lock)
            {
                var sortedLayers = new (long, Layer)[_layers.Count];
                int i = 0;
                foreach (var (layerId, layer) in _layers)
                {
                    sortedLayers[i++] = (layer.ZIndex, layer);
                }

                // sort by ZIndex then by ID
                Array.Sort(sortedLayers, (a, b) =>
                {
                    var (aId, aLayer) = a;
                    var (bId, bLayer) = b;

                    // invisible first
                    int aVisible = aLayer.Visible ? 0 : 1;
                    int bVisible = bLayer.Visible ? 0 : 1;

                    int cmp = aVisible.CompareTo(bVisible);
                    if (cmp == 0)
                        cmp = aLayer.ZIndex.CompareTo(bLayer.ZIndex);
                    if (cmp == 0)
                        cmp = aId.CompareTo(bId);
                    return cmp;
                });

                foreach (var (layerId, layer) in sortedLayers)
                {
                    if (layer.State == LayerState.NotInitialized || layer.State == LayerState.ManagedClosed)
                        continue;

                    // Logger.Info?.Print(LogClass.SurfaceFlinger, $"Composing layer {layerId}");

                    // cleanup layers of dead processes
                    if (_device.System.KernelContext.Processes.TryGetValue(layer.Owner, out var process))
                    {
                        if (process.State == ProcessState.Exiting || process.State == ProcessState.Exited)
                        {
                            HOSBinderDriverServer.UnregisterBinderObject(layer.ProducerBinderId);

                            if (_layers.Remove(layerId))
                            {
                                CloseLayer(layerId, layer);
                            }

                            continue;
                        }
                    }

                    Status acquireStatus = layer.Consumer.AcquireBuffer(out BufferItem item, 0);

                    if (acquireStatus == Status.Success)
                    {
                        // If device vsync is disabled, reflect the change.
                        if (_device.EnableDeviceVsync)
                        {
                            swapInterval = Math.Max(swapInterval, item.SwapInterval);
                            if (_swapInterval != 0)
                            {
                                UpdateSwapInterval(0);
                            }
                        }

                        // if (layer.Visible)
                        {
                            PostFrameBuffer(layer, item);
                        }
                    }
                    else if (acquireStatus != Status.NoBufferAvailaible && acquireStatus != Status.InvalidOperation)
                    {
                        // throw new InvalidOperationException();
                        Logger.Warning?.Print(LogClass.SurfaceFlinger, $"Failed to acquire buffer for layer {layerId} (status: {acquireStatus})");
                        continue;
                    }
                }

                if (_swapInterval != swapInterval)
                {
                    UpdateSwapInterval(swapInterval);
                }
            }
        }

        private void PostFrameBuffer(Layer layer, BufferItem item)
        {
            int frameBufferWidth = item.GraphicBuffer.Object.Width;
            int frameBufferHeight = item.GraphicBuffer.Object.Height;

            int nvMapHandle = item.GraphicBuffer.Object.Buffer.Surfaces[0].NvMapHandle;

            if (nvMapHandle == 0)
            {
                nvMapHandle = item.GraphicBuffer.Object.Buffer.NvMapId;
            }

            ulong bufferOffset = (ulong)item.GraphicBuffer.Object.Buffer.Surfaces[0].Offset;

            NvMapHandle map = NvMapDeviceFile.GetMapFromHandle(nvMapHandle);

            ulong frameBufferAddress = map.Address + bufferOffset;

            Format format = ConvertColorFormat(item.GraphicBuffer.Object.Buffer.Surfaces[0].ColorFormat);

            byte bytesPerPixel =
                format == Format.B5G6R5Unorm ||
                format == Format.R4G4B4A4Unorm ? (byte)2 : (byte)4;

            int gobBlocksInY = 1 << item.GraphicBuffer.Object.Buffer.Surfaces[0].BlockHeightLog2;

            // Note: Rotation is being ignored.
            Rect cropRect = item.Crop;

            bool flipX = item.Transform.HasFlag(NativeWindowTransform.FlipX);
            bool flipY = item.Transform.HasFlag(NativeWindowTransform.FlipY);

            AspectRatio aspectRatio = _device.Configuration.AspectRatio;
            bool isStretched = aspectRatio == AspectRatio.Stretched;

            ImageCrop crop = new(
                cropRect.Left,
                cropRect.Right,
                cropRect.Top,
                cropRect.Bottom,
                flipX,
                flipY,
                isStretched,
                aspectRatio.ToFloatX(),
                aspectRatio.ToFloatY());

            TextureCallbackInformation textureCallbackInformation = new()
            {
                Layer = layer,
                Item = item,
            };

            if (_device.Gpu.Window.EnqueueFrameThreadSafe(
                layer.Owner,
                frameBufferAddress,
                frameBufferWidth,
                frameBufferHeight,
                0,
                false,
                gobBlocksInY,
                format,
                bytesPerPixel,
                crop,
                AcquireBuffer,
                ReleaseBuffer,
                textureCallbackInformation))
            {
                if (item.Fence.FenceCount == 0)
                {
                    _device.Gpu.Window.SignalFrameReady();
                    _device.Gpu.GPFifo.Interrupt();
                }
                else
                {
                    item.Fence.RegisterCallback(_device.Gpu, (x) =>
                    {
                        _device.Gpu.Window.SignalFrameReady();
                        _device.Gpu.GPFifo.Interrupt();
                    });
                }
            }
            else
            {
                ReleaseBuffer(textureCallbackInformation);
            }
        }

        private void ReleaseBuffer(object obj)
        {
            ReleaseBuffer((TextureCallbackInformation)obj);
        }

        private void ReleaseBuffer(TextureCallbackInformation information)
        {
            AndroidFence fence = AndroidFence.NoFence;

            information.Layer.Consumer.ReleaseBuffer(information.Item, ref fence);
        }

        private void AcquireBuffer(GpuContext ignored, object obj)
        {
            AcquireBuffer((TextureCallbackInformation)obj);
        }

        private void AcquireBuffer(TextureCallbackInformation information)
        {
            information.Item.Fence.WaitForever(_device.Gpu);
        }

        public static Format ConvertColorFormat(ColorFormat colorFormat)
        {
            return colorFormat switch
            {
                ColorFormat.A8B8G8R8 => Format.R8G8B8A8Unorm,
                ColorFormat.X8B8G8R8 => Format.R8G8B8A8Unorm,
                ColorFormat.R5G6B5 => Format.B5G6R5Unorm,
                ColorFormat.A8R8G8B8 => Format.B8G8R8A8Unorm,
                ColorFormat.A4B4G4R4 => Format.R4G4B4A4Unorm,
                _ => throw new NotImplementedException($"Color Format \"{colorFormat}\" not implemented!"),
            };
        }

        public void Dispose()
        {
            _isRunning = false;

            foreach (Layer layer in _layers.Values)
            {
                layer.Core.PrepareForExit();
            }
        }

        public void OnFrameAvailable(ref BufferItem item)
        {
            _device.Statistics.RecordGameFrameTime();
        }

        public void OnFrameReplaced(ref BufferItem item)
        {
            _device.Statistics.RecordGameFrameTime();
        }

        public void OnBuffersReleased() { }
    }
}
