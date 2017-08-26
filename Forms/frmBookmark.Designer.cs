namespace NppProject.Forms
{
    partial class frmBookmark
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmBookmark));
            this.tbar = new System.Windows.Forms.ToolStrip();
            this.tddiProjects = new System.Windows.Forms.ToolStripDropDownButton();
            this.allProjectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tbtnDelete = new System.Windows.Forms.ToolStripButton();
            this.lviewBookmarkList = new System.Windows.Forms.ListView();
            this.ctntDeleteBookmark = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tbar.SuspendLayout();
            this.ctntDeleteBookmark.SuspendLayout();
            this.SuspendLayout();
            // 
            // tbar
            // 
            this.tbar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tddiProjects,
            this.tbtnDelete});
            this.tbar.Location = new System.Drawing.Point(0, 0);
            this.tbar.Name = "tbar";
            this.tbar.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.tbar.Size = new System.Drawing.Size(543, 25);
            this.tbar.TabIndex = 0;
            this.tbar.Text = "toolStrip1";
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
            this.allProjectsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.allProjectsToolStripMenuItem.Text = "All Projects";
            this.allProjectsToolStripMenuItem.Click += new System.EventHandler(this.item_Click);
            // 
            // tbtnDelete
            // 
            this.tbtnDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.tbtnDelete.Image = global::NppProject.Properties.Resources.Delete;
            this.tbtnDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tbtnDelete.Name = "tbtnDelete";
            this.tbtnDelete.Size = new System.Drawing.Size(23, 22);
            this.tbtnDelete.Text = "Delete Bookmark";
            this.tbtnDelete.Click += new System.EventHandler(this.tbtnDelete_Click);
            // 
            // lviewBookmarkList
            // 
            this.lviewBookmarkList.CheckBoxes = true;
            this.lviewBookmarkList.ContextMenuStrip = this.ctntDeleteBookmark;
            this.lviewBookmarkList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lviewBookmarkList.FullRowSelect = true;
            this.lviewBookmarkList.GridLines = true;
            this.lviewBookmarkList.LabelEdit = true;
            this.lviewBookmarkList.Location = new System.Drawing.Point(0, 25);
            this.lviewBookmarkList.MultiSelect = false;
            this.lviewBookmarkList.Name = "lviewBookmarkList";
            this.lviewBookmarkList.Size = new System.Drawing.Size(543, 205);
            this.lviewBookmarkList.TabIndex = 1;
            this.lviewBookmarkList.UseCompatibleStateImageBehavior = false;
            this.lviewBookmarkList.View = System.Windows.Forms.View.Details;
            this.lviewBookmarkList.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.lviewBookmarkList_AfterLabelEdit);
            this.lviewBookmarkList.DoubleClick += new System.EventHandler(this.lviewBookmarkList_DoubleClick);
            // 
            // ctntDeleteBookmark
            // 
            this.ctntDeleteBookmark.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteToolStripMenuItem,
            this.deleteAllToolStripMenuItem});
            this.ctntDeleteBookmark.Name = "ctntDeleteBookmark";
            this.ctntDeleteBookmark.Size = new System.Drawing.Size(131, 48);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Image = global::NppProject.Properties.Resources.Delete;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
            this.deleteToolStripMenuItem.Text = "Delete ";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // deleteAllToolStripMenuItem
            // 
            this.deleteAllToolStripMenuItem.Name = "deleteAllToolStripMenuItem";
            this.deleteAllToolStripMenuItem.Size = new System.Drawing.Size(130, 22);
            this.deleteAllToolStripMenuItem.Text = "Delete All";
            this.deleteAllToolStripMenuItem.Click += new System.EventHandler(this.deleteAllToolStripMenuItem_Click);
            // 
            // frmBookmark
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(543, 230);
            this.Controls.Add(this.lviewBookmarkList);
            this.Controls.Add(this.tbar);
            this.Name = "frmBookmark";
            this.Text = "NppProject Bookmarks";
            this.tbar.ResumeLayout(false);
            this.tbar.PerformLayout();
            this.ctntDeleteBookmark.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip tbar;
        private System.Windows.Forms.ListView lviewBookmarkList;
        private System.Windows.Forms.ToolStripButton tbtnDelete;
        private System.Windows.Forms.ContextMenuStrip ctntDeleteBookmark;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteAllToolStripMenuItem;
        private System.Windows.Forms.ToolStripDropDownButton tddiProjects;
        private System.Windows.Forms.ToolStripMenuItem allProjectsToolStripMenuItem;

    }
}