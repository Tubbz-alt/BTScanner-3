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
            this.colMAC = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDeviceName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colRSSI = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colLastSeen = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colTest = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.tmrDiscover = new System.Windows.Forms.Timer(this.components);
            this.cmbPorts = new System.Windows.Forms.ComboBox();
            this.lblCom = new System.Windows.Forms.Label();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.btnScan = new System.Windows.Forms.Button();
            this.btnInit = new System.Windows.Forms.Button();
            this.numInterval = new System.Windows.Forms.NumericUpDown();
            this.lblInterval = new System.Windows.Forms.Label();
            this.txtCsvFile = new System.Windows.Forms.TextBox();
            this.pgrInterval = new System.Windows.Forms.ProgressBar();
            this.lblCsvFilename = new System.Windows.Forms.Label();
            this.btnClear = new System.Windows.Forms.Button();
            this.chkCheckAll = new System.Windows.Forms.CheckBox();
            this.lblRowCount = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.chkCheckConnection = new System.Windows.Forms.CheckBox();
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
            this.colVersion,
            this.colTest});
            this.grdTargets.Location = new System.Drawing.Point(8, 8);
            this.grdTargets.Margin = new System.Windows.Forms.Padding(2);
            this.grdTargets.Name = "grdTargets";
            this.grdTargets.RowHeadersVisible = false;
            this.grdTargets.Size = new System.Drawing.Size(505, 318);
            this.grdTargets.TabIndex = 6;
            this.grdTargets.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.grdTargets_CellContentClick);
            // 
            // colMAC
            // 
            this.colMAC.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colMAC.HeaderText = "MAC";
            this.colMAC.MaxInputLength = 12;
            this.colMAC.Name = "colMAC";
            this.colMAC.ReadOnly = true;
            this.colMAC.Width = 55;
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
            this.colRSSI.Width = 57;
            // 
            // colLastSeen
            // 
            this.colLastSeen.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colLastSeen.HeaderText = "LastSeen";
            this.colLastSeen.Name = "colLastSeen";
            this.colLastSeen.ReadOnly = true;
            this.colLastSeen.Width = 77;
            // 
            // colVersion
            // 
            this.colVersion.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.colVersion.HeaderText = "Version";
            this.colVersion.MinimumWidth = 30;
            this.colVersion.Name = "colVersion";
            this.colVersion.Width = 67;
            // 
            // colTest
            // 
            this.colTest.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.colTest.HeaderText = "Test";
            this.colTest.Name = "colTest";
            this.colTest.Width = 34;
            // 
            // tmrDiscover
            // 
            this.tmrDiscover.Tick += new System.EventHandler(this.tmrScanAdvertising);
            // 
            // cmbPorts
            // 
            this.cmbPorts.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.cmbPorts.FormattingEnabled = true;
            this.cmbPorts.Location = new System.Drawing.Point(596, 16);
            this.cmbPorts.Margin = new System.Windows.Forms.Padding(2);
            this.cmbPorts.Name = "cmbPorts";
            this.cmbPorts.Size = new System.Drawing.Size(82, 21);
            this.cmbPorts.TabIndex = 5;
            this.cmbPorts.TextChanged += new System.EventHandler(this.cmbPorts_TextChanged);
            this.cmbPorts.MouseClick += new System.Windows.Forms.MouseEventHandler(this.cmbPorts_MouseClick);
            // 
            // lblCom
            // 
            this.lblCom.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCom.Location = new System.Drawing.Point(532, 19);
            this.lblCom.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCom.Name = "lblCom";
            this.lblCom.Size = new System.Drawing.Size(60, 15);
            this.lblCom.TabIndex = 4;
            this.lblCom.Text = "Com Port";
            // 
            // btnStart
            // 
            this.btnStart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStart.Location = new System.Drawing.Point(535, 223);
            this.btnStart.Margin = new System.Windows.Forms.Padding(2);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(142, 32);
            this.btnStart.TabIndex = 3;
            this.btnStart.Text = "&Start Test";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnStop.Location = new System.Drawing.Point(535, 260);
            this.btnStop.Margin = new System.Windows.Forms.Padding(2);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(142, 32);
            this.btnStop.TabIndex = 2;
            this.btnStop.Text = "S&top";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // btnScan
            // 
            this.btnScan.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnScan.Location = new System.Drawing.Point(535, 103);
            this.btnScan.Margin = new System.Windows.Forms.Padding(2);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(142, 32);
            this.btnScan.TabIndex = 1;
            this.btnScan.Text = "S&can";
            this.btnScan.UseVisualStyleBackColor = true;
            this.btnScan.Click += new System.EventHandler(this.btnScan_Click);
            // 
            // btnInit
            // 
            this.btnInit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInit.Location = new System.Drawing.Point(535, 67);
            this.btnInit.Margin = new System.Windows.Forms.Padding(2);
            this.btnInit.Name = "btnInit";
            this.btnInit.Size = new System.Drawing.Size(142, 32);
            this.btnInit.TabIndex = 0;
            this.btnInit.Text = "&Init";
            this.btnInit.UseVisualStyleBackColor = true;
            this.btnInit.Click += new System.EventHandler(this.btnInit_Click);
            // 
            // numInterval
            // 
            this.numInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.numInterval.Location = new System.Drawing.Point(597, 295);
            this.numInterval.Margin = new System.Windows.Forms.Padding(2);
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
            this.numInterval.Size = new System.Drawing.Size(80, 20);
            this.numInterval.TabIndex = 7;
            this.numInterval.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            // 
            // lblInterval
            // 
            this.lblInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInterval.Location = new System.Drawing.Point(532, 297);
            this.lblInterval.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblInterval.Name = "lblInterval";
            this.lblInterval.Size = new System.Drawing.Size(60, 15);
            this.lblInterval.TabIndex = 8;
            this.lblInterval.Text = "Interval [s]";
            // 
            // txtCsvFile
            // 
            this.txtCsvFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCsvFile.Location = new System.Drawing.Point(83, 368);
            this.txtCsvFile.Margin = new System.Windows.Forms.Padding(2);
            this.txtCsvFile.Name = "txtCsvFile";
            this.txtCsvFile.Size = new System.Drawing.Size(430, 20);
            this.txtCsvFile.TabIndex = 10;
            this.txtCsvFile.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.txtCsvFile_MouseDoubleClick);
            // 
            // pgrInterval
            // 
            this.pgrInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.pgrInterval.Location = new System.Drawing.Point(535, 321);
            this.pgrInterval.Margin = new System.Windows.Forms.Padding(2);
            this.pgrInterval.Name = "pgrInterval";
            this.pgrInterval.Size = new System.Drawing.Size(142, 25);
            this.pgrInterval.TabIndex = 11;
            // 
            // lblCsvFilename
            // 
            this.lblCsvFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCsvFilename.Location = new System.Drawing.Point(8, 370);
            this.lblCsvFilename.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblCsvFilename.Name = "lblCsvFilename";
            this.lblCsvFilename.Size = new System.Drawing.Size(75, 15);
            this.lblCsvFilename.TabIndex = 12;
            this.lblCsvFilename.Text = "Csv Filename";
            // 
            // btnClear
            // 
            this.btnClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClear.Location = new System.Drawing.Point(535, 361);
            this.btnClear.Margin = new System.Windows.Forms.Padding(2);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(142, 32);
            this.btnClear.TabIndex = 13;
            this.btnClear.Text = "&Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // chkCheckAll
            // 
            this.chkCheckAll.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkCheckAll.AutoSize = true;
            this.chkCheckAll.Location = new System.Drawing.Point(498, 332);
            this.chkCheckAll.Name = "chkCheckAll";
            this.chkCheckAll.Size = new System.Drawing.Size(15, 14);
            this.chkCheckAll.TabIndex = 14;
            this.chkCheckAll.UseVisualStyleBackColor = true;
            this.chkCheckAll.Click += new System.EventHandler(this.chkCheckAll_Click);
            // 
            // lblRowCount
            // 
            this.lblRowCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblRowCount.AutoSize = true;
            this.lblRowCount.Location = new System.Drawing.Point(13, 332);
            this.lblRowCount.Name = "lblRowCount";
            this.lblRowCount.Size = new System.Drawing.Size(13, 13);
            this.lblRowCount.TabIndex = 15;
            this.lblRowCount.Text = "0";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(0, 0);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(80, 17);
            this.checkBox1.TabIndex = 16;
            this.checkBox1.Text = "checkBox1";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // chkCheckConnection
            // 
            this.chkCheckConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkCheckConnection.AutoSize = true;
            this.chkCheckConnection.Location = new System.Drawing.Point(552, 201);
            this.chkCheckConnection.Name = "chkCheckConnection";
            this.chkCheckConnection.Size = new System.Drawing.Size(114, 17);
            this.chkCheckConnection.TabIndex = 17;
            this.chkCheckConnection.Text = "Check Connection";
            this.chkCheckConnection.UseVisualStyleBackColor = true;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CausesValidation = false;
            this.ClientSize = new System.Drawing.Size(691, 403);
            this.Controls.Add(this.chkCheckConnection);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.lblRowCount);
            this.Controls.Add(this.chkCheckAll);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.lblCsvFilename);
            this.Controls.Add(this.pgrInterval);
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
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(707, 376);
            this.Name = "frmMain";
            this.Text = "Advertising Tester";
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
        private System.Windows.Forms.NumericUpDown numInterval;
        private System.Windows.Forms.Label lblInterval;
        private System.Windows.Forms.TextBox txtCsvFile;
        private System.Windows.Forms.ProgressBar pgrInterval;
        private System.Windows.Forms.Label lblCsvFilename;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.CheckBox chkCheckAll;
        private System.Windows.Forms.Label lblRowCount;
        private System.Windows.Forms.DataGridViewTextBoxColumn colMAC;
        private System.Windows.Forms.DataGridViewTextBoxColumn colDeviceName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colRSSI;
        private System.Windows.Forms.DataGridViewTextBoxColumn colLastSeen;
        private System.Windows.Forms.DataGridViewTextBoxColumn colVersion;
        private System.Windows.Forms.DataGridViewCheckBoxColumn colTest;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox chkCheckConnection;
    }
}

