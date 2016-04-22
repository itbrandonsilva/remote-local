namespace RLocal
{
    partial class RLocal
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RLocal));
            this.BindAddressTextBox = new System.Windows.Forms.TextBox();
            this.BindAddressLabel = new System.Windows.Forms.Label();
            this.TabsControl = new System.Windows.Forms.TabControl();
            this.ClientTab = new System.Windows.Forms.TabPage();
            this.HostAddressLabel = new System.Windows.Forms.Label();
            this.HostAddressTextBox = new System.Windows.Forms.TextBox();
            this.ClientButton = new System.Windows.Forms.Button();
            this.ServerTab = new System.Windows.Forms.TabPage();
            this.OutHeightTextBox = new System.Windows.Forms.TextBox();
            this.OutWidthTextBox = new System.Windows.Forms.TextBox();
            this.FramerateTextBox = new System.Windows.Forms.TextBox();
            this.OutHeightLabel = new System.Windows.Forms.Label();
            this.OutWidthLabel = new System.Windows.Forms.Label();
            this.FramerateLabe = new System.Windows.Forms.Label();
            this.SoundCheckBox = new System.Windows.Forms.CheckBox();
            this.SoundLabel = new System.Windows.Forms.Label();
            this.MonitorComboBox = new System.Windows.Forms.ComboBox();
            this.MonitorLabel = new System.Windows.Forms.Label();
            this.ServerButton = new System.Windows.Forms.Button();
            this.OptionsTab = new System.Windows.Forms.TabPage();
            this.PortTextBox = new System.Windows.Forms.TextBox();
            this.PortLabel = new System.Windows.Forms.Label();
            this.InputSourceComboBox = new System.Windows.Forms.ComboBox();
            this.InputSourceLabel = new System.Windows.Forms.Label();
            this.TabsControl.SuspendLayout();
            this.ClientTab.SuspendLayout();
            this.ServerTab.SuspendLayout();
            this.OptionsTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindAddressTextBox
            // 
            this.BindAddressTextBox.Location = new System.Drawing.Point(95, 17);
            this.BindAddressTextBox.Name = "BindAddressTextBox";
            this.BindAddressTextBox.Size = new System.Drawing.Size(76, 20);
            this.BindAddressTextBox.TabIndex = 3;
            this.BindAddressTextBox.Text = "192.168.1.5";
            // 
            // BindAddressLabel
            // 
            this.BindAddressLabel.AutoSize = true;
            this.BindAddressLabel.Location = new System.Drawing.Point(20, 20);
            this.BindAddressLabel.Name = "BindAddressLabel";
            this.BindAddressLabel.Size = new System.Drawing.Size(69, 13);
            this.BindAddressLabel.TabIndex = 4;
            this.BindAddressLabel.Text = "Bind Address";
            // 
            // TabsControl
            // 
            this.TabsControl.Controls.Add(this.ClientTab);
            this.TabsControl.Controls.Add(this.ServerTab);
            this.TabsControl.Controls.Add(this.OptionsTab);
            this.TabsControl.Location = new System.Drawing.Point(12, 12);
            this.TabsControl.Name = "TabsControl";
            this.TabsControl.SelectedIndex = 0;
            this.TabsControl.Size = new System.Drawing.Size(283, 339);
            this.TabsControl.TabIndex = 5;
            // 
            // ClientTab
            // 
            this.ClientTab.Controls.Add(this.InputSourceLabel);
            this.ClientTab.Controls.Add(this.InputSourceComboBox);
            this.ClientTab.Controls.Add(this.HostAddressLabel);
            this.ClientTab.Controls.Add(this.HostAddressTextBox);
            this.ClientTab.Controls.Add(this.ClientButton);
            this.ClientTab.Location = new System.Drawing.Point(4, 22);
            this.ClientTab.Name = "ClientTab";
            this.ClientTab.Padding = new System.Windows.Forms.Padding(3);
            this.ClientTab.Size = new System.Drawing.Size(275, 313);
            this.ClientTab.TabIndex = 1;
            this.ClientTab.Text = "Client";
            this.ClientTab.UseVisualStyleBackColor = true;
            // 
            // HostAddressLabel
            // 
            this.HostAddressLabel.AutoSize = true;
            this.HostAddressLabel.Location = new System.Drawing.Point(20, 20);
            this.HostAddressLabel.Name = "HostAddressLabel";
            this.HostAddressLabel.Size = new System.Drawing.Size(64, 13);
            this.HostAddressLabel.TabIndex = 6;
            this.HostAddressLabel.Text = "Host Adress";
            // 
            // HostAddressTextBox
            // 
            this.HostAddressTextBox.Location = new System.Drawing.Point(95, 17);
            this.HostAddressTextBox.Name = "HostAddressTextBox";
            this.HostAddressTextBox.Size = new System.Drawing.Size(100, 20);
            this.HostAddressTextBox.TabIndex = 5;
            this.HostAddressTextBox.Text = "192.168.1.5";
            // 
            // ClientButton
            // 
            this.ClientButton.Location = new System.Drawing.Point(194, 284);
            this.ClientButton.Name = "ClientButton";
            this.ClientButton.Size = new System.Drawing.Size(75, 23);
            this.ClientButton.TabIndex = 3;
            this.ClientButton.Text = "Connect";
            this.ClientButton.UseVisualStyleBackColor = true;
            this.ClientButton.Click += new System.EventHandler(this.ClientButton_Click);
            // 
            // ServerTab
            // 
            this.ServerTab.Controls.Add(this.OutHeightTextBox);
            this.ServerTab.Controls.Add(this.OutWidthTextBox);
            this.ServerTab.Controls.Add(this.FramerateTextBox);
            this.ServerTab.Controls.Add(this.OutHeightLabel);
            this.ServerTab.Controls.Add(this.OutWidthLabel);
            this.ServerTab.Controls.Add(this.FramerateLabe);
            this.ServerTab.Controls.Add(this.SoundCheckBox);
            this.ServerTab.Controls.Add(this.SoundLabel);
            this.ServerTab.Controls.Add(this.MonitorComboBox);
            this.ServerTab.Controls.Add(this.MonitorLabel);
            this.ServerTab.Controls.Add(this.ServerButton);
            this.ServerTab.Controls.Add(this.BindAddressLabel);
            this.ServerTab.Controls.Add(this.BindAddressTextBox);
            this.ServerTab.Location = new System.Drawing.Point(4, 22);
            this.ServerTab.Name = "ServerTab";
            this.ServerTab.Padding = new System.Windows.Forms.Padding(3);
            this.ServerTab.Size = new System.Drawing.Size(275, 313);
            this.ServerTab.TabIndex = 0;
            this.ServerTab.Text = "Server";
            this.ServerTab.UseVisualStyleBackColor = true;
            // 
            // OutHeightTextBox
            // 
            this.OutHeightTextBox.Location = new System.Drawing.Point(95, 177);
            this.OutHeightTextBox.Name = "OutHeightTextBox";
            this.OutHeightTextBox.Size = new System.Drawing.Size(100, 20);
            this.OutHeightTextBox.TabIndex = 13;
            this.OutHeightTextBox.Text = "900";
            // 
            // OutWidthTextBox
            // 
            this.OutWidthTextBox.Location = new System.Drawing.Point(95, 145);
            this.OutWidthTextBox.Name = "OutWidthTextBox";
            this.OutWidthTextBox.Size = new System.Drawing.Size(100, 20);
            this.OutWidthTextBox.TabIndex = 12;
            this.OutWidthTextBox.Text = "1600";
            // 
            // FramerateTextBox
            // 
            this.FramerateTextBox.Location = new System.Drawing.Point(95, 113);
            this.FramerateTextBox.Name = "FramerateTextBox";
            this.FramerateTextBox.Size = new System.Drawing.Size(100, 20);
            this.FramerateTextBox.TabIndex = 11;
            this.FramerateTextBox.Text = "60";
            // 
            // OutHeightLabel
            // 
            this.OutHeightLabel.AutoSize = true;
            this.OutHeightLabel.Location = new System.Drawing.Point(20, 180);
            this.OutHeightLabel.Name = "OutHeightLabel";
            this.OutHeightLabel.Size = new System.Drawing.Size(58, 13);
            this.OutHeightLabel.TabIndex = 10;
            this.OutHeightLabel.Text = "Out Height";
            // 
            // OutWidthLabel
            // 
            this.OutWidthLabel.AutoSize = true;
            this.OutWidthLabel.Location = new System.Drawing.Point(20, 148);
            this.OutWidthLabel.Name = "OutWidthLabel";
            this.OutWidthLabel.Size = new System.Drawing.Size(55, 13);
            this.OutWidthLabel.TabIndex = 9;
            this.OutWidthLabel.Text = "Out Width";
            // 
            // FramerateLabe
            // 
            this.FramerateLabe.AutoSize = true;
            this.FramerateLabe.Location = new System.Drawing.Point(20, 116);
            this.FramerateLabe.Name = "FramerateLabe";
            this.FramerateLabe.Size = new System.Drawing.Size(54, 13);
            this.FramerateLabe.TabIndex = 8;
            this.FramerateLabe.Text = "Framerate";
            // 
            // SoundCheckBox
            // 
            this.SoundCheckBox.AutoSize = true;
            this.SoundCheckBox.Checked = true;
            this.SoundCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.SoundCheckBox.Location = new System.Drawing.Point(95, 84);
            this.SoundCheckBox.Name = "SoundCheckBox";
            this.SoundCheckBox.Size = new System.Drawing.Size(15, 14);
            this.SoundCheckBox.TabIndex = 7;
            this.SoundCheckBox.UseVisualStyleBackColor = true;
            // 
            // SoundLabel
            // 
            this.SoundLabel.AutoSize = true;
            this.SoundLabel.Location = new System.Drawing.Point(20, 84);
            this.SoundLabel.Name = "SoundLabel";
            this.SoundLabel.Size = new System.Drawing.Size(38, 13);
            this.SoundLabel.TabIndex = 6;
            this.SoundLabel.Text = "Sound";
            // 
            // MonitorComboBox
            // 
            this.MonitorComboBox.FormattingEnabled = true;
            this.MonitorComboBox.Location = new System.Drawing.Point(95, 49);
            this.MonitorComboBox.Name = "MonitorComboBox";
            this.MonitorComboBox.Size = new System.Drawing.Size(76, 21);
            this.MonitorComboBox.TabIndex = 2;
            // 
            // MonitorLabel
            // 
            this.MonitorLabel.AutoSize = true;
            this.MonitorLabel.Location = new System.Drawing.Point(20, 52);
            this.MonitorLabel.Name = "MonitorLabel";
            this.MonitorLabel.Size = new System.Drawing.Size(42, 13);
            this.MonitorLabel.TabIndex = 3;
            this.MonitorLabel.Text = "Monitor";
            // 
            // ServerButton
            // 
            this.ServerButton.Location = new System.Drawing.Point(194, 284);
            this.ServerButton.Name = "ServerButton";
            this.ServerButton.Size = new System.Drawing.Size(75, 23);
            this.ServerButton.TabIndex = 5;
            this.ServerButton.Text = "Host";
            this.ServerButton.UseVisualStyleBackColor = true;
            this.ServerButton.Click += new System.EventHandler(this.ServerButton_Click);
            // 
            // OptionsTab
            // 
            this.OptionsTab.Controls.Add(this.PortTextBox);
            this.OptionsTab.Controls.Add(this.PortLabel);
            this.OptionsTab.Location = new System.Drawing.Point(4, 22);
            this.OptionsTab.Name = "OptionsTab";
            this.OptionsTab.Size = new System.Drawing.Size(275, 313);
            this.OptionsTab.TabIndex = 2;
            this.OptionsTab.Text = "Options";
            this.OptionsTab.UseVisualStyleBackColor = true;
            // 
            // PortTextBox
            // 
            this.PortTextBox.Location = new System.Drawing.Point(95, 17);
            this.PortTextBox.Name = "PortTextBox";
            this.PortTextBox.Size = new System.Drawing.Size(46, 20);
            this.PortTextBox.TabIndex = 1;
            this.PortTextBox.Text = "47812";
            // 
            // PortLabel
            // 
            this.PortLabel.AutoSize = true;
            this.PortLabel.Location = new System.Drawing.Point(20, 20);
            this.PortLabel.Name = "PortLabel";
            this.PortLabel.Size = new System.Drawing.Size(26, 13);
            this.PortLabel.TabIndex = 0;
            this.PortLabel.Text = "Port";
            // 
            // InputSourceComboBox
            // 
            this.InputSourceComboBox.FormattingEnabled = true;
            this.InputSourceComboBox.Location = new System.Drawing.Point(95, 49);
            this.InputSourceComboBox.Name = "InputSourceComboBox";
            this.InputSourceComboBox.Size = new System.Drawing.Size(144, 21);
            this.InputSourceComboBox.TabIndex = 7;
            // 
            // InputSourceLabel
            // 
            this.InputSourceLabel.AutoSize = true;
            this.InputSourceLabel.Location = new System.Drawing.Point(20, 52);
            this.InputSourceLabel.Name = "InputSourceLabel";
            this.InputSourceLabel.Size = new System.Drawing.Size(68, 13);
            this.InputSourceLabel.TabIndex = 8;
            this.InputSourceLabel.Text = "Input Source";
            // 
            // RLocal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(307, 363);
            this.Controls.Add(this.TabsControl);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RLocal";
            this.Text = "RLocal";
            this.TabsControl.ResumeLayout(false);
            this.ClientTab.ResumeLayout(false);
            this.ClientTab.PerformLayout();
            this.ServerTab.ResumeLayout(false);
            this.ServerTab.PerformLayout();
            this.OptionsTab.ResumeLayout(false);
            this.OptionsTab.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TextBox BindAddressTextBox;
        private System.Windows.Forms.Label BindAddressLabel;
        private System.Windows.Forms.TabControl TabsControl;
        private System.Windows.Forms.TabPage ServerTab;
        private System.Windows.Forms.TabPage ClientTab;
        private System.Windows.Forms.TabPage OptionsTab;
        private System.Windows.Forms.Label PortLabel;
        private System.Windows.Forms.Button ClientButton;
        private System.Windows.Forms.Button ServerButton;
        private System.Windows.Forms.TextBox PortTextBox;
        private System.Windows.Forms.Label HostAddressLabel;
        private System.Windows.Forms.TextBox HostAddressTextBox;
        private System.Windows.Forms.Label MonitorLabel;
        private System.Windows.Forms.ComboBox MonitorComboBox;
        private System.Windows.Forms.CheckBox SoundCheckBox;
        private System.Windows.Forms.Label SoundLabel;
        private System.Windows.Forms.Label OutHeightLabel;
        private System.Windows.Forms.Label OutWidthLabel;
        private System.Windows.Forms.Label FramerateLabe;
        private System.Windows.Forms.TextBox OutHeightTextBox;
        private System.Windows.Forms.TextBox OutWidthTextBox;
        private System.Windows.Forms.TextBox FramerateTextBox;
        private System.Windows.Forms.Label InputSourceLabel;
        private System.Windows.Forms.ComboBox InputSourceComboBox;
    }
}