using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace BTScanner
{
    /// <summary>
    /// This class manages a dictoinaty of "BleDevice" objects
    /// </summary>
    class BleDevices 
    {
        static Dictionary<string, BleDevice> deviceList = new Dictionary<string, BleDevice>();
        
        public static void Clear()
        {
            deviceList.Clear();
        }

        public BleDevice this[string key]
        {
            get
            {
                if (deviceList.ContainsKey(key))
                    return deviceList[key];
                return null;
            }
            set
            {
                Add(value);
            }
        }

        #region public static methods
        public static bool Add(BleDevice device)
        {
            if (deviceList.ContainsKey(device.MacAddress.ToString()))
            {
                deviceList[device.MacAddress.ToString()].Name = device.Name;
                return false;
            }

            deviceList.Add(device.MacAddress.ToString(), device);
            return true;
        }

        public static BleDevice GetDeviceByMac(string mac)
        {
            if (!deviceList.ContainsKey(mac))
            {
                return null;
            }

            return deviceList[mac];
        }

        public static List<BleDevice> GetDevices()
        {
            List<BleDevice> list = new List<BleDevice>();
            foreach (KeyValuePair<string, BleDevice> device in deviceList)
            {
                list.Add(device.Value);
            }
            return list;
        }

        public static void UpdateName(string mac, string name)
        {
            if (deviceList.ContainsKey(mac))
            {
                deviceList[mac].Name = name;
            }
        } 
        #endregion
    }


    public class BleDevice
    {
        public string MacAddress { get; }
        public string Name { get; set; }
        public ushort Handle { get; set; }
        public bool Connected { get; set; }
        public bool Upgarded { get; set; }

        public BleDevice (string MacAddress, string Name)
        {
            this.MacAddress = MacAddress;
            this.Name = Name;
            Handle = 0xffff;
            Connected = false;
            Upgarded = false;
        }

        public byte[] MacBytes()
        {
            return (BleDevice.StringToByteArray(MacAddress));
        }

        
        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
