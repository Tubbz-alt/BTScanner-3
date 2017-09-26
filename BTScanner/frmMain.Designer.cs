namespace tiota
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.grdTargets = new System.Windows.Forms.DataGridView();
            this.tmrDiscover = new System.Windows.Forms.Timer(this.components);
            this.cmbPorts = new System.Windows.Forms.ComboBox();
            this.lblCom = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnScan = new System.Windows.Forms.Button();
            this.btnInit = new System.Windows.Forms.Button();
            this.colMAC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDeviceName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRSSI = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLastSeen = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTest = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.numInterval = new System.Windows.Forms.NumericUpDown();
            this.lblInterval = new System.Windows.Forms.Label();
            this.txtCsvFile = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.grdTargets)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numInterval)).BeginInit();
            this.SuspendLayout();
            // 
            // grdTargets
            // 
            this.grdTargets.AllowUserToAddRows = false;
            this.grdTargets.AllowUserToDeleteRows = false;
            this.grdTargets.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grdTargets.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.grdTargets.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdTargets.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colMAC,
            this.colDeviceName,
            this.colRSSI,
            this.colLastSeen,
            this.colTest});
            this.grdTargets.Location = new System.Drawing.Point(12, 12);
            this.grdTargets.Name = "grdTargets";
            this.grdTargets.RowHeadersVisible = false;
            this.grdTargets.Size = new System.Drawing.Size(757, 548);
            this.grdTargets.TabIndex = 6;
            // 
            // tmrDiscover
            // 
            this.tmrDiscover.Tick += new System.EventHandler(this.tmrProgressUpdate_Tick);
            // 
            // cmbPorts
            // 
            this.cmbPorts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbPorts.FormattingEnabled = true;
            this.cmbPorts.Location = new System.Drawing.Point(894, 24);
            this.cmbPorts.Name = "cmbPorts";
            this.cmbPorts.Size = new System.Drawing.Size(121, 28);
            this.cmbPorts.TabIndex = 5;
            this.cmbPorts.TextChanged += new System.EventHandler(this.cmbPorts_TextChanged);
            this.cmbPorts.MouseClick += new System.Windows.Forms.MouseEventHandler(this.cmbPorts_MouseClick);
            // 
            // lblCom
            // 
            this.lblCom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCom.Location = new System.Drawing.Point(798, 29);
            this.lblCom.Name = "lblCom";
            this.lblCom.Size = new System.Drawing.Size(90, 23);
            this.lblCom.TabIndex = 4;
            this.lblCom.Text = "Com Port";
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.Location = new System.Drawing.Point(802, 301);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(213, 50);
            this.btnStart.TabIndex = 3;
            this.btnStart.Text = "&Start Test";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStop.Location = new System.Drawing.Point(802, 357);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(213, 50);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "S&top";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnScan
            // 
            this.btnScan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnScan.Location = new System.Drawing.Point(802, 159);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(213, 50);
            this.btnScan.TabIndex = 1;
            this.btnScan.Text = "S&can";
            this.btnScan.UseVisualStyleBackColor = true;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // btnInit
            // 
            this.btnInit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInit.Location = new System.Drawing.Point(802, 103);
            this.btnInit.Name = "btnInit";
            this.btnInit.Size = new System.Drawing.Size(213, 50);
            this.btnInit.TabIndex = 0;
            this.btnInit.Text = "&Init";
            this.btnInit.UseVisualStyleBackColor = true;
            this.btnInit.Click += new System.EventHandler(this.btnInit_Click);
            // 
            // colMAC
            // 
            this.colMAC.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colMAC.HeaderText = "MAC";
            this.colMAC.MaxInputLength = 12;
            this.colMAC.Name = "colMAC";
            this.colMAC.ReadOnly = true;
            this.colMAC.Width = 80;
            // 
            // colDeviceName
            // 
            this.colDeviceName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colDeviceName.HeaderText = "Device Name";
            this.colDeviceName.MaxInputLength = 20;
            this.colDeviceName.Name = "colDeviceName";
            this.colDeviceName.ReadOnly = true;
            // 
            // colRSSI
            // 
            this.colRSSI.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.colRSSI.HeaderText = "RSSI";
            this.colRSSI.MaxInputLength = 4;
            this.colRSSI.Name = "colRSSI";
            this.colRSSI.ReadOnly = true;
            this.colRSSI.Width = 84;
            // 
            // colLastSeen
            // 
            this.colLastSeen.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colLastSeen.HeaderText = "LastSeen";
            this.colLastSeen.Name = "colLastSeen";
            this.colLastSeen.ReadOnly = true;
            this.colLastSeen.Width = 114;
            // 
            // colTest
            // 
            this.colTest.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.colTest.HeaderText = "Test";
            this.colTest.Name = "colTest";
            this.colTest.Width = 46;
            // 
            // numInterval
            // 
            this.numInterval.Location = new System.Drawing.Point(895, 413);
            this.numInterval.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.numInterval.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.numInterval.Name = "numInterval";
            this.numInterval.Size = new System.Drawing.Size(120, 26);
            this.numInterval.TabIndex = 7;
            this.numInterval.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // lblInterval
            // 
            this.lblInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInterval.Location = new System.Drawing.Point(803, 415);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(90, 23);
            this.lblInterval.TabIndex = 8;
            this.lblInterval.Text = "Interval [s]";
            // 
            // txtCsvFile
            // 
            this.txtCsvFile.Location = new System.Drawing.Point(802, 480);
            this.txtCsvFile.Name = "txtCsvFile";
            this.txtCsvFile.Size = new System.Drawing.Size(213, 26);
            this.txtCsvFile.TabIndex = 10;
            this.txtCsvFile.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.txtCsvFile_MouseDoubleClick);
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(1031, 577);
            this.Controls.Add(this.txtCsvFile);
            this.Controls.Add(this.lblInterval);
            this.Controls.Add(this.numInterval);
            this.Controls.Add(this.btnInit);
            this.Controls.Add(this.btnScan);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.lblCom);
            this.Controls.Add(this.cmbPorts);
            this.Controls.Add(this.grdTargets);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(1053, 452);
            this.Name = "frmMain";
            this.Text = "Advertising Tester";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.grdTargets)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numInterval)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView grdTargets;
        private System.Windows.Forms.Timer tmrDiscover;
        private System.Windows.Forms.ComboBox cmbPorts;
        private System.Windows.Forms.Label lblCom;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.Button btnInit;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMAC;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDeviceName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRSSI;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLastSeen;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colTest;
        private System.Windows.Forms.NumericUpDown numInterval;
        private System.Windows.Forms.Label lblInterval;
        private System.Windows.Forms.TextBox txtCsvFile;
    }
}

