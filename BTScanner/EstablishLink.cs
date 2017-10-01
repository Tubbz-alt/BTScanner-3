using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace tiota
{
    class EstablishLink
    {
        byte status = 0xFF;
        ushort connectionHandle = 0;
        string macAddress = null;

        public EstablishLink(byte[] data)
        {
            status = data[0];

            byte[] mac = new byte[6];
            Array.Copy(data, 2, mac, 0, 6);
            macAddress = BitConverter.ToString(mac).Replace("-", string.Empty);

            connectionHandle = (ushort)(data[8] + (data[9]<<8));
        }

        public byte Status { get { return status; } }

        public string MacAddress { get { return macAddress; } }

        public ushort ConnectionHandle { get { return connectionHandle; } }

        public override string ToString()
        {
            string s = this.GetType().Name + Environment.NewLine;
            s += "\tStatus = " + status + Environment.NewLine;
            s += "\tMac Address = " + macAddress.ToString() + Environment.NewLine;
            s += "\tHandle " + connectionHandle.ToString("X4") + Environment.NewLine;
            return s;
        }
    }
}
