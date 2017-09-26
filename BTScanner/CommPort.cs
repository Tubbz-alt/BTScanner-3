using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Threading;

namespace tiota
{
    interface ISerialPort
    {
        bool Open(SerialDataReceivedEventHandler ReceivedEventHandler = null);
        bool Close();

        void    FlashRxBuffer();
        byte[]  GetBuffer(int size, int timeout = 100);
        void    Read(byte[] buffer, int offset = 0, int count = 1);
        byte    ReadByte();
        void    Write(byte[] buffer, int offset = 0, int count = 0);

        int     BytesToRead { get; }
        bool    IsOpen { get; }
    }

    class CommPort : ISerialPort
    {
        string _portName = null;
        int _readTimeout = 1000;
        static SerialPort _serialPort = null;
        ILog _logger = null;
        SerialDataReceivedEventHandler _DataReceivedEventHandler = null;
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public CommPort(string Port, int timeout = 1000)
        {
            _portName = Port;
            _readTimeout = timeout;
        }

        public bool Open(SerialDataReceivedEventHandler ReceivedEventHandler = null)
        {
            Close();

            _serialPort = new SerialPort(_portName, 115200 * 8);
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Handshake = Handshake.RequestToSend;
            _serialPort.ReadTimeout = _readTimeout;

            _serialPort.ReceivedBytesThreshold = 1;

            _DataReceivedEventHandler = ReceivedEventHandler;

            if (ReceivedEventHandler != null)
                _serialPort.DataReceived += ReceivedEventHandler;

            _serialPort.Open();
            FlashRxBuffer();
            return true;
        }

        public bool Close()
        {
            if (_serialPort != null)
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();

                if (_DataReceivedEventHandler != null)
                {
                    _serialPort.DataReceived -= _DataReceivedEventHandler;
                    _DataReceivedEventHandler = null;
                }

                _serialPort = null;
            }
            return true;
        }

        public void FlashRxBuffer()
        {
            if (_serialPort.IsOpen)
            {
                while (_serialPort.BytesToRead > 0)
                {
                    int temp;
                    temp = _serialPort.ReadByte();
                }
            }
        }

        public void Read(byte []buffer, int offset=0, int count=1)
        {
            _serialPort.Read(buffer, offset, count);
        }

        public byte ReadByte()
        {
            byte[] OneByte = new byte[1];
            _serialPort.Read(OneByte, 0, 1);
            return OneByte[0];
        }

        public byte[] GetBuffer(int size, int timeout = 100)
        {
            DateTime start = DateTime.Now;
            TimeSpan delta;
            byte[] data = null;
            try
            {
                if (_serialPort.IsOpen)
                {
                    while (_serialPort.BytesToRead < size)
                    {
                        delta = DateTime.Now - start;
                        if (delta.TotalMilliseconds > timeout)
                        {
                            return null;
                        }
                    }
                    data = new byte[size];
                    _serialPort.Read(data, 0, size);
                }
                return data;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error during get buffer");
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public void SendBuffer(byte[] TxBuffer)
        {
            _logger.LogDebug("Tx : " + BitConverter.ToString(TxBuffer));
            _serialPort.Write(TxBuffer, 0, TxBuffer.Length);
        }

        public void Write(byte[] buffer, int offset = 0, int count = 0)
        {
            if (count == 0)
                count = buffer.Length;
            _serialPort.Write(buffer, offset, count);
        }

        public int BytesToRead { get { return _serialPort.BytesToRead; } }

        public bool IsOpen
        {
            get
            { if (_serialPort == null) return false;
                return  _serialPort.IsOpen;
            }
        }

    }
}
