using System;
using System.Collections.Generic;

namespace Ryujinx.HLE.HOS.Services.Am.AppletAE
{
    internal class AppletChannel
    {
        private readonly IAppletFifo<byte[]> _data;

        public event EventHandler DataAvailable;

        public AppletChannel()
            : this(new AppletFifo<byte[]>())
        { }

        public AppletChannel(
            IAppletFifo<byte[]> data)
        {
            _data = data;

            _data.DataAvailable += OnDataAvailable;
        }

        private void OnDataAvailable(object sender, EventArgs e)
        {
            DataAvailable?.Invoke(this, null);
        }

        public void PushData(byte[] item)
        {
            if (!this.TryPushData(item))
            {
                // TODO(jduncanator): Throw a proper exception
                throw new InvalidOperationException();
            }
        }

        public bool TryPushData(byte[] item)
        {
            return _data.TryAdd(item);
        }

        public byte[] PopData()
        {
            if (this.TryPopData(out byte[] item))
            {
                return item;
            }

            throw new InvalidOperationException("Input data empty.");
        }

        public bool TryPopData(out byte[] item)
        {
            return _data.TryTake(out item);
        }

        public void InsertFrontData(byte[] item)
        {
            if (!this.TryInsertFrontData(item))
            {
                // TODO: Throw a proper exception
                throw new InvalidOperationException();
            }
        }

        public bool TryInsertFrontData(byte[] item)
        {
            List<byte[]> items = new List<byte[]>();
            while (_data.TryTake(out byte[] i))
            {
                items.Add(i);
            }

            items.Insert(0, item);

            foreach (byte[] i in items)
            {
                _data.TryAdd(i);
            }

            return true;
        }
    }
}
