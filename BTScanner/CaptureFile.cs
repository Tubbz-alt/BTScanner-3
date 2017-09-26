using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace tiota
{
    class CaptureFile
    {
        readonly Regex byteInLineRegex = new Regex(@"(0x[\da - fA - F]{2})+"); //(@"(0x[A-Fa-f0-9][A-Fa-f0-9])");
        string line = null;
        string _filename = null;
        StreamReader _file = null;

        public CaptureFile(string file)
        {
            _filename = file;
            _file =new StreamReader(file);
            line = _file.ReadLine();
        }

        public byte[] GetNextPdu()
        {
            byte[] temp = null;
            do
            {
                temp = GetNextPacket();
                if (temp == null)
                    return null;
            } while (temp.Length <= 27);

            ushort HeaderSize = (ushort)(temp[0] << 8 | temp[1]);

            byte[] pdu = new byte[temp.Length - HeaderSize];
            Array.Copy(temp, HeaderSize, pdu, 0, pdu.Length);
            return pdu;
        }

        private byte[] GetNextPacket()
        {
            List<byte> PacketData = new List<byte>();

            do
            {
                if (line.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                    break;
            } while ((line = _file.ReadLine()) != null);

            while (line.StartsWith("0x"))
            {
                if (line == null)
                    return null;
                AddLineData(line, ref PacketData);
                line = _file.ReadLine();

            }

            return PacketData.ToArray<byte>();
        }

        private void AddLineData(string line, ref List<byte> data)
        {
            MatchCollection matchs = byteInLineRegex.Matches(line);

            foreach (Match m in matchs)
            {
                if (m.Success)
                {
                    byte b = (byte)Int32.Parse(m.Value.Split('x')[1], System.Globalization.NumberStyles.HexNumber);
                    data.Add(b);
                }
            }
        }


    }
}
