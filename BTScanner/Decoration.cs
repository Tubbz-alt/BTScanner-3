using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace tiota
{
    class NoCheckResponse : TiDongle
    {
        public NoCheckResponse(ISerialPort Port, IGUI Gui) : base(Port, Gui) { }

        public override void SendData(byte[] buffer, int wait = 0, [CallerMemberName] string memberName = "")
        {
            base.CommandReplyed = false;
            base.WriteResponse = false;
            if (base._port != null)
                base._port.Write(buffer, 0, buffer.Length);
        }
    }

    class NoComm : TiDongle
    {
        public NoComm(IGUI Gui) : base(null, Gui)
        { }

        public override void SendData(byte[] buffer, int wait = 0, [CallerMemberName] string memberName = "")
        {
            base.CommandReplyed = false;
            base.WriteResponse = false;
        }

        protected override void FixComunicationProblem() { }

        protected override void ParseReceive() { }
    }
}
