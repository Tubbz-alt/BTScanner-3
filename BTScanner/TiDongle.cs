using NLog;
using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Threading;


namespace tiota
{
    interface IBleCommands
    {
        void SendData(byte[] buffer, int wait = 200, [CallerMemberName]string memberName = "");
        void TiConnect(byte[] MacAddress);
        void TiDisconnectAll();
        void TerminateLinkRequest(ushort Handle);
        void GetParam(byte id);
        void GetParam(byte[] ids);
        void ReadCharValueByHandle(ushort CommHandle, ushort Handle);
        void WriteCharValueByHandle(ushort CommHandle, ushort Handle, byte[] value);
        bool CommandReplyed { get; set; }
        bool WriteResponse { get; }
        void WriteOADByHandle(ushort CommHandle, ushort Handle, byte[] value);
        ushort NextBlock { get; set; }
        void SetTimers(ushort Handle);
    }

    public class TiEventArgs : EventArgs
    {
        public PhysicalAddress Address = null;
        public string Message = string.Empty;

        public TiEventArgs(PhysicalAddress a = null, string msg = "")
        {
            Address = a;
            Message = msg;
        }
    }

    class TiDongle : IBleCommands
    {
        const ushort TI_PROTOCOL_EVENT_OPCODE_INIT_SUCCESS = 0x0600;
        const ushort TI_PROTOCOL_EVENT_OPCODE_ESTABLISH_LINK = 0x0605;
        const ushort TI_PROTOCOL_EVENT_OPCODE_TERMINATED_LINK = 0x0606;
        const ushort TI_PROTOCOL_EVENT_OPCODE_SCAN_COMPLIRE = 0x0601;
        const ushort TI_PROTOCOL_EVENT_OPCODE_SCAN_DEVICE = 0x060D;
        const ushort TI_PROTOCOL_EVENT_OPCODE_COMMAND_STATUS = 0x067f;
        const ushort GAP_HCI_ExtentionCommandStatus = 0x067f;
        const ushort GAP_EstablishLink = 0x605;
        const ushort ATT_HANDLE_VALUE_NOTIFICATION = 0x051b;
        const ushort ATT_READ_RESPONSE = 0x50B;
        const ushort ATT_WRITE_RESPONSE = 0x513;
        const ushort ATT_NEXT_BLOCK_NOTIFICATION = 0x51B;
        const ushort ATT_ERROR_RESPONSE = 0x501;

        public delegate void MyEventHandler(object source, TiEventArgs e);
        public event MyEventHandler OnHardwareInitComplite;
        public event MyEventHandler OnSerchComplite;
        public event MyEventHandler OnLinkEstablished;
        public event MyEventHandler OnLinkTerminated;

        const int TICK_TO_MILISECONDS = 10000;

        public bool Status { get; set; }
        public Byte[] MAC { get; set; }
        protected ISerialPort _port = null;
        IGUI _gui = null;
        Thread _receiveParserThread = null;
        List<byte> rxRelay = new List<byte>();
        byte[] buffer = new byte[255];
        int ByteCount = 0;

        protected bool init_success = false;
        protected bool command_replayed = false;
        protected bool device_connected = false;
        protected bool device_disconnected = false;
        protected bool got_write_response = false;
        ushort nextBloxk = 0xFFFF;
        private Object nextBlockLock = new Object();
        private Object writeResponseLock = new Object();
        private Object commandReplyedLock = new Object();
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public TiDongle(ISerialPort Port, IGUI Gui)
        {
            _receiveParserThread = new Thread(ParseReceive);
            _receiveParserThread.IsBackground = true;
            _port = Port;
            if (_port != null)
            {
                _port.Open();
                FixComunicationProblem();
            }
           
            _gui = Gui;
            init_success = false;
            device_connected = false;
            device_disconnected = false;

            _receiveParserThread.Start();
        }

        ~TiDongle()
        {
            Close();
        }

        public bool CommandReplyed
        {
            get
            {
                lock (commandReplyedLock)
                {
                    return command_replayed;
                }
            }
            set
            {
                lock (commandReplyedLock)
                {
                    command_replayed = value;
                }
            }
        }

        public void Close()
        {
            if (_port != null)
                _port.Close();
        }

        protected virtual void FixComunicationProblem()
        {
            while (_port.BytesToRead > 0)
            {
                int temp;
                temp = _port.ReadByte();
            }

        }

        public virtual void SendData(byte[] buffer, int wait = 0, [CallerMemberName] string memberName = "")
        {
           
            CommandReplyed = false;
            WriteResponse = false;
            if (_port != null) 
                _port.Write(buffer, 0, buffer.Length);
            if (wait > 0)
            {
                DateTime start = DateTime.Now;
                TimeSpan ts = new TimeSpan(wait * TICK_TO_MILISECONDS);
                while (!CommandReplyed && ts > DateTime.Now - start)
                {
                    Thread.Sleep(1);
                }
            }
            //_logger.LogDebug("Tx : " + BitConverter.ToString(buffer) + Environment.NewLine);
        }

        #region Receive and parse replays

        protected virtual void ParseReceive()
        {
            if (_port == null)
                return;
            while (_port.IsOpen)
            {
                try
                {
                    if (_port.BytesToRead > 0)
                    {
                        try
                        {
                            if (ByteCount < TiCommand.API_RECEIVE_HEADER_LENGTH)
                            {
                                _port.Read(buffer, ByteCount, 1);
                                ByteCount++;
                            }
                            else if (ByteCount == TiCommand.API_RECEIVE_HEADER_LENGTH)
                            {
                                int PacketSize = CheckMessageLength(buffer[TiCommand.API_RECEIVE_MSG_SIZE_INDEX], buffer[TiCommand.API_RECEIVE_MSG_OPCODE_INDEX]);
                                if (_port.BytesToRead >= PacketSize)
                                {
                                    for (int i = 0; i < PacketSize; i++)
                                    {
                                        _port.Read(buffer, ByteCount + 1, 1);
                                        ByteCount++;
                                    }
                                    ByteCount = 0;
                                    analyzePacket(new TiCommand(buffer));
                                    Array.Clear(buffer, 0, buffer.Length);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Warn(ex, "Error during receive bytes");
                            ByteCount = 0;
                            Array.Clear(buffer, 0, buffer.Length);
                        }
                    }
                    else
                    {
                        Thread.Sleep(4);
                    }
                }
                catch (Exception ex)
                {
                    logger.Warn(ex, "Error during receive bytes");
                }
            }
        }

        private int CheckMessageLength(byte size, byte opcode)
        {
            if (opcode == 0xff || opcode == 0x13 || opcode == 14)
                return size - 2;
            return size;
        }

        public void analyzePacket(TiCommand Replay)
        {
            if (Replay.type == TiCommand.TI_PROTOCOL_EVENT_TYPE_MESSAGE)
            {
                switch (Replay.eventOpCode)
                {
                    case TI_PROTOCOL_EVENT_OPCODE_INIT_SUCCESS:
                        {
                            HCI_ExtentionCommandStatus h = new HCI_ExtentionCommandStatus(Replay.data);
                            GetParam(0x15);
                            Thread.Sleep(200);
                            GetParam(0x16);
                            Thread.Sleep(200);
                            GetParam(0x1A);
                            Thread.Sleep(200);
                            init_success = true;
                            if (OnHardwareInitComplite != null)
                                OnHardwareInitComplite(this, new TiEventArgs());
                        }
                        break;

                    case TI_PROTOCOL_EVENT_OPCODE_ESTABLISH_LINK:
                        {
                            EstablishLink l1 = new EstablishLink(Replay.data);
                            CommandReplyed = true;
                            BleDevice dps = BleDevices.GetDeviceByMac(l1.MacAddress.ToString());
                            if (dps != null)
                            {
                                dps.Handle = l1.ConnectionHandle;
                                dps.Connected = true;
                                //ReadCharValueByHandle(l1.ConnectionHandle, 0x1f); //type 0x2803
                            }
                            device_connected = true;
                            OnLinkEstablished(this, new TiEventArgs(l1.MacAddress, "TI_PROTOCOL_EVENT_OPCODE_ESTABLISH_LINK"));
                        }
                        break;

                    case ATT_HANDLE_VALUE_NOTIFICATION:
                        HandleValueNotification l = new HandleValueNotification(Replay.data);
                        NextBlock = l.NextBlock;
                        CommandReplyed = true;
                        break;

                    case TI_PROTOCOL_EVENT_OPCODE_TERMINATED_LINK:
                        {
                            TerminateLink terminateLink = new TerminateLink(Replay.data);
                            CommandReplyed = true;
                            if (terminateLink.Status == 0)
                            {
                                List<BleDevice> deviceList = BleDevices.GetDevices();
                                foreach (BleDevice dps in deviceList)
                                {
                                    if (dps.Handle == terminateLink.ConnectionHandle)
                                    {
                                        dps.Handle = 0xFFFF;
                                        dps.Connected = false;
                                        OnLinkTerminated(this, new TiEventArgs(dps.MacAddress, "TI_PROTOCOL_EVENT_OPCODE_TERMINATED_LINK"));
                                    }
                                }
                            }
                            device_disconnected = true;
                        }
                        break;

                    case TI_PROTOCOL_EVENT_OPCODE_COMMAND_STATUS:
                        {
                            HCI_ExtentionCommandStatus r = new HCI_ExtentionCommandStatus(Replay.data);
                            if (r.Status == 0)
                                CommandReplyed = true;
                        }
                        break;
                    case TI_PROTOCOL_EVENT_OPCODE_SCAN_COMPLIRE:
                        DeviceDiscoveryDone scan_complite = new DeviceDiscoveryDone(Replay.data);
                        OnSerchComplite(this, new TiEventArgs(null, "TI_PROTOCOL_EVENT_OPCODE_SCAN_COMPLIRE"));
                        break;

                    case TI_PROTOCOL_EVENT_OPCODE_SCAN_DEVICE:
                        DeviceInformation scan_device = new DeviceInformation(Replay.data);
                        CommandReplyed = true;
                        if ((scan_device.Name != null) && (scan_device.Name.Length > 0))
                        {
                            if (BleDevices.Add(new BleDevice(scan_device.MacAddress, scan_device.Name)))
                            {
                                if (_gui != null)
                                {
                                    _gui.AddDevice(scan_device.MacAddress, scan_device.Name, scan_device.RSSI);
                                }
                            }
                            else
                            {
                                _gui.UpdateDevice(scan_device.MacAddress, scan_device.Name, scan_device.RSSI);
                            }
                        }
                        else
                        {
                            _gui.UpdateDeviceRssi(scan_device.MacAddress, scan_device.RSSI);
                        }
                        break;

                    case ATT_READ_RESPONSE:
                        ReadRsp read_response = new ReadRsp(Replay.data);
                        CommandReplyed = true;
                        break;

                    case ATT_WRITE_RESPONSE:
                        WriteRsp write_response = new WriteRsp(Replay.data);
                        CommandReplyed = true;
                        if (write_response.Status == 0)
                            WriteResponse = true;
                        break;

                    case ATT_ERROR_RESPONSE:

                        ErrorRsp error_response = new ErrorRsp(Replay.data);
                        CommandReplyed = true;
                        break;
                }
            }
        }
        #endregion
        public ushort NextBlock
        {
            get { lock (nextBlockLock) { return nextBloxk; } }
            set { lock (nextBlockLock) { nextBloxk = value; } }
        }



        public bool WriteResponse
        {
            get
            {
                lock (writeResponseLock)
                {
                    return got_write_response;
                }
            }
            set
            {
                lock (writeResponseLock)
                    got_write_response = value;
            }

        }

        public void Discaver()
        {
            byte[] TxBuffer = new byte[] { 1, 4, 0xfe, 3, 3, 1, 0 };
            SendData(TxBuffer);
        }

        public void TiConnect(byte[] MacAddress)
        {
            device_connected = false;

            byte[] TxBuffer = new byte[13];

            TxBuffer[0] = 0x01;
            TxBuffer[1] = 0x09;
            TxBuffer[2] = 0xfe;
            TxBuffer[3] = 0x09;
            TxBuffer[4] = 0x00;
            TxBuffer[5] = 0x00;
            TxBuffer[6] = 0x00;
            MacAddress.CopyTo(TxBuffer, 7);

            SendData(TxBuffer);
        }

        public void TiDisconnectAll()
        {
            for (ushort i = 0; i < 15; i++)
            {
                    TerminateLinkRequest(i);
            }
        }

       
        public void TerminateLinkRequest(ushort Handle)
        {
            byte[] TxBuffer = new byte[13];

            TxBuffer[0] = 0x01;
            TxBuffer[1] = 0x0A;
            TxBuffer[2] = 0xfe;
            TxBuffer[3] = 0x03;
            TxBuffer[4] = (byte)Handle;
            TxBuffer[5] = (byte)(Handle>>8);
            TxBuffer[6] = 0x13; //Remote User Terminated Connection

            SendData(TxBuffer);

        }
        #region USB Module commands
        public void Init()
        {
            init_success = false;
            byte[] TxBuffer = new byte[] { 1,
                0x00, 0xfe,
                0x26,
                08,
                05,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                1, 0, 0, 0 };

            SendData(TxBuffer, 0);
        }

        public void HardReset()
        {
            init_success = false;
            SendData(new byte[] { 1, 0x1d, 0xfc, 1, 1 });
        }

        public void Terminate()
        {
            device_disconnected = false;
            SendData(new byte[] { 1, 0xa, 0xfe, 3, 0, 0, 0x13 });
        }

        public void GetParam(byte id)
        {
            GetParam(new byte[] { id });
        }

        public void GetParam(byte[] ids)
        {
            //01 31 FE 01 16
            byte[] TxBuffer = new byte[4 + ids.Length];
            TxBuffer[0] = 0x01; //(Command)
            TxBuffer[1] = 0x31; //0xFE31 (GAP_GetParam)
            TxBuffer[2] = 0xFE; //0xFE31 (GAP_GetParam)
            TxBuffer[3] = (byte)ids.Length; // Length
            Array.Copy(ids, 0, TxBuffer, 4, ids.Length);
            SendData(TxBuffer);
        }

        public void SetTimers(ushort Handle)
        {
            byte[] TxBuffer = { 0x01, 0x13, 0x20, 0x0e, 0, 0, 0x0c, 0, 0x10, 0, 0, 0, 0x32, 0, 0, 0, 0, 0x10, 0, 0 };
            SendData(TxBuffer);
        }

        public void ReadCharValueByHandle(ushort CommHandle, ushort Handle)
        {
            byte[] TxBuffer = new byte[8];
            TxBuffer[0] = 0x01; //(Command)
            TxBuffer[1] = 0x8A; //0xFD8A (GATT_ReadCharValue)
            TxBuffer[2] = 0xFD; //0xFD8A (GATT_ReadCharValue)
            TxBuffer[3] = 0x04; // Length
            TxBuffer[4] = (byte)CommHandle;
            TxBuffer[5] = (byte)(CommHandle >> 8);
            TxBuffer[6] = (byte)Handle;
            TxBuffer[7] = (byte)(Handle >> 8);

            SendData(TxBuffer);
        }

        public void WriteCharValueByHandle(ushort CommHandle, ushort Handle, byte[] value)
        {
            byte[] TxBuffer = new byte[8 + value.Length];
            TxBuffer[0] = 0x01; //(Command)
            TxBuffer[1] = 0x92; //0xFD92 (GATT_WriteCharValue)
            TxBuffer[2] = 0xFD; //0xFD92 (GATT_WriteCharValue)
            TxBuffer[3] = (byte)(4 + value.Length); // Length
            TxBuffer[4] = (byte)CommHandle;
            TxBuffer[5] = (byte)(CommHandle >> 8);
            TxBuffer[6] = (byte)Handle;
            TxBuffer[7] = (byte)(Handle >> 8);
            Array.Copy(value, 0, TxBuffer, 8, value.Length);
            WriteResponse = false;
            SendData(TxBuffer);

            DateTime start = DateTime.Now;
            TimeSpan ts = new TimeSpan(20 * TICK_TO_MILISECONDS);
            while (!WriteResponse && ts > DateTime.Now - start)
            {
                Thread.Sleep(1);
            }
        }


        public void WriteOADByHandle(ushort CommHandle, ushort Handle, byte[] value)
        {
            byte[] TxBuffer = new byte[8 + value.Length];
            TxBuffer[0] = 0x01; //(Command)
            TxBuffer[1] = 0xB6; //0xFD92 (GATT_WriteCharValue)
            TxBuffer[2] = 0xFD; //0xFD92 (GATT_WriteCharValue)
            TxBuffer[3] = (byte)(4 + value.Length); // Length
            TxBuffer[4] = (byte)CommHandle;
            TxBuffer[5] = (byte)(CommHandle >> 8);
            TxBuffer[6] = (byte)Handle;
            TxBuffer[7] = (byte)(Handle >> 8);
            Array.Copy(value, 0, TxBuffer, 8, value.Length);
            WriteResponse = false;
            SendData(TxBuffer);
            /*
            DateTime start = DateTime.Now;
            TimeSpan ts = new TimeSpan(20 * TICK_TO_MILISECONDS);
            while (!GotWriteResponse && ts > DateTime.Now - start)
            {
                Thread.Sleep(1);
            }*/
        }

        #endregion
    }
}
