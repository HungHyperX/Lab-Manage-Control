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
        private System.Windows.Forms.TextBox txtMqttMessages;
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
            this.txtMqttMessages = new System.Windows.Forms.TextBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lvProcesses = new System.Windows.Forms.ListView();
            this.btnUpdateTopics = new System.Windows.Forms.Button();
            this.lblCurrentRoom = new System.Windows.Forms.Label();
            this.lblCurrentComputer = new System.Windows.Forms.Label();
            this.txtRfid = new System.Windows.Forms.TextBox(); // Khởi tạo TextBox mới
            this.btnKillProcess = new System.Windows.Forms.Button();

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
            this.btnConnect.Location = new System.Drawing.Point(12, 90);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(200, 23);
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
            this.txtLog.Location = new System.Drawing.Point(230, 92);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(400, 137);
            this.txtLog.TabIndex = 9;
            // 
            // txtMqttMessages
            // 
            this.txtMqttMessages.Location = new System.Drawing.Point(230, 12);
            this.txtMqttMessages.Multiline = true;
            this.txtMqttMessages.Name = "txtMqttMessages";
            this.txtMqttMessages.ReadOnly = true;
            this.txtMqttMessages.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMqttMessages.Size = new System.Drawing.Size(400, 74);
            this.txtMqttMessages.TabIndex = 10;
            // 
            // lblStatus
            // 
            this.lblStatus.Location = new System.Drawing.Point(12, 261);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(200, 23);
            this.lblStatus.TabIndex = 11;
            this.lblStatus.Text = "Disconnected";
            // 
            // lvProcesses
            // 
            this.lvProcesses.FullRowSelect = true;
            this.lvProcesses.GridLines = true;
            this.lvProcesses.HideSelection = false;
            this.lvProcesses.Location = new System.Drawing.Point(640, 12);
            this.lvProcesses.Name = "lvProcesses";
            this.lvProcesses.Size = new System.Drawing.Size(300, 218);
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
            this.lblCurrentRoom.Location = new System.Drawing.Point(230, 235);
            this.lblCurrentRoom.Name = "lblCurrentRoom";
            this.lblCurrentRoom.Size = new System.Drawing.Size(200, 23);
            this.lblCurrentRoom.TabIndex = 14;
            this.lblCurrentRoom.Text = "Current Room: None";
            // 
            // lblCurrentComputer
            // 
            this.lblCurrentComputer.Location = new System.Drawing.Point(230, 261);
            this.lblCurrentComputer.Name = "lblCurrentComputer";
            this.lblCurrentComputer.Size = new System.Drawing.Size(200, 23);
            this.lblCurrentComputer.TabIndex = 15;
            this.lblCurrentComputer.Text = "Current Computer: None";
            // 
            // txtRfid
            // 
            this.txtRfid.Location = new System.Drawing.Point(640, 240);
            this.txtRfid.Name = "txtRfid";
            this.txtRfid.ReadOnly = true;
            this.txtRfid.Size = new System.Drawing.Size(300, 22);
            this.txtRfid.TabIndex = 16;
            this.txtRfid.Text = "RFID Tag ID here";
            // 
            // btnKillProcess
            // 
            //this.btnKillProcess = new System.Windows.Forms.Button();
            this.btnKillProcess.Text = "Kill Process";
            this.btnKillProcess.Location = new System.Drawing.Point(113, 206); // vị trí tuỳ chỉnh
            this.btnKillProcess.Size = new System.Drawing.Size(95, 23);
            this.btnKillProcess.Click += new System.EventHandler(this.btnKillProcess_Click);
            this.Controls.Add(this.btnKillProcess);

            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(950, 290);
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
            this.Controls.Add(this.txtMqttMessages);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.lvProcesses);
            this.Controls.Add(this.btnUpdateTopics);
            this.Controls.Add(this.lblCurrentRoom);
            this.Controls.Add(this.lblCurrentComputer);
            this.Controls.Add(this.txtRfid); // Thêm vào giao diện
            this.Name = "MainForm";
            this.Text = "Local Center";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}