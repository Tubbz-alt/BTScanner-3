using System;
using System.IO;
using System.Security.Permissions;
using System.Threading;

namespace BTScanner
{
    class SoftwareUpgrade
    {

        #region Class for event args
        public class UpgradeEventArgs : EventArgs
        {
            public string MacAddress = string.Empty;
            public string Message = string.Empty;
            public UPGRADE_STATUS Status = UPGRADE_STATUS.UNKNOWN;

            public UpgradeEventArgs(string MacAddress = null, string msg = "", UPGRADE_STATUS Status = UPGRADE_STATUS.UNKNOWN)
            {
                this.MacAddress = MacAddress;
                Message = msg;
                this.Status = Status;
            }
        } 
        #endregion

        #region delegate for event
        public delegate void UpgradeEventHandler(object source, UpgradeEventArgs e);
        public event UpgradeEventHandler OnUpgradeEnd; 
        #endregion

        #region Constants
        public enum UPGRADE_STATUS
        {
            UNKNOWN,
            NOT_STARTED,
            STARTED,
            COMPLITED,
            BUSY,
            ERROR
        }
        const int OAD_BLOCK_SIZE = 16;
        const int HAL_FLASH_WORD_SIZE = 4;
        const int OAD_BUFFER_SIZE = 2 + OAD_BLOCK_SIZE;
        const int OAD_IMG_HDR_SIZE = 8; 
        #endregion

        private class ImgHdr
        {
            public ushort ver;
            public ushort len;
            public byte imgType;
            public byte[] uid = new byte[4];

            public ImgHdr(ref byte[] Image)
            {
                ver = (ushort)(Image[5] << 8 | Image[4]);
                //len = (ushort)(Image[7] << 8 | Image[6]);
                //len /= HAL_FLASH_WORD_SIZE;
                len = (ushort)(Image.Length / HAL_FLASH_WORD_SIZE);

                imgType = (byte)(((ver & 1) == 1) ? 'B' : 'A');
                Array.Copy(Image, 8, uid, 0, 4);
            }

            public byte[] GetBuffer()
            {
                return new byte[] { //0xFF, 0xFF,  // CRC
                                    BitConverter.GetBytes(ver)[0], BitConverter.GetBytes(ver)[1], // ver
                                    BitConverter.GetBytes(len)[0], BitConverter.GetBytes(len)[1], // Len
                                    uid[0],uid[1],uid[2],uid[3]//,0,0,0,0
                };
            }
        }

        const byte OAD_IMAGE_IDENTITY = 0x12;
        const byte OAD_IMAGE_IDENTITY_NOTIFICATION = 0x13;
        const byte OAD_IMAGE_BLOCK = 0x16;
        const byte OAD_IMAGE_BLOCK_NOTIFICATION = 0x17;
        const int MAX_RETRY = 10;
        const int DELAY_AFTER_REBOOT = 6000;

        const int TICK_TO_MILISECONDS = 10000;

        #region class members
        static bool _busy = false;
        string _macAddress = null;
        IBleCommands _ble = null;
        byte[] _image = null;
        UPGRADE_STATUS _status = UPGRADE_STATUS.NOT_STARTED;
        static Thread _upgradeThread = null;
        int _complitionPercentage = 0;
        object complitionPercentageLock = new object();
        BleDevice dps;
        #endregion

        public SoftwareUpgrade( string MacAddress, IBleCommands Ble, byte[] Image)
        {
            _status = UPGRADE_STATUS.NOT_STARTED;
            _macAddress = MacAddress;
            _ble = Ble;
            _image = Image;
            BleDevices devices = new BleDevices();
            dps = devices[_macAddress];
        }

        #region Public methods and properties
        public void StartUpgrade()
        {
            if (_busy == false)
            {
                _complitionPercentage = 0;
                _status = UPGRADE_STATUS.STARTED;
                Thread thread = new Thread(UpgradeThread);
                thread.Start();
            }
            else
            {
                _status = UPGRADE_STATUS.BUSY;
            }
        }


        [SecurityPermissionAttribute(SecurityAction.Demand, ControlThread = true)]
        public void StopUpgrade()
        {
            if (_upgradeThread != null)
            {
                if (_upgradeThread.IsAlive)
                {
                    _upgradeThread.Abort();
                    _status = UPGRADE_STATUS.NOT_STARTED;
                }
            }
        }

        public bool IsRunning()
        {
            if (_upgradeThread == null)
                return false;
            return (_upgradeThread.IsAlive);
        }

        public int ComplitionPercentage
        {
            get
            {
                return _complitionPercentage;
            }
            set
            {
                _complitionPercentage = value;
            }
        }

        public UPGRADE_STATUS Status { get { return _status; } } 
        #endregion

        #region Private methods
        private void UpgradeThread()
        {
           
            #region Check that the device is "known"
            if (dps == null)
            {
                _status = UPGRADE_STATUS.ERROR;
                OnUpgradeEnd(this, new UpgradeEventArgs(_macAddress, "device not exist", _status));
                return;
            }
            #endregion

            #region Force to enter OAD mode if the name is not starts with "SimpleBLEPer" 
            //_ble.TerminateLinkRequest(dps.Handle);
            if (dps.Name != "SimpleBLEPer")
            {
                // connect
                if (!SD_Connect())
                {
                    _status = UPGRADE_STATUS.ERROR;
                    OnUpgradeEnd(this, new UpgradeEventArgs(_macAddress, "device not connected", _status));
                    return;
                }

                // reboot the device
                _ble.WriteCharValueByHandle(dps.Handle, 0x2B, new byte[] { 0xde, 0 }); // reboot into software download mode 
                WaitForResponse();
                //_ble.WriteCharValueByHandle(dps.Handle, 0x34, new byte[] { 0xde, 0 }); // reboot into software download mode 

                // Wait for the DPS to reboot into software doaload mode
                Thread.Sleep(DELAY_AFTER_REBOOT);
            }
            #endregion

            #region Initiate OAD session
            ushort NextBloxk = 0xFFFF;
            ImgHdr header = new ImgHdr(ref _image);
            int MaxRetry = 0;
           
            do
            {
                _ble.TerminateLinkRequest(dps.Handle);
                #region Connect to the DPS
                SD_Connect();
                //_ble.SetTimers(dps.Handle);

                EnableNotification(dps.Handle);
                WaitForWriteResponse();

                EnableIdentityNotification(dps.Handle);
                WaitForWriteResponse();

                EnableOacNotification(dps.Handle);
                WaitForWriteResponse();

                EnableNotification(dps.Handle);
                WaitForWriteResponse();

                _ble.WriteOADByHandle(dps.Handle, OAD_IMAGE_IDENTITY, header.GetBuffer()); // new byte { 1,0 }); // header.GetBuffer()
                WaitForWriteResponse();
                #endregion

            } while ((MaxRetry++ < MAX_RETRY) && (NextBloxk = GetNextBlock()) == 0xFFFF);
            #endregion

            #region Init download vars
            int nBlocks = header.len/4; // Len is already divided by HAL_FLASH_WORD_SIZE
            int iBlocks = 0;
            int iBytes = 0;
            #endregion

            #region Write all blocks
            do
            {
                
                if (NextBloxk == 0xFFFF)
                {
                    _status = UPGRADE_STATUS.ERROR;
                    OnUpgradeEnd(this, new UpgradeEventArgs(_macAddress, "device timeout. Upgarde failed", _status));
                    return;
                }

                byte[] mOadBuffer = new byte[OAD_BUFFER_SIZE];
                // Prepare block
                mOadBuffer[0] = BitConverter.GetBytes(NextBloxk)[0];
                mOadBuffer[1] = BitConverter.GetBytes(NextBloxk)[1];
                Array.Copy(_image, NextBloxk * OAD_BLOCK_SIZE, mOadBuffer, 2, OAD_BLOCK_SIZE); //OAD_BLOCK_SIZE = 16

                // Send block
                _ble.WriteOADByHandle(dps.Handle, OAD_IMAGE_BLOCK, mOadBuffer);
                
                // Update stats
                iBlocks++;
                iBytes += OAD_BLOCK_SIZE;
                ComplitionPercentage = (int)( (float)(NextBloxk * OAD_BLOCK_SIZE) / _image.Length * 100);
                NextBloxk = GetNextBlock();
               // Thread.Sleep(15);
            } while (NextBloxk < nBlocks) ;
            #endregion
            
            _ble.TerminateLinkRequest(dps.Handle);
            // return 
            _status = UPGRADE_STATUS.COMPLITED;
            OnUpgradeEnd(this, new UpgradeEventArgs(_macAddress, "device upgraded", _status));
            return; 
            #endregion
        }

        private bool SD_Connect()
        {
            _ble.TiConnect(dps.MacBytes());

            // wait for connection
            if (!WaitForConnection())
            {
                return false;
            }
            return true;
        }

        private bool WaitForConnection(int timeoutMilisecond = 2000)
        {
            DateTime start = DateTime.Now;
            TimeSpan ts = new TimeSpan(timeoutMilisecond * TICK_TO_MILISECONDS);
            while (!dps.Connected && ts > DateTime.Now - start)
            {
                Thread.Sleep(1);
            }

            return dps.Connected;
        }

        private bool WaitForResponse(int timeoutMilisecond = 500)
        {
            DateTime start = DateTime.Now;
            TimeSpan ts = new TimeSpan(timeoutMilisecond * TICK_TO_MILISECONDS);
            while (!_ble.CommandReplyed && ts > DateTime.Now - start)
            {
                Thread.Sleep(1);
            }

            return _ble.CommandReplyed;
        }

        private bool WaitForWriteResponse(int timeoutMilisecond = 500)
        {
            DateTime start = DateTime.Now;
            while (!_ble.WriteResponse && timeoutMilisecond > (DateTime.Now - start).TotalMilliseconds)
            {
                Thread.Sleep(1);
            }

            return _ble.WriteResponse;
        }

        private ushort GetNextBlock(int timeoutMilisecond = 5000)
        {
            DateTime start = DateTime.Now;
            TimeSpan ts = new TimeSpan(timeoutMilisecond * TICK_TO_MILISECONDS);
            while ((_ble.NextBlock == 0xFFFF) && ts > DateTime.Now - start)
            {
                Thread.Sleep(1);
            }
            ushort nextblock = _ble.NextBlock;
            _ble.NextBlock = 0xFFFF;
            return nextblock;
        }

        
        private bool SetTimers(ushort Handle)
        {
            _ble.SetTimers(Handle);
            if (!WaitForWriteResponse())
            {
                _status = UPGRADE_STATUS.ERROR;
                OnUpgradeEnd(this, new UpgradeEventArgs(_macAddress, "device not responding to set Timer message", _status));
                return false;
            }
            return true;
        }


        private void EnableNotification(ushort Handle)
        {
            _ble.WriteCharValueByHandle(Handle, 0x0f, new byte[] { 1, 0 });
        }

        private void EnableIdentityNotification(ushort Handle)
        {
            _ble.WriteCharValueByHandle(Handle, OAD_IMAGE_IDENTITY_NOTIFICATION, new byte[] { 1, 0 });
        }

        private void EnableOacNotification(ushort Handle)
        {
            _ble.WriteCharValueByHandle(Handle, OAD_IMAGE_BLOCK_NOTIFICATION, new byte[] { 1, 0 });
        }
        
    }
}
