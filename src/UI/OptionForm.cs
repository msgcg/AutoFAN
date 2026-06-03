using DarkUI.Forms;
using FanCtrl.Resources;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FanCtrl
{
    public partial class OptionForm : ThemeForm
    {
        public OptionForm()
        {
            InitializeComponent();
            this.localizeComponent();

            if (Screen.PrimaryScreen.WorkingArea.Height < this.Height)
            {
                this.AutoScroll = true;
                this.AutoSizeMode = AutoSizeMode.GrowOnly;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.Width = this.Width + 10;
                this.Height = this.Height - 100;

                this.MinimumSize = new Size(this.Width, 300);
                this.MaximumSize = new Size(this.Width, Int32.MaxValue);
            }            

            mToolTip.SetToolTip(mIntervalTextBox, "100 ≤ value ≤ 5000");
            mToolTip.SetToolTip(mStartupDelayTextBox, "0 ≤ value ≤ 59");            mIntervalTextBox.KeyPress += onTextBoxKeyPress;
            mLHMCheckBox.CheckedChanged += (object sender, EventArgs e) =>
            {
                mLHMCPUCheckBox.Enabled = mLHMCheckBox.Checked;
                mLHMMBCheckBox.Enabled = mLHMCheckBox.Checked;
                mLHMGPUCheckBox.Enabled = mLHMCheckBox.Checked;
                mLHMControllerCheckBox.Enabled = mLHMCheckBox.Checked;
                mLHMStorageCheckBox.Enabled = mLHMCheckBox.Checked;
                mLHMMemoryCheckBox.Enabled = mLHMCheckBox.Checked;
            };
            mLHMCheckBox.Checked = OptionManager.getInstance().IsLHM;
            mLHMCPUCheckBox.Checked = OptionManager.getInstance().IsLHMCpu;
            mLHMMBCheckBox.Checked = OptionManager.getInstance().IsLHMMotherboard;
            mLHMGPUCheckBox.Checked = OptionManager.getInstance().IsLHMGpu;
            mLHMControllerCheckBox.Checked = OptionManager.getInstance().IsLHMContolled;
            mLHMStorageCheckBox.Checked = OptionManager.getInstance().IsLHMStorage;
            mLHMMemoryCheckBox.Checked = OptionManager.getInstance().IsLHMMemory;
            if (mLHMCheckBox.Checked == false)
            {
                mLHMCPUCheckBox.Enabled = false;
                mLHMMBCheckBox.Enabled = false;
                mLHMGPUCheckBox.Enabled = false;
                mLHMControllerCheckBox.Enabled = false;
                mLHMStorageCheckBox.Enabled = false;
                mLHMMemoryCheckBox.Enabled = false;
            }

            mNvApiCheckBox.Checked = OptionManager.getInstance().IsNvAPIWrapper;



            mThemeComboBox.Items.Add(StringLib.Theme_System);
            mThemeComboBox.Items.Add(StringLib.Theme_Light);
            mThemeComboBox.Items.Add(StringLib.Theme_Dark);
            mThemeComboBox.SelectedIndex = (int)OptionManager.getInstance().Theme;

            mFahrenheitCheckBox.Checked = OptionManager.getInstance().IsFahrenheit;
            mAnimationCheckBox.Checked = OptionManager.getInstance().IsAnimation;
            mMinimizeCheckBox.Checked = OptionManager.getInstance().IsMinimized;
            mStartupCheckBox.Checked = OptionManager.getInstance().IsStartUp;
            mStartupDelayTextBox.Text = OptionManager.getInstance().DelayTime.ToString();
            mIntervalTextBox.Text = OptionManager.getInstance().Interval.ToString();
        }

        private void localizeComponent()
        {
            this.Text = StringLib.Option;
            mIntervalGroupBox.Text = StringLib.Interval;
            mAnimationCheckBox.Text = StringLib.Tray_Icon_animation;

            mThemeLabel.Text = StringLib.Theme;
            mFahrenheitCheckBox.Text = StringLib.Fahrenheit;
            mMinimizeCheckBox.Text = StringLib.Start_minimized;
            mStartupCheckBox.Text = StringLib.Start_with_Windows;
            mStartupDelayLabel.Text = StringLib.Delay_Time;
            mLibraryGroupBox.Text = StringLib.Library;
            mResetButton.Text = StringLib.Reset;
            mOKButton.Text = StringLib.OK;

            FontFamily fontFamily = null;
            try
            {
                fontFamily = new FontFamily("Gulim");
            }
            catch
            {
                fontFamily = FontFamily.GenericSansSerif;
            }                mStartupDelayLabel.Left = mStartupDelayLabel.Left - 10;
        }

        private void onOKButtonClick(object sender, EventArgs e)
        {
            int delayTime = 0;
            int.TryParse(mStartupDelayTextBox.Text, out delayTime);
            if (delayTime < 0)
            {
                delayTime = 0;
            }
            else if (delayTime > 59)
            {
                delayTime = 59;
            }

            int interval = 0;
            int.TryParse(mIntervalTextBox.Text, out interval);
            if (interval < 100)
            {
                interval = 100;
            }
            else if (interval > 5000)
            {
                interval = 5000;
            }

            var optionManager = OptionManager.getInstance();
            bool isRestart = false;

            if ((optionManager.IsLHM != mLHMCheckBox.Checked) ||
                (mLHMCheckBox.Checked == true && optionManager.IsLHMCpu != mLHMCPUCheckBox.Checked) ||
                (mLHMCheckBox.Checked == true && optionManager.IsLHMMotherboard != mLHMMBCheckBox.Checked) ||
                (mLHMCheckBox.Checked == true && optionManager.IsLHMGpu != mLHMGPUCheckBox.Checked) ||
                (mLHMCheckBox.Checked == true && optionManager.IsLHMContolled != mLHMControllerCheckBox.Checked) ||
                (mLHMCheckBox.Checked == true && optionManager.IsLHMStorage != mLHMStorageCheckBox.Checked) ||
                (mLHMCheckBox.Checked == true && optionManager.IsLHMMemory != mLHMMemoryCheckBox.Checked) ||

                (optionManager.IsNvAPIWrapper != mNvApiCheckBox.Checked) ||
                ((int)optionManager.Theme != mThemeComboBox.SelectedIndex))
            {
                var result = DarkMessageBox.ShowInformation(StringLib.OptionChange, StringLib.Option, DarkDialogButton.OkCancel);
                if (result == DialogResult.Cancel)
                    return;

                isRestart = true;
            }
            optionManager.IsLHM = mLHMCheckBox.Checked;
            optionManager.Interval = interval;
            optionManager.IsLHMCpu = mLHMCPUCheckBox.Checked;
            optionManager.IsLHMMotherboard = mLHMMBCheckBox.Checked;
            optionManager.IsLHMGpu = mLHMGPUCheckBox.Checked;
            optionManager.IsLHMContolled = mLHMControllerCheckBox.Checked;
            optionManager.IsLHMStorage = mLHMStorageCheckBox.Checked;
            optionManager.IsLHMMemory = mLHMMemoryCheckBox.Checked;

            optionManager.IsNvAPIWrapper = mNvApiCheckBox.Checked;

            optionManager.Theme = (THEME_TYPE)mThemeComboBox.SelectedIndex;
            optionManager.IsFahrenheit = mFahrenheitCheckBox.Checked;
            optionManager.IsAnimation = mAnimationCheckBox.Checked;
            optionManager.IsMinimized = mMinimizeCheckBox.Checked;
            optionManager.IsStartUp = mStartupCheckBox.Checked;            
            optionManager.write();

            if (isRestart == true)
            {
                this.DialogResult = DialogResult.Yes;
            }
            else
            {
                this.DialogResult = DialogResult.OK;
            }

            Util.setLanguage();

            this.Close();
        }

        private void onResetButtonClick(object sender, EventArgs e)
        {
            var result = DarkMessageBox.ShowInformation(StringLib.OptionReset, StringLib.Option, DarkDialogButton.OkCancel);
            if (result == DialogResult.Cancel)
                return;

            OptionManager.getInstance().reset();            
            OptionManager.getInstance().write();
            this.DialogResult = DialogResult.No;
            this.Close();
        }        

        private void onTextBoxKeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsDigit(e.KeyChar) == false)
            {
                e.Handled = true;
            }
        }
}


}





