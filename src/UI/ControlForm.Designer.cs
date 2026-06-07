using DarkUI.Controls;
using System.Windows.Forms;

namespace FanCtrl
{
    partial class ControlForm
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ControlForm));
            mEnableCheckBox = new DarkCheckBox();
            mTempGroupBox = new DarkGroupBox();
            mAddTempListView = new ThemeListView();
            mFanGroupBox = new DarkGroupBox();
            mFanListView = new ThemeListView();
            mAddFanListView = new ThemeListView();
            mRemoveButton = new DarkButton();
            mAddButton = new DarkButton();
            mGraphGroupBox = new DarkGroupBox();
            mPresetLabel = new DarkLabel();
            mUnitLabel = new DarkLabel();
            mHysLabel = new DarkLabel();
            mStepCheckBox = new DarkCheckBox();
            mGraph = new ZedGraph.ZedGraphControl();
            mOKButton = new DarkButton();
            mApplyButton = new DarkButton();
            mHysNumericUpDown = new DarkNumericUpDown();
            mModeGroupBox = new DarkGroupBox();
            mGameRadioButton = new DarkRadioButton();
            mPerformanceRadioButton = new DarkRadioButton();
            mSilenceRadioButton = new DarkRadioButton();
            mNormalRadioButton = new DarkRadioButton();
            mImportIntelligentButton = new DarkButton();
            mUnitComboBox = new DarkComboBox();
            mPresetLoadButton = new DarkButton();
            mPresetSaveButton = new DarkButton();
            mAutoNumericUpDown = new DarkNumericUpDown();
            mAutoLabel = new DarkLabel();
            mDelayLabel = new DarkLabel();
            mDelayNumericUpDown = new DarkNumericUpDown();
            mDelayLabel2 = new DarkLabel();
            mTempGroupBox.SuspendLayout();
            mFanGroupBox.SuspendLayout();
            mGraphGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mHysNumericUpDown).BeginInit();
            mModeGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)mAutoNumericUpDown).BeginInit();
            ((System.ComponentModel.ISupportInitialize)mDelayNumericUpDown).BeginInit();
            SuspendLayout();
            // 
            // mEnableCheckBox
            // 
            mEnableCheckBox.AutoSize = true;
            mEnableCheckBox.Location = new System.Drawing.Point(19, 27);
            mEnableCheckBox.Name = "mEnableCheckBox";
            mEnableCheckBox.Size = new System.Drawing.Size(179, 19);
            mEnableCheckBox.TabIndex = 0;
            mEnableCheckBox.Text = "Enable automatic fan control";
            // 
            // mTempGroupBox
            // 
            mTempGroupBox.BorderColor = System.Drawing.Color.FromArgb(51, 51, 51);
            mTempGroupBox.Controls.Add(mAddTempListView);
            mTempGroupBox.Location = new System.Drawing.Point(12, 61);
            mTempGroupBox.Name = "mTempGroupBox";
            mTempGroupBox.Size = new System.Drawing.Size(305, 211);
            mTempGroupBox.TabIndex = 1;
            mTempGroupBox.TabStop = false;
            mTempGroupBox.Text = "Temperature Sensor";
            // 
            // mAddTempListView
            // 
            mAddTempListView.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            mAddTempListView.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            mAddTempListView.FullRowSelect = true;
            mAddTempListView.HeaderStyle = ColumnHeaderStyle.None;
            mAddTempListView.Location = new System.Drawing.Point(6, 20);
            mAddTempListView.MultiSelect = false;
            mAddTempListView.Name = "mAddTempListView";
            mAddTempListView.Size = new System.Drawing.Size(293, 185);
            mAddTempListView.TabIndex = 5;
            mAddTempListView.UseCompatibleStateImageBehavior = false;
            mAddTempListView.View = View.Details;
            // 
            // mFanGroupBox
            // 
            mFanGroupBox.BorderColor = System.Drawing.Color.FromArgb(51, 51, 51);
            mFanGroupBox.Controls.Add(mFanListView);
            mFanGroupBox.Controls.Add(mAddFanListView);
            mFanGroupBox.Controls.Add(mRemoveButton);
            mFanGroupBox.Controls.Add(mAddButton);
            mFanGroupBox.Location = new System.Drawing.Point(12, 278);
            mFanGroupBox.Name = "mFanGroupBox";
            mFanGroupBox.Size = new System.Drawing.Size(305, 416);
            mFanGroupBox.TabIndex = 2;
            mFanGroupBox.TabStop = false;
            mFanGroupBox.Text = "Fan";
            // 
            // mFanListView
            // 
            mFanListView.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            mFanListView.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            mFanListView.FullRowSelect = true;
            mFanListView.HeaderStyle = ColumnHeaderStyle.None;
            mFanListView.Location = new System.Drawing.Point(5, 199);
            mFanListView.MultiSelect = false;
            mFanListView.Name = "mFanListView";
            mFanListView.Size = new System.Drawing.Size(292, 174);
            mFanListView.TabIndex = 7;
            mFanListView.UseCompatibleStateImageBehavior = false;
            mFanListView.View = View.Details;
            // 
            // mAddFanListView
            // 
            mAddFanListView.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);
            mAddFanListView.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            mAddFanListView.FullRowSelect = true;
            mAddFanListView.HeaderStyle = ColumnHeaderStyle.None;
            mAddFanListView.Location = new System.Drawing.Point(5, 20);
            mAddFanListView.Name = "mAddFanListView";
            mAddFanListView.Size = new System.Drawing.Size(292, 136);
            mAddFanListView.TabIndex = 6;
            mAddFanListView.UseCompatibleStateImageBehavior = false;
            mAddFanListView.View = View.Details;
            // 
            // mRemoveButton
            // 
            mRemoveButton.Location = new System.Drawing.Point(5, 379);
            mRemoveButton.Name = "mRemoveButton";
            mRemoveButton.Padding = new Padding(1);
            mRemoveButton.Size = new System.Drawing.Size(292, 31);
            mRemoveButton.TabIndex = 4;
            mRemoveButton.Text = "Remove";
            mRemoveButton.Click += onRemoveButtonClick;
            // 
            // mAddButton
            // 
            mAddButton.Location = new System.Drawing.Point(5, 162);
            mAddButton.Name = "mAddButton";
            mAddButton.Padding = new Padding(1);
            mAddButton.Size = new System.Drawing.Size(290, 31);
            mAddButton.TabIndex = 3;
            mAddButton.Text = "Add";
            mAddButton.Click += onAddButtonClick;
            // 
            // mGraphGroupBox
            // 
            mGraphGroupBox.BorderColor = System.Drawing.Color.FromArgb(51, 51, 51);
            mGraphGroupBox.Controls.Add(mPresetLabel);
            mGraphGroupBox.Controls.Add(mUnitLabel);
            mGraphGroupBox.Controls.Add(mHysLabel);
            mGraphGroupBox.Controls.Add(mStepCheckBox);
            mGraphGroupBox.Controls.Add(mGraph);
            mGraphGroupBox.Location = new System.Drawing.Point(323, 61);
            mGraphGroupBox.Name = "mGraphGroupBox";
            mGraphGroupBox.Size = new System.Drawing.Size(870, 633);
            mGraphGroupBox.TabIndex = 4;
            mGraphGroupBox.TabStop = false;
            mGraphGroupBox.Text = "Graph";
            // 
            // mPresetLabel
            // 
            mPresetLabel.AutoSize = true;
            mPresetLabel.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            mPresetLabel.Location = new System.Drawing.Point(144, 1);
            mPresetLabel.Name = "mPresetLabel";
            mPresetLabel.Size = new System.Drawing.Size(45, 15);
            mPresetLabel.TabIndex = 5;
            mPresetLabel.Text = "Preset :";
            mPresetLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // mUnitLabel
            // 
            mUnitLabel.AutoSize = true;
            mUnitLabel.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            mUnitLabel.Location = new System.Drawing.Point(329, 1);
            mUnitLabel.Name = "mUnitLabel";
            mUnitLabel.Size = new System.Drawing.Size(35, 15);
            mUnitLabel.TabIndex = 4;
            mUnitLabel.Text = "Unit :";
            mUnitLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // mHysLabel
            // 
            mHysLabel.AutoSize = true;
            mHysLabel.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            mHysLabel.Location = new System.Drawing.Point(434, 1);
            mHysLabel.Name = "mHysLabel";
            mHysLabel.Size = new System.Drawing.Size(66, 15);
            mHysLabel.TabIndex = 4;
            mHysLabel.Text = "Hysteresis :";
            mHysLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // mStepCheckBox
            // 
            mStepCheckBox.AutoSize = true;
            mStepCheckBox.Location = new System.Drawing.Point(573, 0);
            mStepCheckBox.Name = "mStepCheckBox";
            mStepCheckBox.Size = new System.Drawing.Size(49, 19);
            mStepCheckBox.TabIndex = 13;
            mStepCheckBox.Text = "Step";
            mStepCheckBox.CheckedChanged += onStepCheckBoxCheckedChanged;
            // 
            // mGraph
            // 
            mGraph.Location = new System.Drawing.Point(6, 23);
            mGraph.Margin = new Padding(4, 3, 4, 3);
            mGraph.Name = "mGraph";
            mGraph.ScrollGrace = 0D;
            mGraph.ScrollMaxX = 0D;
            mGraph.ScrollMaxY = 0D;
            mGraph.ScrollMaxY2 = 0D;
            mGraph.ScrollMinX = 0D;
            mGraph.ScrollMinY = 0D;
            mGraph.ScrollMinY2 = 0D;
            mGraph.Size = new System.Drawing.Size(857, 604);
            mGraph.TabIndex = 4;
            mGraph.ZoomButtons = MouseButtons.None;
            // 
            // mOKButton
            // 
            mOKButton.Location = new System.Drawing.Point(1012, 700);
            mOKButton.Name = "mOKButton";
            mOKButton.Padding = new Padding(1);
            mOKButton.Size = new System.Drawing.Size(181, 47);
            mOKButton.TabIndex = 17;
            mOKButton.Text = "OK";
            mOKButton.Click += onOKButtonClick;
            // 
            // mApplyButton
            // 
            mApplyButton.Location = new System.Drawing.Point(825, 700);
            mApplyButton.Name = "mApplyButton";
            mApplyButton.Padding = new Padding(1);
            mApplyButton.Size = new System.Drawing.Size(181, 47);
            mApplyButton.TabIndex = 16;
            mApplyButton.Text = "Apply";
            mApplyButton.Click += onApplyButtonClick;
            // 
            // mHysNumericUpDown
            // 
            mHysNumericUpDown.Location = new System.Drawing.Point(832, 59);
            mHysNumericUpDown.Name = "mHysNumericUpDown";
            mHysNumericUpDown.ReadOnly = true;
            mHysNumericUpDown.Size = new System.Drawing.Size(38, 23);
            mHysNumericUpDown.TabIndex = 12;
            mHysNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // mModeGroupBox
            // 
            mModeGroupBox.BorderColor = System.Drawing.Color.FromArgb(51, 51, 51);
            mModeGroupBox.Controls.Add(mGameRadioButton);
            mModeGroupBox.Controls.Add(mPerformanceRadioButton);
            mModeGroupBox.Controls.Add(mSilenceRadioButton);
            mModeGroupBox.Controls.Add(mNormalRadioButton);
            mModeGroupBox.Controls.Add(mImportIntelligentButton);
            mModeGroupBox.Location = new System.Drawing.Point(323, 9);
            mModeGroupBox.Name = "mModeGroupBox";
            mModeGroupBox.Size = new System.Drawing.Size(870, 43);
            mModeGroupBox.TabIndex = 3;
            mModeGroupBox.TabStop = false;
            mModeGroupBox.Text = "Mode";
            // 
            // mGameRadioButton
            // 
            mGameRadioButton.AutoSize = true;
            mGameRadioButton.Location = new System.Drawing.Point(384, 18);
            mGameRadioButton.Name = "mGameRadioButton";
            mGameRadioButton.Size = new System.Drawing.Size(56, 19);
            mGameRadioButton.TabIndex = 8;
            mGameRadioButton.TabStop = true;
            mGameRadioButton.Text = "Game";
            // 
            // mPerformanceRadioButton
            // 
            mPerformanceRadioButton.AutoSize = true;
            mPerformanceRadioButton.Location = new System.Drawing.Point(255, 18);
            mPerformanceRadioButton.Name = "mPerformanceRadioButton";
            mPerformanceRadioButton.Size = new System.Drawing.Size(93, 19);
            mPerformanceRadioButton.TabIndex = 7;
            mPerformanceRadioButton.TabStop = true;
            mPerformanceRadioButton.Text = "Performance";
            // 
            // mSilenceRadioButton
            // 
            mSilenceRadioButton.AutoSize = true;
            mSilenceRadioButton.Location = new System.Drawing.Point(146, 18);
            mSilenceRadioButton.Name = "mSilenceRadioButton";
            mSilenceRadioButton.Size = new System.Drawing.Size(62, 19);
            mSilenceRadioButton.TabIndex = 6;
            mSilenceRadioButton.TabStop = true;
            mSilenceRadioButton.Text = "Silence";
            // 
            // mNormalRadioButton
            // 
            mNormalRadioButton.AutoSize = true;
            mNormalRadioButton.Location = new System.Drawing.Point(41, 18);
            mNormalRadioButton.Name = "mNormalRadioButton";
            mNormalRadioButton.Size = new System.Drawing.Size(65, 19);
            mNormalRadioButton.TabIndex = 5;
            mNormalRadioButton.TabStop = true;
            mNormalRadioButton.Text = "Normal";
            // 
            // mImportIntelligentButton
            // 
            mImportIntelligentButton.Location = new System.Drawing.Point(744, 14);
            mImportIntelligentButton.Name = "mImportIntelligentButton";
            mImportIntelligentButton.Padding = new Padding(1);
            mImportIntelligentButton.Size = new System.Drawing.Size(120, 23);
            mImportIntelligentButton.TabIndex = 11;
            mImportIntelligentButton.Text = "Импорт из Умного";
            mImportIntelligentButton.Click += onImportIntelligentButtonClick;
            // 
            // mUnitComboBox
            // 
            mUnitComboBox.DrawMode = DrawMode.OwnerDrawVariable;
            mUnitComboBox.FormattingEnabled = true;
            mUnitComboBox.Location = new System.Drawing.Point(689, 58);
            mUnitComboBox.Name = "mUnitComboBox";
            mUnitComboBox.Size = new System.Drawing.Size(44, 24);
            mUnitComboBox.TabIndex = 11;
            // 
            // mPresetLoadButton
            // 
            mPresetLoadButton.Location = new System.Drawing.Point(520, 56);
            mPresetLoadButton.Name = "mPresetLoadButton";
            mPresetLoadButton.Padding = new Padding(1);
            mPresetLoadButton.Size = new System.Drawing.Size(57, 23);
            mPresetLoadButton.TabIndex = 9;
            mPresetLoadButton.Text = "Load";
            mPresetLoadButton.Click += onPresetLoadButtonClick;
            // 
            // mPresetSaveButton
            // 
            mPresetSaveButton.Location = new System.Drawing.Point(579, 56);
            mPresetSaveButton.Name = "mPresetSaveButton";
            mPresetSaveButton.Padding = new Padding(1);
            mPresetSaveButton.Size = new System.Drawing.Size(57, 23);
            mPresetSaveButton.TabIndex = 10;
            mPresetSaveButton.Text = "Save";
            mPresetSaveButton.Click += onPresetSaveButtonClick;
            // 
            // mAutoNumericUpDown
            // 
            mAutoNumericUpDown.AutoSize = true;
            mAutoNumericUpDown.Increment = new decimal(new int[] { 5, 0, 0, 0 });
            mAutoNumericUpDown.Location = new System.Drawing.Point(998, 59);
            mAutoNumericUpDown.Name = "mAutoNumericUpDown";
            mAutoNumericUpDown.ReadOnly = true;
            mAutoNumericUpDown.Size = new System.Drawing.Size(41, 23);
            mAutoNumericUpDown.TabIndex = 14;
            mAutoNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // mAutoLabel
            // 
            mAutoLabel.AutoSize = true;
            mAutoLabel.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            mAutoLabel.Location = new System.Drawing.Point(957, 62);
            mAutoLabel.Name = "mAutoLabel";
            mAutoLabel.Size = new System.Drawing.Size(39, 15);
            mAutoLabel.TabIndex = 6;
            mAutoLabel.Text = "Auto :";
            mAutoLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // mDelayLabel
            // 
            mDelayLabel.AutoSize = true;
            mDelayLabel.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            mDelayLabel.Location = new System.Drawing.Point(1061, 62);
            mDelayLabel.Name = "mDelayLabel";
            mDelayLabel.Size = new System.Drawing.Size(42, 15);
            mDelayLabel.TabIndex = 8;
            mDelayLabel.Text = "Delay :";
            mDelayLabel.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // mDelayNumericUpDown
            // 
            mDelayNumericUpDown.Location = new System.Drawing.Point(1109, 58);
            mDelayNumericUpDown.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            mDelayNumericUpDown.Name = "mDelayNumericUpDown";
            mDelayNumericUpDown.Size = new System.Drawing.Size(50, 23);
            mDelayNumericUpDown.TabIndex = 15;
            mDelayNumericUpDown.TextAlign = HorizontalAlignment.Center;
            // 
            // mDelayLabel2
            // 
            mDelayLabel2.AutoSize = true;
            mDelayLabel2.ForeColor = System.Drawing.Color.FromArgb(220, 220, 220);
            mDelayLabel2.Location = new System.Drawing.Point(1161, 61);
            mDelayLabel2.Name = "mDelayLabel2";
            mDelayLabel2.Size = new System.Drawing.Size(23, 15);
            mDelayLabel2.TabIndex = 18;
            mDelayLabel2.Text = "ms";
            mDelayLabel2.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // ControlForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new System.Drawing.Size(1203, 757);
            Controls.Add(mDelayLabel2);
            Controls.Add(mDelayLabel);
            Controls.Add(mDelayNumericUpDown);
            Controls.Add(mAutoLabel);
            Controls.Add(mAutoNumericUpDown);
            Controls.Add(mPresetSaveButton);
            Controls.Add(mPresetLoadButton);
            Controls.Add(mUnitComboBox);
            Controls.Add(mModeGroupBox);
            Controls.Add(mHysNumericUpDown);
            Controls.Add(mApplyButton);
            Controls.Add(mOKButton);
            Controls.Add(mGraphGroupBox);
            Controls.Add(mFanGroupBox);
            Controls.Add(mTempGroupBox);
            Controls.Add(mEnableCheckBox);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Location = new System.Drawing.Point(0, 0);
            MinimizeBox = false;
            MinimumSize = new System.Drawing.Size(1219, 796);
            Name = "ControlForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "AutoFAN";
            mTempGroupBox.ResumeLayout(false);
            mFanGroupBox.ResumeLayout(false);
            mGraphGroupBox.ResumeLayout(false);
            mGraphGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)mHysNumericUpDown).EndInit();
            mModeGroupBox.ResumeLayout(false);
            mModeGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)mAutoNumericUpDown).EndInit();
            ((System.ComponentModel.ISupportInitialize)mDelayNumericUpDown).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private DarkCheckBox mEnableCheckBox;
        private DarkGroupBox mTempGroupBox;
        private DarkGroupBox mFanGroupBox;
        private DarkButton mRemoveButton;
        private DarkButton mAddButton;
        private DarkGroupBox mGraphGroupBox;
        private ZedGraph.ZedGraphControl mGraph;
        private DarkCheckBox mStepCheckBox;
        private DarkButton mOKButton;
        private DarkButton mApplyButton;
        private DarkNumericUpDown mHysNumericUpDown;
        private DarkLabel mHysLabel;
        private DarkGroupBox mModeGroupBox;
        private DarkRadioButton mGameRadioButton;
        private DarkRadioButton mPerformanceRadioButton;
        private DarkRadioButton mSilenceRadioButton;
        private DarkRadioButton mNormalRadioButton;
        private DarkLabel mUnitLabel;
        private DarkComboBox mUnitComboBox;
        private DarkLabel mPresetLabel;
        private DarkButton mPresetLoadButton;
        private DarkButton mPresetSaveButton;
        private DarkButton mImportIntelligentButton;
        private DarkNumericUpDown mAutoNumericUpDown;
        private DarkLabel mAutoLabel;
        private DarkLabel mDelayLabel;
        private DarkNumericUpDown mDelayNumericUpDown;
        private DarkLabel mDelayLabel2;
        private ThemeListView mAddTempListView;
        private ThemeListView mAddFanListView;
        private ThemeListView mFanListView;
    }
}
