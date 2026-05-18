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
        private System.Windows.Forms.ListView mResultListView;
        private System.Windows.Forms.ColumnHeader chTemp;
        private System.Windows.Forms.ColumnHeader chControl;
        private System.Windows.Forms.ColumnHeader chDelta;
        private System.Windows.Forms.DataGridView mMappingDataGridView;
        private System.Windows.Forms.Button mAcceptButton;
        private System.Windows.Forms.Button mCreateProfileButton;
        private System.Windows.Forms.Button mCancelButton;

        private void InitializeComponent()
        {
            this.mStartButton = new System.Windows.Forms.Button();
            this.mStopButton = new System.Windows.Forms.Button();
            this.mProgressBar = new System.Windows.Forms.ProgressBar();
            this.mResultListView = new System.Windows.Forms.ListView();
            this.chTemp = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chControl = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.chDelta = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.mMappingDataGridView = new System.Windows.Forms.DataGridView();
            this.mAcceptButton = new System.Windows.Forms.Button();
            this.mCreateProfileButton = new System.Windows.Forms.Button();
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
            // mResultListView
            // 
            this.mResultListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.chTemp,
            this.chControl,
            this.chDelta});
            this.mResultListView.FullRowSelect = true;
            this.mResultListView.GridLines = true;
            this.mResultListView.Location = new System.Drawing.Point(16, 64);
            this.mResultListView.Name = "mResultListView";
            this.mResultListView.Size = new System.Drawing.Size(552, 150);
            this.mResultListView.TabIndex = 3;
            this.mResultListView.UseCompatibleStateImageBehavior = false;
            this.mResultListView.View = System.Windows.Forms.View.Details;
            // 
            // chTemp
            // 
            this.chTemp.Text = "Temperature Sensor";
            this.chTemp.Width = 300;
            // 
            // chControl
            // 
            this.chControl.Text = "Control";
            this.chControl.Width = 180;
            // 
            // chDelta
            // 
            this.chDelta.Text = "Delta";
            this.chDelta.Width = 70;
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
            this.mAcceptButton.Location = new System.Drawing.Point(16, 377);
            this.mAcceptButton.Name = "mAcceptButton";
            this.mAcceptButton.Size = new System.Drawing.Size(100, 30);
            this.mAcceptButton.TabIndex = 5;
            this.mAcceptButton.Text = "Accept";
            this.mAcceptButton.UseVisualStyleBackColor = true;
            this.mAcceptButton.Enabled = false;
            // 
            // mCreateProfileButton
            // 
            this.mCreateProfileButton.Location = new System.Drawing.Point(128, 377);
            this.mCreateProfileButton.Name = "mCreateProfileButton";
            this.mCreateProfileButton.Size = new System.Drawing.Size(150, 30);
            this.mCreateProfileButton.TabIndex = 6;
            this.mCreateProfileButton.Text = "Create Profile";
            this.mCreateProfileButton.UseVisualStyleBackColor = true;
            this.mCreateProfileButton.Enabled = false;
            // 
            // mCancelButton
            // 
            this.mCancelButton.Location = new System.Drawing.Point(468, 377);
            this.mCancelButton.Name = "mCancelButton";
            this.mCancelButton.Size = new System.Drawing.Size(100, 30);
            this.mCancelButton.TabIndex = 7;
            this.mCancelButton.Text = "Cancel";
            this.mCancelButton.UseVisualStyleBackColor = true;
            // 
            // IntelligentForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(580, 420);
            this.Controls.Add(this.mCancelButton);
            this.Controls.Add(this.mCreateProfileButton);
            this.Controls.Add(this.mAcceptButton);
            this.Controls.Add(this.mMappingDataGridView);
            this.Controls.Add(this.mResultListView);
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

