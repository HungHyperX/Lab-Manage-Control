
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
            this.lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();

            // txtHost
            this.txtHost.Location = new System.Drawing.Point(12, 12);
            this.txtHost.Size = new System.Drawing.Size(200, 20);

            // txtRoom
            this.txtRoom.Location = new System.Drawing.Point(12, 38);
            this.txtRoom.Size = new System.Drawing.Size(200, 20);

            // txtComputer
            this.txtComputer.Location = new System.Drawing.Point(12, 64);
            this.txtComputer.Size = new System.Drawing.Size(200, 20);

            // btnConnect
            this.btnConnect.Location = new System.Drawing.Point(12, 90);
            this.btnConnect.Size = new System.Drawing.Size(200, 23);
            this.btnConnect.Text = "Connect MQTT";
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);

            // btnEnableFirewall
            this.btnEnableFirewall.Location = new System.Drawing.Point(12, 119);
            this.btnEnableFirewall.Size = new System.Drawing.Size(95, 23);
            this.btnEnableFirewall.Text = "Enable FW";
            this.btnEnableFirewall.Click += new System.EventHandler(this.btnEnableFirewall_Click);

            // btnDisableFirewall
            this.btnDisableFirewall.Location = new System.Drawing.Point(117, 119);
            this.btnDisableFirewall.Size = new System.Drawing.Size(95, 23);
            this.btnDisableFirewall.Text = "Disable FW";
            this.btnDisableFirewall.Click += new System.EventHandler(this.btnDisableFirewall_Click);

            // btnShutdown
            this.btnShutdown.Location = new System.Drawing.Point(12, 148);
            this.btnShutdown.Size = new System.Drawing.Size(200, 23);
            this.btnShutdown.Text = "Shutdown";
            this.btnShutdown.Click += new System.EventHandler(this.btnShutdown_Click);

            // btnLightOn
            this.btnLightOn.Location = new System.Drawing.Point(12, 177);
            this.btnLightOn.Size = new System.Drawing.Size(200, 23);
            this.btnLightOn.Text = "Light On";
            this.btnLightOn.Click += new System.EventHandler(this.btnLightOn_Click);

            // btnSaveSettings
            this.btnSaveSettings.Location = new System.Drawing.Point(12, 206);
            this.btnSaveSettings.Size = new System.Drawing.Size(200, 23);
            this.btnSaveSettings.Text = "Save Settings";
            this.btnSaveSettings.Click += new System.EventHandler(this.SaveSettings_Click);

            // txtLog
            this.txtLog.Location = new System.Drawing.Point(230, 12);
            this.txtLog.Multiline = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtLog.Size = new System.Drawing.Size(400, 217);

            // lblStatus
            this.lblStatus.Location = new System.Drawing.Point(12, 235);
            this.lblStatus.Size = new System.Drawing.Size(200, 23);
            this.lblStatus.Text = "Disconnected";

            // MainForm
            this.ClientSize = new System.Drawing.Size(650, 270);
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
            this.Text = "Local Center";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
