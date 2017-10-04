using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace BTScanner
{
    class DeviceDiscoveryDone
    {
        byte status = 0xff;
        byte numOfDevices = 0;
        byte dataLength = 0;
        List<PhysicalAddress> devices = new List<PhysicalAddress>();

        public DeviceDiscoveryDone(byte[] data)
        {
            status = data[0];
            numOfDevices = data[1];
            dataLength = data[7];

            byte[] mac = new byte[6];
            for (int i = 0; i < NumOfDevices; i++)
            {
                Array.Copy(data, 2 + (i * 8) + 2, mac, 0, 6);
                devices.Add(new PhysicalAddress(mac));
            }

        }

        public byte Status { get { return status; } }

        public byte NumOfDevices { get { return numOfDevices; } }

        public byte DataLength { get { return dataLength; } }

        public List<PhysicalAddress> Devices
        {
            get
            {
               return devices;
            }
        }

        public override string ToString()
        {
            //string s = base.ToString() + Environment.NewLine;
            string s = this.GetType().Name + Environment.NewLine;
            s += "\tStatus = " + status.ToString("X2") + Environment.NewLine;
            s += "\tData Length = " + dataLength.ToString("X2") + Environment.NewLine;
            s += "\tNumber of devices = " + numOfDevices.ToString("X2") + Environment.NewLine;
            s += "\tMAC List:" + Environment.NewLine;
            foreach (PhysicalAddress a in devices)
            {
                s += "\t\t" + a.ToString() + Environment.NewLine;
            }
            return s;
        }
    }
}

