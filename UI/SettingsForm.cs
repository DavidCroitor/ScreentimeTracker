using ScreentimeTracker.Utils; 
using System;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;

namespace ScreentimeTracker.UI;

    public partial class SettingsForm : Form
    {
        private AppSettings _currentSettings;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

        public bool SettingsChanged { get; private set; } = false;

        public SettingsForm(AppSettings currentSettings)
        {
            InitializeComponent();
            _currentSettings = currentSettings; 
            PopulateIdleThresholdComboBox();
            PopulateBreakIntervalComboBox();
            LoadSettings();
        }
        private void PopulateBreakIntervalComboBox()
        {
            int[] intervals = { 15, 30, 45, 60, 75, 90, 120 };
            foreach (int interval in intervals)
            {
                cmbBreakInterval.Items.Add(new ComboBoxItem { Text = $"{interval} minutes", Value = interval });
            }
            cmbBreakInterval.DisplayMember = "Text";
            cmbBreakInterval.ValueMember = "Value";
        }
        private void PopulateIdleThresholdComboBox()
        {
            for (int i = 1; i <= 10; i++)
            {
                cmbIdleThreshold.Items.Add(new ComboBoxItem { Text = $"{i} minute{(i > 1 ? "s" : "")}", Value = i });
            }
            cmbIdleThreshold.DisplayMember = "Text";
            cmbIdleThreshold.ValueMember = "Value";
        }

        private void LoadSettings()
    {
        var idleItemToSelect = cmbIdleThreshold.Items
            .OfType<ComboBoxItem>()
            .FirstOrDefault(item => item.Value == _currentSettings.IdleThreshold);

        if (idleItemToSelect != null)
        {
            cmbIdleThreshold.SelectedItem = idleItemToSelect;
        }
        else if (cmbIdleThreshold.Items.Count > 0)
        {
            // Fallback if current value is not in list (e.g. changed manually in json)
            // or select a default, e.g. 2 minutes
            var defaultItem = cmbIdleThreshold.Items.OfType<ComboBoxItem>().FirstOrDefault(i => i.Value == 2)
                              ?? cmbIdleThreshold.Items[0];
            cmbIdleThreshold.SelectedItem = defaultItem;
            _currentSettings.IdleThreshold = (defaultItem as ComboBoxItem)?.Value ?? 2; // Update settings if changed
        }
            
            // Load Break Reminder Settings
            chkBreakReminder.Checked = _currentSettings.IsBreakReminderEnabled;

            var breakItemToSelect = cmbBreakInterval.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(item => item.Value == _currentSettings.BreakReminderIntervalMinutes);
            if (breakItemToSelect != null)
            {
                cmbBreakInterval.SelectedItem = breakItemToSelect;
            }
            else if (cmbBreakInterval.Items.Count > 0)
            {
                var defaultBreakItem = cmbBreakInterval.Items.OfType<ComboBoxItem>().FirstOrDefault(i => i.Value == 45)
                                    ?? cmbBreakInterval.Items[0];
                cmbBreakInterval.SelectedItem = defaultBreakItem;
                _currentSettings.BreakReminderIntervalMinutes = (defaultBreakItem as ComboBoxItem)?.Value ?? 45;
            }
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            bool changed = false;

            // Save Idle Threshold (existing)
            if (cmbIdleThreshold.SelectedItem is ComboBoxItem selectedIdleItem)
            {
                int newIdleThreshold = selectedIdleItem.Value;
                if (_currentSettings.IdleThreshold != newIdleThreshold)
                {
                    _currentSettings.IdleThreshold = newIdleThreshold;
                    changed = true;
                }
            }
            // Save Break Reminder Settings
            if (_currentSettings.IsBreakReminderEnabled != chkBreakReminder.Checked)
            {
                _currentSettings.IsBreakReminderEnabled = chkBreakReminder.Checked;
                changed = true;
            }
            if (cmbBreakInterval.SelectedItem is ComboBoxItem selectedBreakItem)
            {
                int newBreakInterval = selectedBreakItem.Value;
                if (_currentSettings.BreakReminderIntervalMinutes != newBreakInterval)
                {
                    _currentSettings.BreakReminderIntervalMinutes = newBreakInterval;
                    changed = true;
                }
            }

            if (changed)
            {
                _currentSettings.Save();
                SettingsChanged = true; 
                MessageBox.Show("Settings saved successfully!", "Settings Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void chkBreakReminder_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateBreakIntervalControlsState();
        }

        private void UpdateBreakIntervalControlsState()
        {
            lblBreakInterval.Enabled = chkBreakReminder.Checked;
            cmbBreakInterval.Enabled = chkBreakReminder.Checked;
        }

        private class ComboBoxItem
    {
        public string Text { get; set; } = string.Empty;
        public int Value { get; set; }
    }
    }
