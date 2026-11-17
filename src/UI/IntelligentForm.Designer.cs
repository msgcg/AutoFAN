namespace FanCtrl
{
    partial class IntelligentForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private DarkUI.Controls.DarkButton mStartButton;
        private DarkUI.Controls.DarkButton mStopButton;
        private System.Windows.Forms.ProgressBar mProgressBar;
        private System.Windows.Forms.ListBox mResultListBox;
        private System.Windows.Forms.DataGridView mMappingDataGridView;
        private DarkUI.Controls.DarkButton mAcceptButton;

        private DarkUI.Controls.DarkButton mOptimizeAIButton;
        private DarkUI.Controls.DarkButton mCancelButton;

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IntelligentForm));
            mStartButton = new DarkUI.Controls.DarkButton();
            mStopButton = new DarkUI.Controls.DarkButton();
            mProgressBar = new System.Windows.Forms.ProgressBar();
            mResultListBox = new System.Windows.Forms.ListBox();
            mMappingDataGridView = new System.Windows.Forms.DataGridView();
            mAcceptButton = new DarkUI.Controls.DarkButton();
            mOptimizeAIButton = new DarkUI.Controls.DarkButton();
            mCancelButton = new DarkUI.Controls.DarkButton();
            ((System.ComponentModel.ISupportInitialize)mMappingDataGridView).BeginInit();
            SuspendLayout();
            // 
            // mStartButton
            // 
            mStartButton.Location = new System.Drawing.Point(16, 16);
            mStartButton.Name = "mStartButton";
            mStartButton.Padding = new System.Windows.Forms.Padding(1);
            mStartButton.Size = new System.Drawing.Size(100, 30);
            mStartButton.TabIndex = 0;
            mStartButton.Text = "Start";
            // 
            // mStopButton
            // 
            mStopButton.Location = new System.Drawing.Point(128, 16);
            mStopButton.Name = "mStopButton";
            mStopButton.Padding = new System.Windows.Forms.Padding(1);
            mStopButton.Size = new System.Drawing.Size(100, 30);
            mStopButton.TabIndex = 1;
            mStopButton.Text = "Stop";
            // 
            // mProgressBar
            // 
            mProgressBar.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            mProgressBar.Location = new System.Drawing.Point(236, 16);
            mProgressBar.Name = "mProgressBar";
            mProgressBar.Size = new System.Drawing.Size(608, 30);
            mProgressBar.TabIndex = 2;
            // 
            // mResultListBox
            // 
            mResultListBox.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            mResultListBox.BackColor = System.Drawing.SystemColors.ControlDark;
            mResultListBox.HorizontalScrollbar = true;
            mResultListBox.ItemHeight = 15;
            mResultListBox.Location = new System.Drawing.Point(16, 58);
            mResultListBox.Name = "mResultListBox";
            mResultListBox.Size = new System.Drawing.Size(828, 140);
            mResultListBox.TabIndex = 3;
            // 
            // mMappingDataGridView
            // 
            mMappingDataGridView.AllowUserToAddRows = false;
            mMappingDataGridView.AllowUserToDeleteRows = false;
            mMappingDataGridView.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
            mMappingDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            mMappingDataGridView.Location = new System.Drawing.Point(16, 210);
            mMappingDataGridView.Name = "mMappingDataGridView";
            mMappingDataGridView.RowHeadersVisible = false;
            mMappingDataGridView.Size = new System.Drawing.Size(828, 288);
            mMappingDataGridView.TabIndex = 4;
            // 
            // mAcceptButton
            // 
            mAcceptButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
            mAcceptButton.Enabled = false;
            mAcceptButton.Location = new System.Drawing.Point(16, 512);
            mAcceptButton.Name = "mAcceptButton";
            mAcceptButton.Padding = new System.Windows.Forms.Padding(1);
            mAcceptButton.Size = new System.Drawing.Size(110, 32);
            mAcceptButton.TabIndex = 5;
            mAcceptButton.Text = "Accept";
            // 
            // mOptimizeAIButton
            // 
            mOptimizeAIButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            mOptimizeAIButton.Enabled = false;
            mOptimizeAIButton.Location = new System.Drawing.Point(544, 512);
            mOptimizeAIButton.Name = "mOptimizeAIButton";
            mOptimizeAIButton.Padding = new System.Windows.Forms.Padding(1);
            mOptimizeAIButton.Size = new System.Drawing.Size(160, 32);
            mOptimizeAIButton.TabIndex = 8;
            mOptimizeAIButton.Text = "AI Optimize";
            // 
            // mCancelButton
            // 
            mCancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right;
            mCancelButton.Location = new System.Drawing.Point(714, 512);
            mCancelButton.Name = "mCancelButton";
            mCancelButton.Padding = new System.Windows.Forms.Padding(1);
            mCancelButton.Size = new System.Drawing.Size(130, 32);
            mCancelButton.TabIndex = 7;
            mCancelButton.Text = "Cancel";
            // 
            // IntelligentForm
            // 
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            ClientSize = new System.Drawing.Size(860, 560);
            MinimumSize = new System.Drawing.Size(760, 480);
            Controls.Add(mCancelButton);
            Controls.Add(mOptimizeAIButton);
            Controls.Add(mAcceptButton);
            Controls.Add(mMappingDataGridView);
            Controls.Add(mResultListBox);
            Controls.Add(mProgressBar);
            Controls.Add(mStopButton);
            Controls.Add(mStartButton);
            Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
            Location = new System.Drawing.Point(0, 0);
            MaximizeBox = true;
            MinimizeBox = false;
            Name = "IntelligentForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Intelligent Mode";
            ((System.ComponentModel.ISupportInitialize)mMappingDataGridView).EndInit();
            ResumeLayout(false);
        }
    }
}

