using LibHac.Util;
using Ryujinx.Common.Logging;
using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE
{
    internal class AppletSession
    {
        private readonly IAppletFifo<byte[]> _inputData;
        private readonly IAppletFifo<byte[]> _outputData;

        public event EventHandler DataAvailable;

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

            _inputData.DataAvailable += OnDataAvailable;
        }

        private void OnDataAvailable(object sender, EventArgs e)
        {
            DataAvailable?.Invoke(this, null);
        }

        public void Push(byte[] item)
        {
            if (!this.TryPush(item))
            {
                // TODO(jduncanator): Throw a proper exception
                throw new InvalidOperationException();
            }
        }

        public bool TryPush(byte[] item)
        {
            return _outputData.TryAdd(item);
        }

        public byte[] Pop()
        {
            if (this.TryPop(out byte[] item))
            {
                Logger.Info?.Print(LogClass.ServiceAm, $"Pop applet session data: {item.ToHexString()}");
                return item;
            }

            throw new InvalidOperationException("Input data empty.");
        }

        public bool TryPop(out byte[] item)
        {
            return _inputData.TryTake(out item);
        }

        public void InsertFront(byte[] item)
        {
            if (!this.TryInsertFront(item))
            {
                // TODO: Throw a proper exception
                throw new InvalidOperationException();
            }
        }

        public bool TryInsertFront(byte[] item)
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

        /// <summary>
        /// This returns an AppletSession that can be used at the
        /// other end of the pipe. Pushing data into this new session
        /// will put it in the first session's input buffer, and vice
        /// versa.
        /// </summary>
        public AppletSession GetConsumer()
        {
            return new AppletSession(this._outputData, this._inputData);
        }
    }
}
