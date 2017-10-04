using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTScanner
{
    class TerminateLink
    {
        byte status = 0xFF;
        ushort connectionHandle = 0;
        byte reason = 0;


        public TerminateLink(byte[] data)
        {
            status = data[0];
            connectionHandle = (ushort)(data[1] + (data[2] << 8));
            reason = data[3];
        }

        public byte Status { get { return status; } }

        public ushort ConnectionHandle { get { return connectionHandle; } }

        public byte Reason { get { return reason; } }

        public override string ToString()
        {
            string s = this.GetType().Name + Environment.NewLine;
            s += "\tStatus = " + status.ToString("X2") + Environment.NewLine;
            s += "\tConnection Handle = " + connectionHandle.ToString("X4") + Environment.NewLine;
            s += "\tReason = " + reason.ToString("X2") + Environment.NewLine;
            return s;
        }
    }
}
