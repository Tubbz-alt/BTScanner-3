using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BTScanner
{
    class NoCheckResponse : TiDongle
    {
        public NoCheckResponse(ISerialPort Port, IGUI Gui) : base(Port, Gui) { }

        public override bool SendData(byte[] buffer, int wait = 0, [CallerMemberName] string memberName = "")
        {
            base.CommandReplyed = false;
            base.WriteResponse = false;
            if (base._port != null)
                base._port.Write(buffer, 0, buffer.Length);

            return true;
        }
    }

    class NoComm : TiDongle
    {
        public NoComm(IGUI Gui) : base(null, Gui)
        { }

        public override bool SendData(byte[] buffer, int wait = 0, [CallerMemberName] string memberName = "")
        {
            base.CommandReplyed = false;
            base.WriteResponse = false;
            return true;
        }

        protected override void FixComunicationProblem() { }

        protected override void ParseReceive() { }
    }
}
