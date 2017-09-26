using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tiota
{
    class HCI_ExtentionCommandStatus 
    {

        byte status = 0xff;
        ushort opcode = 0;
        byte dataLength = 0;
        byte[] parameters = null;

        public HCI_ExtentionCommandStatus(byte[] data)
        {
            status = data[0];
            opcode = (ushort)((data[2] << 8) + data[1]);
            dataLength = data[3];

            parameters = new byte[dataLength];
            Array.Copy(data, 4, parameters, 0, parameters.Length);
        }

        public byte Status {  get { return status; } }
        
        public ushort OpCode { get { return opcode; } }

        public byte DataLength { get { return dataLength; } }

        public byte[] Params { get { return parameters; } }

        public override string ToString()
        {
            string s = this.GetType().Name + Environment.NewLine;
            s += "\tStatus = " + status.ToString("X2") + Environment.NewLine;
            s += "\tData Length = " + dataLength.ToString("X2") + Environment.NewLine;
            s += "\tParameters = " + BitConverter.ToString(parameters) + Environment.NewLine;
            return s;
        }
    }
}
