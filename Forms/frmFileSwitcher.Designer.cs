namespace NppPIALexer2.Forms
{
    partial class frmFileSwitcher
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
            this.txtFileName = new System.Windows.Forms.TextBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lviewFileList = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // txtFileName
            // 
            this.txtFileName.Location = new System.Drawing.Point(13, 13);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.Size = new System.Drawing.Size(320, 21);
            this.txtFileName.TabIndex = 0;
            this.txtFileName.VisibleChanged += new System.EventHandler(this.txtFileName_VisibleChanged);
            this.txtFileName.TextChanged += new System.EventHandler(this.txtFileName_TextChanged);
            this.txtFileName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtFileName_KeyDown);
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(338, 12);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(50, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(394, 12);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(50, 23);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lviewFileList
            // 
            this.lviewFileList.FullRowSelect = true;
            this.lviewFileList.GridLines = true;
            this.lviewFileList.Location = new System.Drawing.Point(12, 40);
            this.lviewFileList.MultiSelect = false;
            this.lviewFileList.Name = "lviewFileList";
            this.lviewFileList.Size = new System.Drawing.Size(433, 276);
            this.lviewFileList.TabIndex = 3;
            this.lviewFileList.UseCompatibleStateImageBehavior = false;
            this.lviewFileList.View = System.Windows.Forms.View.Details;
            this.lviewFileList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.lviewFileList_MouseDoubleClick);
            this.lviewFileList.MouseClick += new System.Windows.Forms.MouseEventHandler(this.lviewFileList_MouseClick);
            this.lviewFileList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lviewFileList_KeyDown);
            // 
            // frmFileSwitcher
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(457, 328);
            this.Controls.Add(this.lviewFileList);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtFileName);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmFileSwitcher";
            this.ShowInTaskbar = false;
            this.Text = "NppPIALexer2 File Switcher";
            this.TopMost = true;
            this.Deactivate += new System.EventHandler(this.frmFileSwitcher_Deactivate);
            this.Activated += new System.EventHandler(this.frmFileSwitcher_Activated);
            this.VisibleChanged += new System.EventHandler(this.frmFileSwitcher_VisibleChanged);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmFileSwitcher_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtFileName;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ListView lviewFileList;
    }
}