namespace RLocal
{
    partial class RLocalKeyboardConfig
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RLocalKeyboardConfig));
            this.ButtonComboBox = new System.Windows.Forms.ComboBox();
            this.ButtonLabel = new System.Windows.Forms.Label();
            this.KeyLabel = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ButtonComboBox
            // 
            this.ButtonComboBox.FormattingEnabled = true;
            this.ButtonComboBox.Location = new System.Drawing.Point(79, 22);
            this.ButtonComboBox.Name = "ButtonComboBox";
            this.ButtonComboBox.Size = new System.Drawing.Size(116, 21);
            this.ButtonComboBox.TabIndex = 0;
            // 
            // ButtonLabel
            // 
            this.ButtonLabel.AutoSize = true;
            this.ButtonLabel.Location = new System.Drawing.Point(25, 25);
            this.ButtonLabel.Name = "ButtonLabel";
            this.ButtonLabel.Size = new System.Drawing.Size(38, 13);
            this.ButtonLabel.TabIndex = 1;
            this.ButtonLabel.Text = "Button";
            // 
            // KeyLabel
            // 
            this.KeyLabel.AutoSize = true;
            this.KeyLabel.Location = new System.Drawing.Point(212, 25);
            this.KeyLabel.Name = "KeyLabel";
            this.KeyLabel.Size = new System.Drawing.Size(27, 13);
            this.KeyLabel.TabIndex = 2;
            this.KeyLabel.Text = "N/A";
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.SystemColors.Control;
            this.button1.Location = new System.Drawing.Point(79, 49);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Record";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // RLocalKeyboardConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(329, 158);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.KeyLabel);
            this.Controls.Add(this.ButtonLabel);
            this.Controls.Add(this.ButtonComboBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "RLocalKeyboardConfig";
            this.Text = "Keyboard Config";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox ButtonComboBox;
        private System.Windows.Forms.Label ButtonLabel;
        private System.Windows.Forms.Label KeyLabel;
        private System.Windows.Forms.Button button1;
    }
}