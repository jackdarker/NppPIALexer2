namespace NppPIALexer2.Forms
{
    partial class frmLogList
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmLogList));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tddiProjects = new System.Windows.Forms.ToolStripDropDownButton();
            this.allProjectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tddiTaskType = new System.Windows.Forms.ToolStripDropDownButton();
            this.allTasksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.lviewTaskList = new System.Windows.Forms.ListView();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tddiProjects,
            this.toolStripSeparator1,
            this.tddiTaskType,
            this.toolStripButton1,
            this.toolStripSeparator2,
            this.toolStripButton2});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.toolStrip1.Size = new System.Drawing.Size(524, 25);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "tbar";
            // 
            // tddiProjects
            // 
            this.tddiProjects.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tddiProjects.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allProjectsToolStripMenuItem});
            this.tddiProjects.Image = ((System.Drawing.Image)(resources.GetObject("tddiProjects.Image")));
            this.tddiProjects.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tddiProjects.Name = "tddiProjects";
            this.tddiProjects.Size = new System.Drawing.Size(73, 22);
            this.tddiProjects.Text = "All Projects";
            // 
            // allProjectsToolStripMenuItem
            // 
            this.allProjectsToolStripMenuItem.Name = "allProjectsToolStripMenuItem";
            this.allProjectsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.allProjectsToolStripMenuItem.Text = "All Projects";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // tddiTaskType
            // 
            this.tddiTaskType.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tddiTaskType.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allTasksToolStripMenuItem});
            this.tddiTaskType.Image = ((System.Drawing.Image)(resources.GetObject("tddiTaskType.Image")));
            this.tddiTaskType.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tddiTaskType.Name = "tddiTaskType";
            this.tddiTaskType.Size = new System.Drawing.Size(61, 22);
            this.tddiTaskType.Text = "All Tasks";
            // 
            // allTasksToolStripMenuItem
            // 
            this.allTasksToolStripMenuItem.Name = "allTasksToolStripMenuItem";
            this.allTasksToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.allTasksToolStripMenuItem.Text = "All Tasks";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::NppPIALexer2.Properties.Resources.Setting;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "Customize Task Definition";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // lviewTaskList
            // 
            this.lviewTaskList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lviewTaskList.FullRowSelect = true;
            this.lviewTaskList.GridLines = true;
            this.lviewTaskList.Location = new System.Drawing.Point(0, 25);
            this.lviewTaskList.Name = "lviewTaskList";
            this.lviewTaskList.Size = new System.Drawing.Size(524, 200);
            this.lviewTaskList.TabIndex = 1;
            this.lviewTaskList.UseCompatibleStateImageBehavior = false;
            this.lviewTaskList.View = System.Windows.Forms.View.Details;
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton2.Text = "toolStripButton2";
            this.toolStripButton2.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // frmLogList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 225);
            this.Controls.Add(this.lviewTaskList);
            this.Controls.Add(this.toolStrip1);
            this.Name = "frmLogList";
            this.Text = "frmLogList";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ListView lviewTaskList;
        private System.Windows.Forms.ToolStripDropDownButton tddiProjects;
        private System.Windows.Forms.ToolStripMenuItem allProjectsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripDropDownButton tddiTaskType;
        private System.Windows.Forms.ToolStripMenuItem allTasksToolStripMenuItem;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
    }
}