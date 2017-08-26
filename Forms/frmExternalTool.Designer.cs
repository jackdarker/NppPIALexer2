namespace NppPIALexer2.Forms
{
    partial class frmExternalTool
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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.lboxMenuItems = new System.Windows.Forms.ListBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnMoveUP = new System.Windows.Forms.Button();
            this.btnMoveDown = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtTitle = new System.Windows.Forms.TextBox();
            this.txtCmd = new System.Windows.Forms.TextBox();
            this.txtArgs = new System.Windows.Forms.TextBox();
            this.txtInitDir = new System.Windows.Forms.TextBox();
            this.btnSelectCmd = new System.Windows.Forms.Button();
            this.btnArgs = new System.Windows.Forms.Button();
            this.btnInitDir = new System.Windows.Forms.Button();
            this.ctntArgs = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.projectDirToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.projectFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.activeFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.activeFileToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.activeDirToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.activeFileDirToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ctntInitDir = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.projectDirToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.activeFileDirToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.dlgSelectCmd = new System.Windows.Forms.OpenFileDialog();
            this.ctntArgs.SuspendLayout();
            this.ctntInitDir.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "Menu contents:";
            // 
            // lboxMenuItems
            // 
            this.lboxMenuItems.FormattingEnabled = true;
            this.lboxMenuItems.ItemHeight = 12;
            this.lboxMenuItems.Location = new System.Drawing.Point(12, 29);
            this.lboxMenuItems.Name = "lboxMenuItems";
            this.lboxMenuItems.Size = new System.Drawing.Size(288, 136);
            this.lboxMenuItems.TabIndex = 1;
            this.lboxMenuItems.SelectedIndexChanged += new System.EventHandler(this.lboxMenuItems_SelectedIndexChanged);
            this.lboxMenuItems.DoubleClick += new System.EventHandler(this.lboxMenuItems_DoubleClick);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(307, 28);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 2;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Enabled = false;
            this.btnDelete.Location = new System.Drawing.Point(307, 58);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 3;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnMoveUP
            // 
            this.btnMoveUP.Enabled = false;
            this.btnMoveUP.Location = new System.Drawing.Point(306, 112);
            this.btnMoveUP.Name = "btnMoveUP";
            this.btnMoveUP.Size = new System.Drawing.Size(75, 23);
            this.btnMoveUP.TabIndex = 4;
            this.btnMoveUP.Text = "Move Up";
            this.btnMoveUP.UseVisualStyleBackColor = true;
            this.btnMoveUP.Click += new System.EventHandler(this.btnMoveUP_Click);
            // 
            // btnMoveDown
            // 
            this.btnMoveDown.Enabled = false;
            this.btnMoveDown.Location = new System.Drawing.Point(307, 141);
            this.btnMoveDown.Name = "btnMoveDown";
            this.btnMoveDown.Size = new System.Drawing.Size(75, 23);
            this.btnMoveDown.TabIndex = 5;
            this.btnMoveDown.Text = "Move Down";
            this.btnMoveDown.UseVisualStyleBackColor = true;
            this.btnMoveDown.Click += new System.EventHandler(this.btnMoveDown_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 177);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 6;
            this.label2.Text = "Title: ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 202);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 7;
            this.label3.Text = "Command:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 226);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "Arguments:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 252);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(113, 12);
            this.label5.TabIndex = 9;
            this.label5.Text = "Initial Directory:";
            // 
            // txtTitle
            // 
            this.txtTitle.Location = new System.Drawing.Point(64, 172);
            this.txtTitle.Name = "txtTitle";
            this.txtTitle.Size = new System.Drawing.Size(280, 21);
            this.txtTitle.TabIndex = 10;
            // 
            // txtCmd
            // 
            this.txtCmd.Location = new System.Drawing.Point(64, 197);
            this.txtCmd.Name = "txtCmd";
            this.txtCmd.Size = new System.Drawing.Size(280, 21);
            this.txtCmd.TabIndex = 11;
            // 
            // txtArgs
            // 
            this.txtArgs.Location = new System.Drawing.Point(81, 222);
            this.txtArgs.Name = "txtArgs";
            this.txtArgs.Size = new System.Drawing.Size(263, 21);
            this.txtArgs.TabIndex = 12;
            // 
            // txtInitDir
            // 
            this.txtInitDir.Location = new System.Drawing.Point(123, 248);
            this.txtInitDir.Name = "txtInitDir";
            this.txtInitDir.Size = new System.Drawing.Size(221, 21);
            this.txtInitDir.TabIndex = 13;
            // 
            // btnSelectCmd
            // 
            this.btnSelectCmd.Location = new System.Drawing.Point(350, 195);
            this.btnSelectCmd.Name = "btnSelectCmd";
            this.btnSelectCmd.Size = new System.Drawing.Size(31, 23);
            this.btnSelectCmd.TabIndex = 14;
            this.btnSelectCmd.Text = "...";
            this.btnSelectCmd.UseVisualStyleBackColor = true;
            this.btnSelectCmd.Click += new System.EventHandler(this.btnSelectCmd_Click);
            // 
            // btnArgs
            // 
            this.btnArgs.Location = new System.Drawing.Point(350, 220);
            this.btnArgs.Name = "btnArgs";
            this.btnArgs.Size = new System.Drawing.Size(31, 23);
            this.btnArgs.TabIndex = 14;
            this.btnArgs.Text = "→";
            this.btnArgs.UseVisualStyleBackColor = true;
            this.btnArgs.Click += new System.EventHandler(this.btnArgs_Click);
            // 
            // btnInitDir
            // 
            this.btnInitDir.Location = new System.Drawing.Point(350, 247);
            this.btnInitDir.Name = "btnInitDir";
            this.btnInitDir.Size = new System.Drawing.Size(31, 23);
            this.btnInitDir.TabIndex = 14;
            this.btnInitDir.Text = "→";
            this.btnInitDir.UseVisualStyleBackColor = true;
            this.btnInitDir.Click += new System.EventHandler(this.btnInitDir_Click);
            // 
            // ctntArgs
            // 
            this.ctntArgs.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectDirToolStripMenuItem,
            this.projectFileToolStripMenuItem,
            this.activeFileToolStripMenuItem,
            this.toolStripSeparator1,
            this.activeFileToolStripMenuItem1,
            this.activeDirToolStripMenuItem,
            this.activeFileDirToolStripMenuItem});
            this.ctntArgs.Name = "ctntArgs";
            this.ctntArgs.Size = new System.Drawing.Size(173, 142);
            // 
            // projectDirToolStripMenuItem
            // 
            this.projectDirToolStripMenuItem.Name = "projectDirToolStripMenuItem";
            this.projectDirToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.projectDirToolStripMenuItem.Text = "$(ProjectDir)";
            // 
            // projectFileToolStripMenuItem
            // 
            this.projectFileToolStripMenuItem.Name = "projectFileToolStripMenuItem";
            this.projectFileToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.projectFileToolStripMenuItem.Text = "$(ProjectFile)";
            // 
            // activeFileToolStripMenuItem
            // 
            this.activeFileToolStripMenuItem.Name = "activeFileToolStripMenuItem";
            this.activeFileToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.activeFileToolStripMenuItem.Text = "$(ProjectName)";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(169, 6);
            // 
            // activeFileToolStripMenuItem1
            // 
            this.activeFileToolStripMenuItem1.Name = "activeFileToolStripMenuItem1";
            this.activeFileToolStripMenuItem1.Size = new System.Drawing.Size(172, 22);
            this.activeFileToolStripMenuItem1.Text = "$(ActiveFile)";
            // 
            // activeDirToolStripMenuItem
            // 
            this.activeDirToolStripMenuItem.Name = "activeDirToolStripMenuItem";
            this.activeDirToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.activeDirToolStripMenuItem.Text = "$(ActiveFileDir)";
            // 
            // activeFileDirToolStripMenuItem
            // 
            this.activeFileDirToolStripMenuItem.Name = "activeFileDirToolStripMenuItem";
            this.activeFileDirToolStripMenuItem.Size = new System.Drawing.Size(172, 22);
            this.activeFileDirToolStripMenuItem.Text = "$(ActiveFileName)";
            // 
            // ctntInitDir
            // 
            this.ctntInitDir.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.projectDirToolStripMenuItem1,
            this.activeFileDirToolStripMenuItem1});
            this.ctntInitDir.Name = "ctntInitDir";
            this.ctntInitDir.Size = new System.Drawing.Size(167, 48);
            // 
            // projectDirToolStripMenuItem1
            // 
            this.projectDirToolStripMenuItem1.Name = "projectDirToolStripMenuItem1";
            this.projectDirToolStripMenuItem1.Size = new System.Drawing.Size(166, 22);
            this.projectDirToolStripMenuItem1.Text = "$(ProjectDir)";
            // 
            // activeFileDirToolStripMenuItem1
            // 
            this.activeFileDirToolStripMenuItem1.Name = "activeFileDirToolStripMenuItem1";
            this.activeFileDirToolStripMenuItem1.Size = new System.Drawing.Size(166, 22);
            this.activeFileDirToolStripMenuItem1.Text = "$(ActiveFileDir)";
            // 
            // dlgSelectCmd
            // 
            this.dlgSelectCmd.FileName = "Open File";
            this.dlgSelectCmd.Filter = "(All executables)|*.exe;*.com;*.pif;*.bat;*.cmd|All(*.*)|*.*";
            // 
            // frmExternalTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(394, 278);
            this.Controls.Add(this.btnInitDir);
            this.Controls.Add(this.btnArgs);
            this.Controls.Add(this.btnSelectCmd);
            this.Controls.Add(this.txtInitDir);
            this.Controls.Add(this.txtArgs);
            this.Controls.Add(this.txtCmd);
            this.Controls.Add(this.txtTitle);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnMoveDown);
            this.Controls.Add(this.btnMoveUP);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.lboxMenuItems);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmExternalTool";
            this.ShowInTaskbar = false;
            this.Text = "NppPIALexer2 External Tool";
            this.TopMost = true;
            this.Deactivate += new System.EventHandler(this.frmExternalTool_Deactivate);
            this.Load += new System.EventHandler(this.frmExternalTool_Load);
            this.Activated += new System.EventHandler(this.frmExternalTool_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmExternalTool_FormClosing);
            this.ctntArgs.ResumeLayout(false);
            this.ctntInitDir.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox lboxMenuItems;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnMoveUP;
        private System.Windows.Forms.Button btnMoveDown;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtTitle;
        private System.Windows.Forms.TextBox txtCmd;
        private System.Windows.Forms.TextBox txtArgs;
        private System.Windows.Forms.TextBox txtInitDir;
        private System.Windows.Forms.Button btnSelectCmd;
        private System.Windows.Forms.Button btnArgs;
        private System.Windows.Forms.Button btnInitDir;
        private System.Windows.Forms.ContextMenuStrip ctntArgs;
        private System.Windows.Forms.ToolStripMenuItem projectDirToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem projectFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem activeFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem activeFileToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem activeDirToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem activeFileDirToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip ctntInitDir;
        private System.Windows.Forms.ToolStripMenuItem projectDirToolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem activeFileDirToolStripMenuItem1;
        private System.Windows.Forms.OpenFileDialog dlgSelectCmd;
    }
}