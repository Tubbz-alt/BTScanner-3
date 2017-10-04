using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using static BTScanner.HCICmds;
using static BTScanner.HCICmds.GAPCmds;
using static BTScanner.SoftwareUpgrade;

namespace BTScanner
{
    #region Interfaces definitions
    interface ILog
    {
        void LogDebug(string msg);
        void LogInfo(string msg);
    }

    interface IGUI
    {
        void AddDevice(string MAC, string name, int rssi);
        void UpdateDevice(string MAC, string name, int rssi);
        void UpdateDeviceRssi(string MAC, int rssi);
    } 
    #endregion

    /// <summary>
    /// GUI class
    /// </summary>
    public partial class frmMain : Form, IGUI
    {
        #region const
        private const byte MAX_TIME_TO_INIT_BLE_DONGLE = 5;
        private const byte MAX_TIME_TO_CONNECTION = 5;
        private const byte MAX_TIME_TO_DISCONNECT = 15;
        #endregion

        #region members
        TiDongle _dongle = null;
        private static Logger logger = LogManager.GetCurrentClassLogger();
        bool _in_scan = false;
        bool _in_test = false;
        int timeOutnter = 0;
        private int next_dps_to_connect = -1; // after increment by 1 it will set to the first decvice
        private BleDevice last_connected_device = null;
        private bool _checkConnection = false;
        private string CsvFilename;
        #endregion

        #region Form init
        public frmMain()
        {
            InitializeComponent();

            //GAP_DeviceInit x = new GAP_DeviceInit();
            //byte[] y = x.GetBuffer();

            //GAP_DeviceDiscoveryRequest disc = new GAP_DeviceDiscoveryRequest();
            //y = disc.GetBuffer();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            logger.Info("EApplication started");
            InitGuiData();
            tmrDiscover.Start();
            tmrDiscover.Enabled = false;
            logger.Debug("Staring at :" + DateTime.Now.ToString());
            this.Text += " - Ver " + Application.ProductVersion;
            btnScan.Enabled = false;
            btnStart.Enabled = false;
            btnStop.Enabled = false;
        }

        private void InitGuiData()
        {
            //txtBinFile.Text = Properties.Settings.Default.LastBinFile;
            cmbPorts_MouseClick(this, null);
        }
        protected override void OnClosed(EventArgs e)
        {
            Properties.Settings.Default.Save();
            base.OnClosed(e);
        }
        #endregion

        #region Buttons support
        private void btnScan_Click(object sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnStop.Enabled = false;

            //_dongle.TiDisconnectAll();
            logger.Info("Starting test. Connection test = {0}", chkCheckConnection.Checked.ToString());
            _dongle.Discaver();
        }

        private void btnInit_Click(object sender, EventArgs e)
        {

            CommPort Port = new CommPort(cmbPorts.Text);
            try
            {
                if (_dongle == null)
                    _dongle = new TiDongle(Port, this);
            }
            catch
            {
                MessageBox.Show("Can't open Comm Port, Please select other", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            _dongle.OnHardwareInitComplite += HardwareInitComplite;
            _dongle.OnSerchComplite += ScanComplite;
            _dongle.Init();
            btnScan.Enabled = false;
            btnStart.Enabled = false;
            btnStop.Enabled = false;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            _in_test = true;
            btnInit.Enabled = false;
            btnScan.Enabled = false;
            btnStart.Enabled = false;
            btnStop.Enabled = true;
            btnClear.Enabled = false;
            chkCheckAll.Enabled = false;
            chkCheckConnection.Enabled = false;
            //Send cancel discovery

            _checkConnection = chkCheckConnection.Checked;
            CsvFilename = txtCsvFile.Text;

        DataGridViewCheckBoxCell oCell;
            List<DataGridViewRow> removeRows = new List<DataGridViewRow>();
            BleDevices.Clear();
            foreach (DataGridViewRow row in grdTargets.Rows)
            {
                row.Tag = 0;
                oCell = row.Cells["colTest"] as DataGridViewCheckBoxCell;
                bool bChecked = (null != oCell && null != oCell.Value && true == (bool)oCell.Value);
                if (bChecked != true)
                {
                    removeRows.Add(row);
                }
                else
                {
                    BleDevices.Add(new BleDevice(row.Cells["colMAC"].Value.ToString(), 
                                                row.Cells["colDeviceName"].Value.ToString()));
                }
            }

            foreach (DataGridViewRow row in removeRows)
            {
                grdTargets.Rows.Remove(row);
            }
            UpdateRowCounter();
            tmrDiscover.Interval = 1000;
            tmrDiscover.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            logger.Info("Test stoped");
            tmrDiscover.Stop();
            _dongle.TiDisconnectAll();

            _dongle.TerminateLinkRequest();
            _dongle.HardReset();

            Thread.Sleep(500);
            _dongle.Close();
            _dongle = null;
            _in_test = false;
            btnInit.Enabled = true;
            btnScan.Enabled = false;
            btnStart.Enabled = false;
            btnStop.Enabled = false;
            btnClear.Enabled = true;
            chkCheckAll.Enabled = true;
            chkCheckConnection.Enabled = true; 
        }

        private void cmbPorts_SelectedIndexChanged(object sender, EventArgs e) { }

       
        #endregion

        #region GUI Update
        public void AddDevice(string MAC, string name, int rssi)
        {
            if (this.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { AddDevice(MAC, name, rssi); }));
            }
            else
            {
                byte[] mac = StringToByteArray(MAC);
                
                if ((_in_test == false) /*|| ((mac[5] == 0x04 ) && (mac[4]== 0xa3) && (mac[3] == 0x16) && (mac[2] == 0x04))*/)
                {
                    int i = this.grdTargets.Rows.Add(MAC, name, rssi);
                    if ((mac[5] == 0x04) && (mac[4] == 0xa3) && (mac[3] == 0x16) && (mac[2] == 0x04))
                    {
                        ((DataGridViewCheckBoxCell)grdTargets.Rows[i].Cells["colTest"]).Value = true;
                    }
                    UpdateRowCounter();
                }

            }
        }

        public void UpdateDevice(string MAC, string name, int rssi)
        {
            if (this.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { UpdateDevice(MAC, name, rssi); }));
            }
            else
            {
                foreach (DataGridViewRow row in this.grdTargets.Rows)
                {
                    if (row.Cells["colMAC"].Value.ToString() == MAC.ToString())
                    {
                        row.Cells["colDeviceName"].Value = name;
                        row.Cells["colRSSI"].Value = rssi;
                        
                        row.Cells["colLastSeen"].Value = DateTime.Now.ToShortTimeString();
                        if (row.Tag == null)
                            row.Tag = 0;
                        row.Tag = (int)row.Tag + 1;

                        break;
                    }
                }
            }
        }

        public void UpdateDeviceRssi(string MAC, int rssi)
        {
            if (this.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { UpdateDeviceRssi(MAC, rssi); }));
            }
            else
            {
                foreach (DataGridViewRow row in this.grdTargets.Rows)
                {
                    if (row.Cells["colMAC"].Value.ToString() == MAC.ToString())
                    {
                        row.Cells["colRSSI"].Value = rssi;
                        break;
                    }
                }
            }
        }

        #endregion

        #region Buttons enable state
        private void HardwareInitComplite(object sender, TiEventArgs e)
        {
            if (this.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { HardwareInitComplite(sender, e); }));
            }
            else
            {
                btnScan.Enabled = true;
                btnStart.Enabled = false;
                btnStop.Enabled = false;
            }
        }

        private void ScanComplite(object sender, TiEventArgs e)
        {
            if (grdTargets.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { ScanComplite(sender, e); }));
            }
            else
            {
                _in_scan = false;
                if (_in_test == false)
                {
                    btnScan.Enabled = true;
                    btnStart.Enabled = true;
                    btnStop.Enabled = true;
                }
                else
                {
                    foreach (DataGridViewRow row in grdTargets.Rows)
                    {
                        if (row.Tag == null)
                            row.Tag = 0;
                        BleDevice device = BleDevices.GetDeviceByMac(row.Cells["colMac"].Value.ToString());
                        if ((int)row.Tag > 0 || ((device != null) && (device.Connected)))
                        {
                            row.DefaultCellStyle.BackColor = Color.White;
                        }
                        else
                        {
                            row.DefaultCellStyle.BackColor = Color.OrangeRed; 
                        }
                        if (!string.IsNullOrEmpty(txtCsvFile.Text))
                        {
                            string line = string.Format("{4,3},{0,8},{1,15},{2,4},{5,12},{6,6},{3}" + Environment.NewLine,
                                                                  row.Cells["colMAC"].Value.ToString(),
                                                                  row.Cells["colDeviceName"].Value.ToString(),
                                                                  row.Cells["colRSSI"].Value.ToString(),
                                                                  DateTime.Now.ToString(),
                                                                  (int)row.Tag,
                                                                  (row.Cells["colVersion"].Value == null ? "" : row.Cells["colVersion"].Value.ToString()),
                                                                  device != null ? device.Connected.ToString() : "false");
                            File.AppendAllText(txtCsvFile.Text, line);
                        }
                        row.Tag = 0;
                    }

                    TestConnection();
                }
            }
        }

        private void EnableControlsns()
        {
            if (btnInit.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { EnableControlsns(); }));
            }
            else
            {
                btnInit.Enabled = true;
                btnScan.Enabled = true;
                btnStart.Enabled = true;
                btnStop.Enabled = true;
            }
        }
        #endregion

        #region Events
        private void UpgradeEnded(object source, UpgradeEventArgs e)
        {
            EnableControlsns();
        } 
        #endregion

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private void cmbPorts_MouseClick(object sender, MouseEventArgs e)
        {
            string[] ports = SerialPort.GetPortNames();

            foreach (string port in ports)
            {
                if (!cmbPorts.Items.Contains(port))
                {
                    cmbPorts.Items.Add(port);
                }
            }
            string lastComPortUse = Properties.Settings.Default.LastComPort;
            if (!string.IsNullOrEmpty(lastComPortUse))
            {
                if (cmbPorts.Items.Contains(lastComPortUse))
                {
                    cmbPorts.Text = lastComPortUse;
                }
            }
            if (!string.IsNullOrEmpty(cmbPorts.Text))
            {
                Properties.Settings.Default.LastComPort = cmbPorts.Text;
            }
        }

        private void tmrScanAdvertising(object sender, EventArgs e)
        {
            timeOutnter += 1;
            SetProgress((timeOutnter * 100) / (int)numInterval.Value);
            if ((int)numInterval.Value <= timeOutnter)
            {
                if (_in_scan == false)
                {
                    _dongle.Discaver();
                    
                    _in_scan = true;
                    timeOutnter = 0;
                    SetProgress(0);
                    timeOutnter = 0;
                }
                else
                {
                    _in_scan = false;
                }
            }
        }

        private void TestConnection()
        {
            if (_checkConnection == false)
                return;

            #region Disconnect the last connected device
            if (last_connected_device != null)
            {
                if (last_connected_device.Connected == true)
                {
                    _dongle.TerminateLinkRequest(last_connected_device.Handle);
                }
            }
            #endregion

            #region Connect to the next device
            next_dps_to_connect++;
            if (grdTargets.Rows.Count <= next_dps_to_connect)
            {
                next_dps_to_connect = 0;
            }

            DataGridViewRow row = grdTargets.Rows[next_dps_to_connect];
            BleDevice device = BleDevices.GetDeviceByMac(row.Cells["colMAC"].Value.ToString());
            last_connected_device = device;
            if (device != null)
            {
                if (!device.Connected)
                    _dongle.TiConnect(device.MacBytes());
            }
            #endregion
        }

        private void cmbPorts_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.LastComPort = ((ComboBox)sender).Text;
        }

        private void txtCsvFile_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            SaveFileDialog ofd = new SaveFileDialog();
            ofd.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            ofd.AddExtension = true;
            ofd.CheckPathExists = true;
            ofd.FilterIndex = 1;
            ofd.RestoreDirectory = true;
            ofd.OverwritePrompt = false;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ((TextBox)sender).Text = ofd.FileName;
            }
        }

        private void SetProgress(int value)
        {
            if (pgrInterval.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { SetProgress(value); }));
            }
            else
            {
               
                try
                {
                    if ((value <= pgrInterval.Maximum) && (value >= pgrInterval.Minimum))
                    {
                        pgrInterval.Value = value;
                    }
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "Wrong progress value");
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            grdTargets.Rows.Clear();
            BleDevices.Clear();
            UpdateRowCounter();

        }

        private void UpdateRowCounter()
        {
            if (lblRowCount.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { UpdateRowCounter(); }));
            }
            else
            {
                lblRowCount.Text = grdTargets.Rows.Count.ToString();
            }
        }

        private void chkCheckAll_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in grdTargets.Rows)
            {
                DataGridViewCheckBoxCell oCell = row.Cells["colTest"] as DataGridViewCheckBoxCell;
                oCell.Value = chkCheckAll.Checked;
            }
        }

        private void grdTargets_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            bool newState = true;
            if (e.ColumnIndex == 4)
            {
                grdTargets.Refresh();
                DataGridViewCheckBoxCell cell = (DataGridViewCheckBoxCell)grdTargets.Rows[e.RowIndex].Cells[e.ColumnIndex];
               
                newState = (bool)(cell.Value);// ? false : true;
                if (newState == true)
                {
                    foreach (DataGridViewRow row in grdTargets.Rows)
                    {
                        DataGridViewCheckBoxCell oCell = row.Cells["colTest"] as DataGridViewCheckBoxCell;
                        if (false == (bool)oCell.Value)
                        {
                            newState = false;
                            break;
                        }
                    }
                }
                chkCheckAll.Checked = newState;
            }
        }
    }
}
