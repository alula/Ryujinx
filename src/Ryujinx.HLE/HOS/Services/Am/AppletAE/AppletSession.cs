using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE
{
    internal class AppletSession
    {
        private readonly IAppletFifo<byte[]> _inputData;
        private readonly IAppletFifo<byte[]> _outputData;

        public event EventHandler InDataAvailable;
        public event EventHandler OutDataAvailable;

        public int Length
        {
            get { return _inputData.Count; }
        }

        public AppletSession()
            : this(new AppletFifo<byte[]>(),
                   new AppletFifo<byte[]>())
        { }

        public AppletSession(
            IAppletFifo<byte[]> inputData,
            IAppletFifo<byte[]> outputData)
        {
            _inputData = inputData;
            _outputData = outputData;

            _inputData.DataAvailable += OnInDataAvailable;
            _outputData.DataAvailable += OnOutDataAvailable;
        }

        private void OnInDataAvailable(object sender, EventArgs e)
        {
            InDataAvailable?.Invoke(this, null);
        }

        private void OnOutDataAvailable(object sender, EventArgs e)
        {
            OutDataAvailable?.Invoke(this, null);
        }

        public void PushInData(byte[] item)
        {
            if (!this.TryPushInData(item))
            {
                // TODO(jduncanator): Throw a proper exception
                throw new InvalidOperationException();
            }
        }

        public bool TryPushInData(byte[] item)
        {
            return _inputData.TryAdd(item);
        }

        public void PushOutData(byte[] item)
        {
            if (!this.TryPushOutData(item))
            {
                // TODO(jduncanator): Throw a proper exception
                throw new InvalidOperationException();
            }
        }

        public bool TryPushOutData(byte[] item)
        {
            return _outputData.TryAdd(item);
        }

        public byte[] PopInData()
        {
            if (this.TryPopInData(out byte[] item))
            {
                return item;
            }

            throw new InvalidOperationException("Input data empty.");
        }

        public bool TryPopInData(out byte[] item)
        {
            return _inputData.TryTake(out item);
        }

        public byte[] PopOutData()
        {
            if (this.TryPopOutData(out byte[] item))
            {
                return item;
            }

            throw new InvalidOperationException("Input data empty.");
        }

        public bool TryPopOutData(out byte[] item)
        {
            return _outputData.TryTake(out item);
        }

        public void InsertFrontInData(byte[] item)
        {
            if (!this.TryInsertFrontInData(item))
            {
                // TODO: Throw a proper exception
                throw new InvalidOperationException();
            }
        }

        public bool TryInsertFrontInData(byte[] item)
        {
            List<byte[]> items = new List<byte[]>();
            while (_inputData.TryTake(out byte[] i))
            {
                items.Add(i);
            }

            items.Insert(0, item);

            foreach (byte[] i in items)
            {
                _inputData.TryAdd(i);
            }

            return true;
        }
    }
}
