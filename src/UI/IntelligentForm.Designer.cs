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

        private System.Windows.Forms.Button mStartButton;
        private System.Windows.Forms.Button mStopButton;
        private System.Windows.Forms.ProgressBar mProgressBar;
        private System.Windows.Forms.ListBox mResultListBox;
        private System.Windows.Forms.DataGridView mMappingDataGridView;
        private System.Windows.Forms.Button mAcceptButton;
        private System.Windows.Forms.Button mCreateProfileButton;
        private System.Windows.Forms.Button mOptimizeAIButton;
        private System.Windows.Forms.Button mCancelButton;

        private void InitializeComponent()
        {
            this.mStartButton = new System.Windows.Forms.Button();
            this.mStopButton = new System.Windows.Forms.Button();
            this.mProgressBar = new System.Windows.Forms.ProgressBar();
            this.mResultListBox = new System.Windows.Forms.ListBox();
            this.mMappingDataGridView = new System.Windows.Forms.DataGridView();
            this.mAcceptButton = new System.Windows.Forms.Button();
            this.mCreateProfileButton = new System.Windows.Forms.Button();
            this.mOptimizeAIButton = new System.Windows.Forms.Button();
            this.mCancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.mMappingDataGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // mStartButton
            // 
            this.mStartButton.Location = new System.Drawing.Point(16, 16);
            this.mStartButton.Name = "mStartButton";
            this.mStartButton.Size = new System.Drawing.Size(100, 30);
            this.mStartButton.TabIndex = 0;
            this.mStartButton.Text = "Start";
            this.mStartButton.UseVisualStyleBackColor = true;
            // 
            // mStopButton
            // 
            this.mStopButton.Location = new System.Drawing.Point(128, 16);
            this.mStopButton.Name = "mStopButton";
            this.mStopButton.Size = new System.Drawing.Size(100, 30);
            this.mStopButton.TabIndex = 1;
            this.mStopButton.Text = "Stop";
            this.mStopButton.UseVisualStyleBackColor = true;
            // 
            // mProgressBar
            // 
            this.mProgressBar.Location = new System.Drawing.Point(240, 16);
            this.mProgressBar.Name = "mProgressBar";
            this.mProgressBar.Size = new System.Drawing.Size(328, 30);
            this.mProgressBar.TabIndex = 2;
            // 
            // mResultListBox
            // 
            this.mResultListBox.Location = new System.Drawing.Point(16, 64);
            this.mResultListBox.Name = "mResultListBox";
            this.mResultListBox.Size = new System.Drawing.Size(552, 147);
            this.mResultListBox.TabIndex = 3;
            // 
            // mMappingDataGridView
            // 
            this.mMappingDataGridView.AllowUserToAddRows = false;
            this.mMappingDataGridView.AllowUserToDeleteRows = false;
            this.mMappingDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.mMappingDataGridView.Location = new System.Drawing.Point(16, 220);
            this.mMappingDataGridView.Name = "mMappingDataGridView";
            this.mMappingDataGridView.RowHeadersVisible = false;
            this.mMappingDataGridView.Size = new System.Drawing.Size(552, 150);
            this.mMappingDataGridView.TabIndex = 4;
            // 
            // mAcceptButton
            // 
            this.mAcceptButton.Location = new System.Drawing.Point(16, 400);
            this.mAcceptButton.Name = "mAcceptButton";
            this.mAcceptButton.Size = new System.Drawing.Size(100, 30);
            this.mAcceptButton.TabIndex = 5;
            this.mAcceptButton.Text = "Accept";
            this.mAcceptButton.UseVisualStyleBackColor = true;
            this.mAcceptButton.Enabled = false;
            // 
            // mCreateProfileButton
            // 
            this.mCreateProfileButton.Location = new System.Drawing.Point(128, 400);
            this.mCreateProfileButton.Name = "mCreateProfileButton";
            this.mCreateProfileButton.Size = new System.Drawing.Size(150, 30);
            this.mCreateProfileButton.TabIndex = 6;
            this.mCreateProfileButton.Text = "Create Profile";
            this.mCreateProfileButton.UseVisualStyleBackColor = true;
            this.mCreateProfileButton.Enabled = false;
            // 
            // mOptimizeAIButton
            // 
            this.mOptimizeAIButton.Location = new System.Drawing.Point(290, 400);
            this.mOptimizeAIButton.Name = "mOptimizeAIButton";
            this.mOptimizeAIButton.Size = new System.Drawing.Size(150, 30);
            this.mOptimizeAIButton.TabIndex = 8;
            this.mOptimizeAIButton.Text = "AI Optimize";
            this.mOptimizeAIButton.UseVisualStyleBackColor = true;
            this.mOptimizeAIButton.Enabled = false;
            // 
            // mCancelButton
            // 
            this.mCancelButton.Location = new System.Drawing.Point(468, 400);
            this.mCancelButton.Name = "mCancelButton";
            this.mCancelButton.Size = new System.Drawing.Size(100, 30);
            this.mCancelButton.TabIndex = 7;
            this.mCancelButton.Text = "Cancel";
            this.mCancelButton.UseVisualStyleBackColor = true;
            // 
            // IntelligentForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(580, 440);
            this.Controls.Add(this.mCancelButton);
            this.Controls.Add(this.mOptimizeAIButton);
            this.Controls.Add(this.mCreateProfileButton);
            this.Controls.Add(this.mAcceptButton);
            this.Controls.Add(this.mMappingDataGridView);
            this.Controls.Add(this.mResultListBox);
            this.Controls.Add(this.mProgressBar);
            this.Controls.Add(this.mStopButton);
            this.Controls.Add(this.mStartButton);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "IntelligentForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Intelligent Mode";
            ((System.ComponentModel.ISupportInitialize)(this.mMappingDataGridView)).EndInit();
            this.ResumeLayout(false);
        }
    }
}

