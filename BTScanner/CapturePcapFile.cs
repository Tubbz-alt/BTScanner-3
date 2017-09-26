using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tiota
{
    class CapturePcapFile
    {
        public enum DIRECTION
        {
            RX,
            TX
        }

        CaptureFileReaderDevice _file = null;

        public CapturePcapFile(string Filename)
        {
            _file = new CaptureFileReaderDevice(Filename);
        }

        #region Packet methods
        public Tuple<DIRECTION, byte[]> GetPacket()
        {
            RawCapture raw = _file.GetNextPacket();
            if (raw == null)
                return null;
            ushort HeaderSize = (ushort)(raw.Data[0] | raw.Data[1] << 8);

            DIRECTION dir = ((raw.Data[0x15] & 0x80) != 0) ? DIRECTION.RX : DIRECTION.TX;
            byte[] data = new byte[raw.Data.Length - HeaderSize];
            Array.Copy(raw.Data, HeaderSize, data, 0, raw.Data.Length - HeaderSize);
            return Tuple.Create<DIRECTION, byte[]>(dir, data);
        }

        public string Parse(byte[] data)
        {
            return new TiCommand(data).ToString();
        } 
        #endregion
    }
}
