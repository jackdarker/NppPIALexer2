namespace NppPIALexer2.Forms
{
    partial class frmTaskList
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmTaskList));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.tddiProjects = new System.Windows.Forms.ToolStripDropDownButton();
            this.allProjectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tddiTaskType = new System.Windows.Forms.ToolStripDropDownButton();
            this.allTasksToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.lviewTaskList = new System.Windows.Forms.ListView();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tddiProjects,
            this.toolStripSeparator1,
            this.tddiTaskType,
            this.toolStripButton1});
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
            this.tddiProjects.Size = new System.Drawing.Size(90, 22);
            this.tddiProjects.Text = "All Projects";
            // 
            // allProjectsToolStripMenuItem
            // 
            this.allProjectsToolStripMenuItem.Name = "allProjectsToolStripMenuItem";
            this.allProjectsToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.allProjectsToolStripMenuItem.Text = "All Projects";
            this.allProjectsToolStripMenuItem.Click += new System.EventHandler(this.item_Click);
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
            this.tddiTaskType.Size = new System.Drawing.Size(72, 22);
            this.tddiTaskType.Text = "All Tasks";
            // 
            // allTasksToolStripMenuItem
            // 
            this.allTasksToolStripMenuItem.Name = "allTasksToolStripMenuItem";
            this.allTasksToolStripMenuItem.Size = new System.Drawing.Size(124, 22);
            this.allTasksToolStripMenuItem.Text = "All Tasks";
            this.allTasksToolStripMenuItem.Click += new System.EventHandler(this.taskType_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::NppPIALexer2.Properties.Resources.Setting;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "Customize Task Definition";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // lviewTaskList
            // 
            this.lviewTaskList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lviewTaskList.FullRowSelect = true;
            this.lviewTaskList.GridLines = true;
            this.lviewTaskList.Location = new System.Drawing.Point(0, 25);
            this.lviewTaskList.Name = "lviewTaskList";
            this.lviewTaskList.Size = new System.Drawing.Size(524, 183);
            this.lviewTaskList.TabIndex = 1;
            this.lviewTaskList.UseCompatibleStateImageBehavior = false;
            this.lviewTaskList.View = System.Windows.Forms.View.Details;
            this.lviewTaskList.DoubleClick += new System.EventHandler(this.lviewTaskList_DoubleClick);
            this.lviewTaskList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.lviewTaskList_KeyDown);
            // 
            // frmTaskList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 208);
            this.Controls.Add(this.lviewTaskList);
            this.Controls.Add(this.toolStrip1);
            this.Name = "frmTaskList";
            this.Text = "frmTaskList";
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
    }
}