using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Windows.Forms;
using static tiota.HCISerializer.HCICmds;
using static tiota.HCISerializer.HCICmds.GAPCmds;
using static tiota.SoftwareUpgrade;

namespace tiota
{
    #region Interfaces definitions
    interface ILog
    {
        void LogDebug(string msg);
        void LogInfo(string msg);
    }

    interface IGUI
    {
        void AddDevice(PhysicalAddress MAC, string name, int rssi);
        void UpdateDevice(PhysicalAddress MAC, string name, int rssi);
        void UpdateDeviceRssi(PhysicalAddress MAC, int rssi);
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
        bool _first_round = true;
        int timeOutnter = 0;
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
            InitGuiData();
            tmrDiscover.Start();
            tmrDiscover.Enabled = false;
            logger.Debug("Staring at :" + DateTime.Now.ToString());
            this.Text += " - Ver " + Application.ProductVersion;
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
            _dongle.Discaver();
        }

        private void btnInit_Click(object sender, EventArgs e)
        {

            CommPort Port = new CommPort(cmbPorts.Text);
            try
            {
                if (_dongle == null)
                    _dongle = new NoCheckResponse(Port, this);
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
            //Send cancel discovery

            DataGridViewCheckBoxCell oCell;
            List<DataGridViewRow> removeRows = new List<DataGridViewRow>();
            foreach (DataGridViewRow row in grdTargets.Rows)
            {
                row.Tag = 0;
                oCell = row.Cells["colTest"] as DataGridViewCheckBoxCell;
                bool bChecked = (null != oCell && null != oCell.Value && true == (bool)oCell.Value);
                if (bChecked != true)
                {
                    removeRows.Add(row);
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
            tmrDiscover.Stop();
            _dongle.TiDisconnectAll();

            _dongle.Terminate();
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
        }

        private void cmbPorts_SelectedIndexChanged(object sender, EventArgs e) { }

       
        #endregion

        #region GUI Update
        public void AddDevice(PhysicalAddress MAC, string name, int rssi)
        {
            if (this.InvokeRequired)
            {
                Invoke(new MethodInvoker(delegate { AddDevice(MAC, name, rssi); }));
            }
            else
            {
                if (_in_test == false)
                {
                    this.grdTargets.Rows.Add(MAC.ToString(), name, rssi);
                    UpdateRowCounter();
                }

            }
        }

        public void UpdateDevice(PhysicalAddress MAC, string name, int rssi)
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

        public void UpdateDeviceRssi(PhysicalAddress MAC, int rssi)
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
                        if ((int)row.Tag == 0)
                        {
                            row.DefaultCellStyle.BackColor = Color.OrangeRed;
                        }
                        else
                        {
                            row.DefaultCellStyle.BackColor = Color.White;
                        }
                        string line = string.Format("{4,3},{0,8},{1,15},{2,4},{3}" + Environment.NewLine, row.Cells["colMAC"].Value.ToString(),
                                                              row.Cells["colDeviceName"].Value.ToString(),
                                                              row.Cells["colRSSI"].Value.ToString(),
                                                              DateTime.Now.ToString(),
                                                              (int)row.Tag);
                        row.Tag = 0;
                        File.AppendAllText(txtCsvFile.Text, line);
                    }
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

        private void tmrProgressUpdate_Tick(object sender, EventArgs e)
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
                }
            }
        }

        private void cmbPorts_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.LastComPort = ((ComboBox)sender).Text;
        }

        private void txtCsvFile_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            SaveFileDialog ofd = new SaveFileDialog();
            ofd.Filter = "csv files (*.csv)|*.csv|All files (*.*)|*.*";
            ofd.FilterIndex = 2;
            ofd.RestoreDirectory = true;

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
                pgrInterval.Value = value;
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            grdTargets.Rows.Clear();
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
