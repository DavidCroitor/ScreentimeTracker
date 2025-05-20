namespace ScreentimeTracker.UI;
partial class SettingsForm
{
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Label lblIdleThreshold;
    private System.Windows.Forms.ComboBox cmbIdleThreshold;
    private System.Windows.Forms.Button btnSave;
    private System.Windows.Forms.Button btnCancel;
    private System.Windows.Forms.CheckBox chkBreakReminder;
    private System.Windows.Forms.Label lblBreakInterval;
    private System.Windows.Forms.ComboBox cmbBreakInterval;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) { components.Dispose(); }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {

        this.lblIdleThreshold = new System.Windows.Forms.Label();
        this.cmbIdleThreshold = new System.Windows.Forms.ComboBox();
        this.btnSave = new System.Windows.Forms.Button();
        this.btnCancel = new System.Windows.Forms.Button();
        this.SuspendLayout();

        // lblIdleThreshold
        this.lblIdleThreshold.AutoSize = true;
        this.lblIdleThreshold.Location = new System.Drawing.Point(20, 23);
        this.lblIdleThreshold.Name = "lblIdleThreshold";
        this.lblIdleThreshold.Size = new System.Drawing.Size(110, 15);
        this.lblIdleThreshold.Text = "Idle Threshold:";

        // cmbIdleThreshold
        this.cmbIdleThreshold.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbIdleThreshold.FormattingEnabled = true;
        this.cmbIdleThreshold.Location = new System.Drawing.Point(140, 20);
        this.cmbIdleThreshold.Name = "cmbIdleThreshold";
        this.cmbIdleThreshold.Size = new System.Drawing.Size(120, 23);

        //chkBreakReminder
        this.chkBreakReminder = new System.Windows.Forms.CheckBox();
        this.lblBreakInterval = new System.Windows.Forms.Label();
        this.cmbBreakInterval = new System.Windows.Forms.ComboBox();

        // chkBreakReminder
        this.chkBreakReminder.AutoSize = true;
        this.chkBreakReminder.Location = new System.Drawing.Point(23, 55);
        this.chkBreakReminder.Name = "chkBreakReminder";
        this.chkBreakReminder.Size = new System.Drawing.Size(140, 19);
        this.chkBreakReminder.Text = "Enable Break Reminder";
        this.chkBreakReminder.UseVisualStyleBackColor = true;
        this.chkBreakReminder.CheckedChanged += new System.EventHandler(this.chkBreakReminder_CheckedChanged); // Event handler

        // lblBreakInterval
        this.lblBreakInterval.AutoSize = true;
        this.lblBreakInterval.Location = new System.Drawing.Point(40, 85);
        this.lblBreakInterval.Name = "lblBreakInterval";
        this.lblBreakInterval.Size = new System.Drawing.Size(90, 15);
        this.lblBreakInterval.Text = "Remind every:";

        // cmbBreakInterval
        this.cmbBreakInterval.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cmbBreakInterval.FormattingEnabled = true;
        this.cmbBreakInterval.Location = new System.Drawing.Point(140, 82);
        this.cmbBreakInterval.Name = "cmbBreakInterval";
        this.cmbBreakInterval.Size = new System.Drawing.Size(120, 23);

        // btnSave
        this.btnSave.Location = new System.Drawing.Point(70, 120);
        this.btnSave.Name = "btnSave";
        this.btnSave.Size = new System.Drawing.Size(75, 25);
        this.btnSave.Text = "Save";
        this.btnSave.UseVisualStyleBackColor = true;
        this.btnSave.Click += new System.EventHandler(this.btnSave_Click);

        // btnCancel
        this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.btnCancel.Location = new System.Drawing.Point(160, 120);
        this.btnCancel.Name = "btnCancel";
        this.btnCancel.Size = new System.Drawing.Size(75, 25);
        this.btnCancel.Text = "Cancel";
        this.btnCancel.UseVisualStyleBackColor = true;
        this.btnCancel.Click += (s, e) => this.Close(); // Simple close action

        // SettingsForm
        this.AcceptButton = this.btnSave; // Save on Enter
        this.CancelButton = this.btnCancel; // Close on Esc
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(300, 170); 
        this.Controls.Add(this.btnCancel);
        this.Controls.Add(this.btnSave);
        this.Controls.Add(this.cmbIdleThreshold);
        this.Controls.Add(this.lblIdleThreshold);
        this.Controls.Add(this.chkBreakReminder);
        this.Controls.Add(this.lblBreakInterval);
        this.Controls.Add(this.cmbBreakInterval);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "SettingsForm";
        this.Text = "Settings";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.ResumeLayout(false);
        this.PerformLayout();
    }
}