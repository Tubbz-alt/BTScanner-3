using System;

namespace tiota
{
    class ErrorRsp
    {
        byte status = 0xFF;
        ushort connectionHandle = 0;
        byte pduLength = 0;
        byte responseOpcode = 0;
        ushort handle = 0xffff;
        byte errorCode = 0;
        byte[] value = null;

        public ErrorRsp(byte[] data)
        {
            status = data[0];
            connectionHandle = (ushort)(data[1] + (data[2] << 8));
            pduLength = data[3];
            responseOpcode = data[4];
            handle = (ushort)(data[5] + (data[6] << 8));
            errorCode = data[7];

            value = new byte[pduLength];
            if (pduLength > 0)
            {
                Array.Copy(data, 4, value, 0, Math.Min(data.Length-4,pduLength));
            }
        }

        public byte Status { get { return status; } }

        public ushort ConnectionHandle { get { return connectionHandle; } }

        public byte PduLength { get { return pduLength; } }

        public byte[] Value { get { return value; } }

        public override string ToString()
        {
            string s = this.GetType().Name + Environment.NewLine;
            s += "\tStatus = " + status + Environment.NewLine;
            s += "\tConnection Handle = " + connectionHandle + Environment.NewLine;
            s += "\tPDU Length = " + pduLength + Environment.NewLine;
            s+= "\t Response Opcode = " + responseOpcode + Environment.NewLine;
            s += "\t Handle = " + handle + Environment.NewLine;
            s += "\t Error code = " + errorCode + Environment.NewLine;
            s += "\tValue = " + BitConverter.ToString(value) + Environment.NewLine;
            return s;
        }
    }
}
