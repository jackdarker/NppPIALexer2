namespace NppPIALexer2.Forms
{
    partial class frmSettings
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.cboxLangList = new System.Windows.Forms.CheckedListBox();
            this.chkEnableAutoC = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnEditTemplate = new System.Windows.Forms.Button();
            this.chkEnableTemplate = new System.Windows.Forms.CheckBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.lboxExtList = new System.Windows.Forms.ListBox();
            this.lboxLangList = new System.Windows.Forms.ListBox();
            this.txtExt = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.cboxLangList);
            this.groupBox1.Controls.Add(this.chkEnableAutoC);
            this.groupBox1.Location = new System.Drawing.Point(12, 14);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(149, 306);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "AutoCompletion";
            // 
            // cboxLangList
            // 
            this.cboxLangList.Enabled = false;
            this.cboxLangList.FormattingEnabled = true;
            this.cboxLangList.Location = new System.Drawing.Point(16, 49);
            this.cboxLangList.Name = "cboxLangList";
            this.cboxLangList.Size = new System.Drawing.Size(119, 244);
            this.cboxLangList.TabIndex = 1;
            // 
            // chkEnableAutoC
            // 
            this.chkEnableAutoC.AutoSize = true;
            this.chkEnableAutoC.Location = new System.Drawing.Point(16, 23);
            this.chkEnableAutoC.Name = "chkEnableAutoC";
            this.chkEnableAutoC.Size = new System.Drawing.Size(60, 16);
            this.chkEnableAutoC.TabIndex = 0;
            this.chkEnableAutoC.Text = "Enable";
            this.chkEnableAutoC.UseVisualStyleBackColor = true;
            this.chkEnableAutoC.CheckedChanged += new System.EventHandler(this.chkEnableAutoC_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.btnEditTemplate);
            this.groupBox2.Controls.Add(this.chkEnableTemplate);
            this.groupBox2.Location = new System.Drawing.Point(167, 14);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(181, 306);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Code Template";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(19, 276);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(83, 12);
            this.label9.TabIndex = 10;
            this.label9.Text = "$(ProjectDir)";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(19, 231);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(71, 12);
            this.label8.TabIndex = 9;
            this.label8.Text = "$(FileName)";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(19, 184);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(47, 12);
            this.label7.TabIndex = 8;
            this.label7.Text = "$(Time)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(19, 163);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(47, 12);
            this.label6.TabIndex = 7;
            this.label6.Text = "$(Date)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 254);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(89, 12);
            this.label5.TabIndex = 6;
            this.label5.Text = "$(ProjectName)";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 208);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 12);
            this.label4.TabIndex = 5;
            this.label4.Text = "$(FilePath)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 142);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(71, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "$(DateTime)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(21, 119);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "Micros:";
            // 
            // label1
            // 
            this.label1.AllowDrop = true;
            this.label1.Location = new System.Drawing.Point(19, 86);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(156, 29);
            this.label1.TabIndex = 2;
            this.label1.Text = "File Ext -> Language     eg: py --> Python";
            // 
            // btnEditTemplate
            // 
            this.btnEditTemplate.Enabled = false;
            this.btnEditTemplate.Location = new System.Drawing.Point(21, 51);
            this.btnEditTemplate.Name = "btnEditTemplate";
            this.btnEditTemplate.Size = new System.Drawing.Size(138, 23);
            this.btnEditTemplate.TabIndex = 1;
            this.btnEditTemplate.Text = "Edit Templates";
            this.btnEditTemplate.UseVisualStyleBackColor = true;
            this.btnEditTemplate.Click += new System.EventHandler(this.btnEditTemplate_Click);
            // 
            // chkEnableTemplate
            // 
            this.chkEnableTemplate.AutoSize = true;
            this.chkEnableTemplate.Location = new System.Drawing.Point(21, 23);
            this.chkEnableTemplate.Name = "chkEnableTemplate";
            this.chkEnableTemplate.Size = new System.Drawing.Size(138, 16);
            this.chkEnableTemplate.TabIndex = 0;
            this.chkEnableTemplate.Text = "Auto Apply Template";
            this.chkEnableTemplate.UseVisualStyleBackColor = true;
            this.chkEnableTemplate.CheckedChanged += new System.EventHandler(this.chkEnableTemplate_CheckedChanged);
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(219, 326);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(75, 28);
            this.btnSave.TabIndex = 2;
            this.btnSave.Text = "Save";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(300, 326);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 28);
            this.btnCancel.TabIndex = 3;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.txtExt);
            this.groupBox3.Controls.Add(this.btnDelete);
            this.groupBox3.Controls.Add(this.btnAdd);
            this.groupBox3.Controls.Add(this.lboxExtList);
            this.groupBox3.Controls.Add(this.lboxLangList);
            this.groupBox3.Location = new System.Drawing.Point(354, 14);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(231, 306);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Language Mapping";
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(133, 268);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(75, 23);
            this.btnDelete.TabIndex = 3;
            this.btnDelete.Text = "Delete";
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(133, 241);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 2;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // lboxExtList
            // 
            this.lboxExtList.FormattingEnabled = true;
            this.lboxExtList.ItemHeight = 12;
            this.lboxExtList.Location = new System.Drawing.Point(133, 23);
            this.lboxExtList.Name = "lboxExtList";
            this.lboxExtList.Size = new System.Drawing.Size(81, 184);
            this.lboxExtList.TabIndex = 1;
            // 
            // lboxLangList
            // 
            this.lboxLangList.FormattingEnabled = true;
            this.lboxLangList.ItemHeight = 12;
            this.lboxLangList.Location = new System.Drawing.Point(6, 23);
            this.lboxLangList.Name = "lboxLangList";
            this.lboxLangList.Size = new System.Drawing.Size(120, 268);
            this.lboxLangList.TabIndex = 0;
            this.lboxLangList.SelectedIndexChanged += new System.EventHandler(this.lboxLangList_SelectedIndexChanged);
            // 
            // txtExt
            // 
            this.txtExt.Location = new System.Drawing.Point(133, 214);
            this.txtExt.Name = "txtExt";
            this.txtExt.Size = new System.Drawing.Size(75, 21);
            this.txtExt.TabIndex = 4;
            // 
            // frmSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(597, 381);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "frmSettings";
            this.ShowInTaskbar = false;
            this.Text = "NppPIALexer2 Settings";
            this.Load += new System.EventHandler(this.frmSettings_Load);
            this.VisibleChanged += new System.EventHandler(this.frmSettings_VisibleChanged);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmSettings_FormClosing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckedListBox cboxLangList;
        private System.Windows.Forms.CheckBox chkEnableAutoC;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnEditTemplate;
        private System.Windows.Forms.CheckBox chkEnableTemplate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.ListBox lboxExtList;
        private System.Windows.Forms.ListBox lboxLangList;
        private System.Windows.Forms.TextBox txtExt;
    }
}