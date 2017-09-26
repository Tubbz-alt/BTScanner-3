using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace tiota
{
    class DeviceInformation
    {
        
        byte status = 0xFF;
        byte dataLength = 0;
        int rssi = 0;
        byte eventType = 0;
        
        PhysicalAddress macAddress = null;
        string name = null;
        public DeviceInformation(byte[] data)
        {
            status = data[0];
            dataLength = data[10];
            rssi = data[9] - 0x100;
            eventType = data[1];

            byte[] mac = new byte[6];
            Array.Copy(data, 3, mac, 0, 6);
            macAddress = new PhysicalAddress(mac);
            if (eventType == 4)
            {
                byte[] description = new byte[12];
                Array.Copy(data, 13, description, 0, Math.Min(data.Length-13, description.Length));
                name = Encoding.UTF8.GetString(description);
            }
        }

        public byte Status { get { return status; } }

        public PhysicalAddress MacAddress  { get { return macAddress; } }

        public string Name { get { return name; } }

        public int RSSI { get { return rssi; } }

        public override string ToString()
        {
            string s = this.GetType().Name + Environment.NewLine;
            s += "\tStatus = " + status.ToString("X2") + Environment.NewLine;
            s+= "\tEventType = "+ eventType.ToString("X2") + Environment.NewLine;
            s += "\tData Length = " + dataLength.ToString("X2") + Environment.NewLine;
            s += "\tMac Address = " + macAddress.ToString() + Environment.NewLine;
            s += "\tName " + name + Environment.NewLine;
            return s;
        }

    }
}
