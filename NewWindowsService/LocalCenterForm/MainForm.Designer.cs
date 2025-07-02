// Đoạn code sửa đổi trong Designer.cs
using System.Windows.Forms;

namespace LocalCenterForm
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.TextBox txtHost;
        private System.Windows.Forms.TextBox txtRoom;
        private System.Windows.Forms.TextBox txtComputer;
        private System.Windows.Forms.Button btnConnect;
        private System.Windows.Forms.Button btnEnableFirewall;
        private System.Windows.Forms.Button btnDisableFirewall;
        private System.Windows.Forms.Button btnShutdown;
        private System.Windows.Forms.Button btnLightOn;
        private System.Windows.Forms.Button btnSaveSettings;
        private System.Windows.Forms.TextBox txtLog;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ListView lvProcesses;
        private System.Windows.Forms.Button btnUpdateTopics;
        private System.Windows.Forms.Label lblCurrentRoom;
        private System.Windows.Forms.Label lblCurrentComputer;
        private System.Windows.Forms.TextBox txtRfid; // TextBox mới để hiển thị mã thẻ RFID
        private System.Windows.Forms.Button btnKillProcess;

        private System.Windows.Forms.ColumnHeader colName;
        private System.Windows.Forms.ColumnHeader colId;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.txtHost = new System.Windows.Forms.TextBox();
            this.txtRoom = new System.Windows.Forms.TextBox();
            this.txtComputer = new System.Windows.Forms.TextBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.btnEnableFirewall = new System.Windows.Forms.Button();
            this.btnDisableFirewall = new System.Windows.Forms.Button();
            this.btnShutdown = new System.Windows.Forms.Button();
            this.btnLightOn = new System.Windows.Forms.Button();
            this.btnSaveSettings = new System.Windows.Forms.Button();
            this.txtLog = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lvProcesses = new System.Windows.Forms.ListView();
            this.btnUpdateTopics = new System.Windows.Forms.Button();
            this.lblCurrentRoom = new System.Windows.Forms.Label();
            this.lblCurrentComputer = new System.Windows.Forms.Label();
            this.txtRfid = new System.Windows.Forms.TextBox();
            this.btnKillProcess = new System.Windows.Forms.Button();
            this.infoDataGrid = new System.Windows.Forms.DataGridView();
            this.KeyColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.KeyColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ValueColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBoxMachines = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.infoDataGrid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.SuspendLayout();
            // 
            // txtHost
            // 
            this.txtHost.Location = new System.Drawing.Point(12, 12);
            this.txtHost.Name = "txtHost";
            this.txtHost.Size = new System.Drawing.Size(200, 22);
            this.txtHost.TabIndex = 0;
            // 
            // txtRoom
            // 
            this.txtRoom.Location = new System.Drawing.Point(12, 38);
            this.txtRoom.Name = "txtRoom";
            this.txtRoom.Size = new System.Drawing.Size(200, 22);
            this.txtRoom.TabIndex = 1;
            // 
            // txtComputer
            // 
            this.txtComputer.Location = new System.Drawing.Point(12, 64);
            this.txtComputer.Name = "txtComputer";
            this.txtComputer.Size = new System.Drawing.Size(200, 22);
            this.txtComputer.TabIndex = 2;
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(113, 90);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(99, 23);
            this.btnConnect.TabIndex = 3;
            this.btnConnect.Text = "Connect MQTT";
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // btnEnableFirewall
            // 
            this.btnEnableFirewall.Location = new System.Drawing.Point(12, 148);
            this.btnEnableFirewall.Name = "btnEnableFirewall";
            this.btnEnableFirewall.Size = new System.Drawing.Size(95, 23);
            this.btnEnableFirewall.TabIndex = 4;
            this.btnEnableFirewall.Text = "Enable FW";
            this.btnEnableFirewall.Click += new System.EventHandler(this.btnEnableFirewall_Click);
            // 
            // btnDisableFirewall
            // 
            this.btnDisableFirewall.Location = new System.Drawing.Point(113, 148);
            this.btnDisableFirewall.Name = "btnDisableFirewall";
            this.btnDisableFirewall.Size = new System.Drawing.Size(95, 23);
            this.btnDisableFirewall.TabIndex = 5;
            this.btnDisableFirewall.Text = "Disable FW";
            this.btnDisableFirewall.Click += new System.EventHandler(this.btnDisableFirewall_Click);
            // 
            // btnShutdown
            // 
            this.btnShutdown.Location = new System.Drawing.Point(12, 177);
            this.btnShutdown.Name = "btnShutdown";
            this.btnShutdown.Size = new System.Drawing.Size(200, 23);
            this.btnShutdown.TabIndex = 6;
            this.btnShutdown.Text = "Shutdown";
            this.btnShutdown.Click += new System.EventHandler(this.btnShutdown_Click);
            // 
            // btnLightOn
            // 
            this.btnLightOn.Location = new System.Drawing.Point(12, 206);
            this.btnLightOn.Name = "btnLightOn";
            this.btnLightOn.Size = new System.Drawing.Size(95, 23);
            this.btnLightOn.TabIndex = 7;
            this.btnLightOn.Text = "Light On";
            this.btnLightOn.Click += new System.EventHandler(this.btnLightOn_Click);
            // 
            // btnSaveSettings
            // 
            this.btnSaveSettings.Location = new System.Drawing.Point(12, 235);
            this.btnSaveSettings.Name = "btnSaveSettings";
            this.btnSaveSettings.Size = new System.Drawing.Size(200, 23);
            this.btnSaveSettings.TabIndex = 8;
            this.btnSaveSettings.Text = "Save Settings";
            this.btnSaveSettings.Click += new System.EventHandler(this.SaveSettings_Click);
            // 
            // txtLog
            // 
            this.txtLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLog.Location = new System.Drawing.Point(560, 281);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(380, 205);
            this.txtLog.TabIndex = 9;
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(12, 335);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(200, 23);
            this.lblStatus.TabIndex = 11;
            this.lblStatus.Text = "MQTT Disconnected";
            // 
            // lvProcesses
            // 
            this.lvProcesses.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvProcesses.FullRowSelect = true;
            this.lvProcesses.GridLines = true;
            this.lvProcesses.HideSelection = false;
            this.lvProcesses.Location = new System.Drawing.Point(560, 30);
            this.lvProcesses.Name = "lvProcesses";
            this.lvProcesses.Size = new System.Drawing.Size(380, 206);
            this.lvProcesses.TabIndex = 12;
            this.lvProcesses.UseCompatibleStateImageBehavior = false;
            this.lvProcesses.View = System.Windows.Forms.View.Details;
            this.lvProcesses.Columns.Add("Process Name", 250, HorizontalAlignment.Left);
            this.lvProcesses.Columns.Add("PID", 100, HorizontalAlignment.Left);
            // 
            // btnUpdateTopics
            // 
            this.btnUpdateTopics.Location = new System.Drawing.Point(12, 119);
            this.btnUpdateTopics.Name = "btnUpdateTopics";
            this.btnUpdateTopics.Size = new System.Drawing.Size(200, 23);
            this.btnUpdateTopics.TabIndex = 13;
            this.btnUpdateTopics.Text = "Update Topics";
            this.btnUpdateTopics.Click += new System.EventHandler(this.btnUpdateTopics_Click);
            // 
            // lblCurrentRoom
            // 
            this.lblCurrentRoom.Location = new System.Drawing.Point(12, 261);
            this.lblCurrentRoom.Name = "lblCurrentRoom";
            this.lblCurrentRoom.Size = new System.Drawing.Size(200, 23);
            this.lblCurrentRoom.TabIndex = 14;
            this.lblCurrentRoom.Text = "Current Room: None";
            // 
            // lblCurrentComputer
            // 
            this.lblCurrentComputer.Location = new System.Drawing.Point(12, 284);
            this.lblCurrentComputer.Name = "lblCurrentComputer";
            this.lblCurrentComputer.Size = new System.Drawing.Size(200, 23);
            this.lblCurrentComputer.TabIndex = 15;
            this.lblCurrentComputer.Text = "Current Computer: None";
            // 
            // txtRfid
            // 
            this.txtRfid.Location = new System.Drawing.Point(12, 310);
            this.txtRfid.Name = "txtRfid";
            this.txtRfid.ReadOnly = true;
            this.txtRfid.Size = new System.Drawing.Size(196, 22);
            this.txtRfid.TabIndex = 16;
            this.txtRfid.Text = "RFID Tag ID here";
            // 
            // btnKillProcess
            // 
            this.btnKillProcess.Location = new System.Drawing.Point(113, 206);
            this.btnKillProcess.Name = "btnKillProcess";
            this.btnKillProcess.Size = new System.Drawing.Size(95, 23);
            this.btnKillProcess.TabIndex = 0;
            this.btnKillProcess.Text = "Kill Process";
            this.btnKillProcess.Click += new System.EventHandler(this.btnKillProcess_Click);
            // 
            // infoDataGrid
            // 
            this.infoDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.infoDataGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.infoDataGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.infoDataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.KeyColumn,
            this.ValueColumn});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.infoDataGrid.DefaultCellStyle = dataGridViewCellStyle2;
            this.infoDataGrid.Location = new System.Drawing.Point(234, 30);
            this.infoDataGrid.Name = "infoDataGrid";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.infoDataGrid.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.infoDataGrid.RowHeadersWidth = 51;
            this.infoDataGrid.RowTemplate.Height = 24;
            this.infoDataGrid.Size = new System.Drawing.Size(307, 206);
            this.infoDataGrid.TabIndex = 17;
            // 
            // KeyColumn
            // 
            this.KeyColumn.HeaderText = "Key";
            this.KeyColumn.MinimumWidth = 6;
            this.KeyColumn.Name = "KeyColumn";
            this.KeyColumn.Width = 125;
            // 
            // ValueColumn
            // 
            this.ValueColumn.HeaderText = "Value";
            this.ValueColumn.MinimumWidth = 6;
            this.ValueColumn.Name = "ValueColumn";
            this.ValueColumn.Width = 125;
            // 
            // dataGridView1
            // 
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.KeyColumn1,
            this.ValueColumn1});
            this.dataGridView1.Location = new System.Drawing.Point(234, 281);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(307, 205);
            this.dataGridView1.TabIndex = 18;
            // 
            // KeyColumn1
            // 
            this.KeyColumn1.HeaderText = "Key";
            this.KeyColumn1.MinimumWidth = 6;
            this.KeyColumn1.Name = "KeyColumn1";
            this.KeyColumn1.Width = 125;
            // 
            // ValueColumn1
            // 
            this.ValueColumn1.HeaderText = "Value";
            this.ValueColumn1.MinimumWidth = 6;
            this.ValueColumn1.Name = "ValueColumn1";
            this.ValueColumn1.Width = 125;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(231, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 16);
            this.label1.TabIndex = 19;
            this.label1.Text = "Thông tin máy trạm";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(557, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(210, 16);
            this.label2.TabIndex = 20;
            this.label2.Text = "Tiến trình đang chạy trên máy trạm";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(231, 259);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(201, 16);
            this.label3.TabIndex = 21;
            this.label3.Text = "Thông tin sinh viên đang sử dụng";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(560, 259);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(111, 16);
            this.label4.TabIndex = 22;
            this.label4.Text = "Lịch sử hoạt động";
            // 
            // comboBoxMachines
            // 
            this.comboBoxMachines.FormattingEnabled = true;
            this.comboBoxMachines.Location = new System.Drawing.Point(12, 90);
            this.comboBoxMachines.Name = "comboBoxMachines";
            this.comboBoxMachines.Size = new System.Drawing.Size(98, 24);
            this.comboBoxMachines.TabIndex = 24;
            // 
            // MainForm
            // 
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(952, 496);
            this.Controls.Add(this.comboBoxMachines);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.infoDataGrid);
            this.Controls.Add(this.btnKillProcess);
            this.Controls.Add(this.txtHost);
            this.Controls.Add(this.txtRoom);
            this.Controls.Add(this.txtComputer);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.btnEnableFirewall);
            this.Controls.Add(this.btnDisableFirewall);
            this.Controls.Add(this.btnShutdown);
            this.Controls.Add(this.btnLightOn);
            this.Controls.Add(this.btnSaveSettings);
            this.Controls.Add(this.txtLog);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lvProcesses);
            this.Controls.Add(this.btnUpdateTopics);
            this.Controls.Add(this.lblCurrentRoom);
            this.Controls.Add(this.lblCurrentComputer);
            this.Controls.Add(this.txtRfid);
            this.MinimumSize = new System.Drawing.Size(800, 400);
            this.Name = "MainForm";
            this.Text = "Local Center";
            ((System.ComponentModel.ISupportInitialize)(this.infoDataGrid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private DataGridView infoDataGrid;
        private DataGridViewTextBoxColumn KeyColumn;
        private DataGridViewTextBoxColumn ValueColumn;
        private DataGridView dataGridView1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Label label4;
        private DataGridViewTextBoxColumn KeyColumn1;
        private DataGridViewTextBoxColumn ValueColumn1;
        private ComboBox comboBoxMachines;
    }
}