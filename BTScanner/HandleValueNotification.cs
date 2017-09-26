using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace tiota
{
    class HandleValueNotification
    {
        byte status = 0xFF;
        ushort connectionHandle = 0;
        byte pduLength = 0;
        ushort nextBlockLength = 0;
        ushort nextBlock = 0;

        public HandleValueNotification(byte[] data)
        {
            status = data[0];
            connectionHandle = (ushort)(data[1] + (data[2] << 8));
            pduLength = data[3];
            nextBlockLength = (ushort)(data[4] + (data[5] << 8));
            nextBlock = (ushort)(data[6] + (data[7] << 8));
        }

        public byte Status { get { return status; } }

        public ushort ConnectionHandle { get { return connectionHandle; } }

        public byte PduLength { get { return pduLength; } }

        public ushort NextBlockLength { get { return nextBlockLength; } }

        public ushort NextBlock { get { return nextBlock; } }

        public override string ToString()
        {
            string s = this.GetType().Name + Environment.NewLine;
            s += "\tStatus = " + status + Environment.NewLine;
            s += "\tConnection Handle = " + connectionHandle + Environment.NewLine;
            s += "\tPDU Length = " + pduLength + Environment.NewLine;
            s += "\t Next Block Length = " + nextBlockLength + Environment.NewLine;
            s += "\t Next Block = " + nextBlock + Environment.NewLine;
            return s;
        }
    }
}
