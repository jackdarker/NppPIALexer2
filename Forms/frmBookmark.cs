using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NppProject.Tag;

namespace NppProject.Forms
{
    public partial class frmBookmark : Form
    {
        delegate void UpdateBookmarkViewDelegate(string file, List<Bookmark> removed, List<Bookmark> added, List<Bookmark> updated);
        UpdateBookmarkViewDelegate _updateBookmarkView;

        public frmBookmark()
        {
            InitializeComponent();

            lviewBookmarkList.Columns.Add("Bookmark", -2, HorizontalAlignment.Left);
            lviewBookmarkList.Columns.Add("Project", -2, HorizontalAlignment.Left);
            lviewBookmarkList.Columns.Add("Line", -2, HorizontalAlignment.Left);
            lviewBookmarkList.Columns.Add("File Location", -2, HorizontalAlignment.Left);

            _updateBookmarkView = new UpdateBookmarkViewDelegate(UpdateBookmarkView);
            BookmarkUpdater.BookmarkChanged += new BookmarkChanged(BookmarkUpdater_BookmarkChanged);

            BindBookmarks();
        }

        void BookmarkUpdater_BookmarkChanged(string file, List<Bookmark> removed, List<Bookmark> added, List<Bookmark> updated)
        {
            this.Invoke(_updateBookmarkView, new object[] { file, removed, added, updated });
        }

        /// <summary>
        /// 更新视图
        /// </summary>
        /// <param name="file"></param>
        /// <param name="oldBks"></param>
        /// <param name="newBks"></param>
        void UpdateBookmarkView(string file, List<Bookmark> removed, List<Bookmark> added, List<Bookmark> updated)
        {
            if (tddiProjects.Tag == null)
                return;

            Project proj = ProjectManager.GetProjectByItsFile(file);
            if (proj == null)
                return;

            //Utility.Info("Index: {0}, Index1: {1}", ProjectManager.GetProjectIndex(proj), tcboxProjects.SelectedIndex);

            int index = (int)tddiProjects.Tag;
            if (index == -1 || index == 0 ||  ProjectManager.GetProjectIndex(proj) + 1 == index)   // 所有项目，或者当前显示的书签是属于当前项目的
            {
                if (removed != null && removed.Count > 0)
                    foreach (var bk in removed)
                        for (int i = 0; i < lviewBookmarkList.Items.Count; ++i)
                            if (bk == lviewBookmarkList.Items[i].Tag as Bookmark)
                            {
                                lviewBookmarkList.Items.RemoveAt(i);
                                break;
                            }
                if (added != null && added.Count > 0)
                    foreach (var bk in added)
                        lviewBookmarkList.Items.Insert(0, _Bk2ListViewItem(bk, proj.Name));
                if (updated != null && updated.Count > 0)
                    foreach (var bk in updated)
                        for (int i = 0; i < lviewBookmarkList.Items.Count; ++i)
                            if (bk == lviewBookmarkList.Items[i].Tag as Bookmark)
                            {
                                var item = lviewBookmarkList.Items[i];
                                item.SubItems[0].Text = string.Format(string.IsNullOrEmpty(bk.Name) ? "Bookmark" : bk.Name);
                                item.SubItems[1].Text = proj.Name;
                                item.SubItems[2].Text = (bk.LineNo + 1).ToString();
                                item.SubItems[3].Text = bk.File;
                                item.ToolTipText = bk.Line.Trim();
                                break;
                        }
            }
        }

        /// <summary>
        /// 将目标树中的所有书签绑定到ListView
        /// </summary>
        public void BindBookmarks()
        {
            // 使用ToolstripCombox有问题，当窗口作为NPP可附着窗口时，会死掉(cpu 100%)
            //lviewBookmarkList.Items.Clear();
            //tcboxProjects.Items.Clear();
            //if (ProjectManager.Projects.Length > 0)
            //{
            //    tcboxProjects.Items.Add("All Projects");
            //    foreach (Project proj in ProjectManager.Projects)
            //    {
            //        tcboxProjects.Items.Add(proj.Name);
            //        _BindBookmarks(proj);
            //    }
            //}

            lviewBookmarkList.Items.Clear();
            while (tddiProjects.DropDownItems.Count > 1)
                tddiProjects.DropDownItems.RemoveAt(1);
            foreach (Project proj in ProjectManager.Projects)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(proj.Name);
                item.Click += new EventHandler(item_Click);
                tddiProjects.DropDownItems.Add(item);
                _BindBookmarks(proj);
            }
            tddiProjects.Tag = -1;
            tddiProjects.Text = tddiProjects.DropDownItems[0].Text;
        }

        void item_Click(object sender, EventArgs e)
        {
            var src = sender as ToolStripMenuItem;
            int index = tddiProjects.DropDownItems.IndexOf(src);
            if (index < 0)
                return;
            
            tddiProjects.Text = src.Text;
            if (index == 0)
            {
                BindBookmarks();
            }
            else
            {
                Project proj = ProjectManager.Projects[index - 1];
                lviewBookmarkList.Items.Clear();
                _BindBookmarks(proj);
            }
            tddiProjects.Tag = index;
        }

        /// <summary>
        /// 绑定某个项目中的书签
        /// </summary>
        /// <param name="proj"></param>
        void _BindBookmarks(Project proj)
        {
            foreach (Bookmark bk in proj.Bookmarks)
            {
                lviewBookmarkList.Items.Add(_Bk2ListViewItem(bk, proj.Name));
            }
        }

        ListViewItem _Bk2ListViewItem(Bookmark bk, string projName)
        {
            ListViewItem item = new ListViewItem();
            item.SubItems[0].Text = string.Format(string.IsNullOrEmpty(bk.Name) ? "Bookmark" : bk.Name);
            item.SubItems.Add(projName);
            item.SubItems.Add((bk.LineNo + 1).ToString());
            item.SubItems.Add(bk.File);
            item.Tag = bk;
            item.ToolTipText = bk.Line.Trim();
            return item;
        }

        ///// <summary>
        ///// 不再使用
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //private void tcboxProjects_SelectedIndexChanged(object sender, EventArgs e)
        //{
        //    int sindex = tcboxProjects.SelectedIndex;
        //    if (sindex == -1 || sindex == 0)
        //    {
        //        BindBookmarks();
        //    }
        //    else
        //    {
        //        Project proj = ProjectManager.Projects[tcboxProjects.SelectedIndex - 1];
        //        lviewBookmarkList.Items.Clear();
        //        _BindBookmarks(proj);
        //    }
        //}

        private void lviewBookmarkList_DoubleClick(object sender, EventArgs e)
        {
            var items = lviewBookmarkList.SelectedItems;
            if (items.Count > 0)
            {
                items[0].Checked = !items[0].Checked;   // 双击不改变原checkbox状态
                var book = items[0].Tag as Bookmark;
                if (book != null)
                    NPP.GoToLine(book.File, book.LineNo);
            }
            
        }

        private void lviewBookmarkList_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            if (e.Label == null)
            {
                e.CancelEdit = true;
            }
            else
            {
                var mark = lviewBookmarkList.Items[e.Item].Tag as Bookmark;
                mark.Name = e.Label;

                var item = ProjectManager.GetProjectItemByFile(mark.File);
                if (item != null)
                    item.Project.Save();
            }
        }

        /// <summary>
        /// 删除选中的书签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbtnDelete_Click(object sender, EventArgs e)
        {
            List<Bookmark> lst = new List<Bookmark>();
            var items = lviewBookmarkList.Items;
            for (int i = items.Count - 1; i >= 0; --i)
            {
                if (items[i].Checked)
                {
                    lst.Add(items[i].Tag as Bookmark);
                    items.RemoveAt(i);
                }
            }

            string curFile = NPP.GetCurrentFile();
            foreach (var bk in lst)
            {
                if (bk.File == curFile)
                    NPP.DeleteBookmark(bk.LineNo);
                var i = ProjectManager.GetProjectItemByFile(bk.File);
                if (i != null)
                    i.Bookmarks.Remove(bk);
            }
        }

        /// <summary>
        /// 删除当前选中书签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lviewBookmarkList.SelectedItems.Count > 0)
            {
                var item = lviewBookmarkList.SelectedItems[0];
                var bk = item.Tag as Bookmark;
                if (bk.File == NPP.GetCurrentFile())
                    NPP.DeleteBookmark(bk.LineNo);
                var i = ProjectManager.GetProjectItemByFile(bk.File);
                if (i != null)
                    i.Bookmarks.Remove(bk);
                lviewBookmarkList.Items.RemoveAt(item.Index);
            }
        }

        /// <summary>
        /// 删除所有的书签
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Utility.Warn2("Delete all bookmarks?") != DialogResult.Yes)
                return;

            //删除当前页的书签
            ProjectItem item = ProjectManager.GetProjectItemByFile(NPP.GetCurrentFile());
            foreach (var bk in item.Bookmarks)
                NPP.DeleteBookmark(bk.LineNo);

            int index = (int)tddiProjects.Tag;
            if (index == -1 || index == 0)
                foreach (var proj in ProjectManager.Projects)
                    _DeleteBookmarks(proj.Root);
            else
                _DeleteBookmarks(ProjectManager.Projects[index - 1].Root);
            lviewBookmarkList.Items.Clear();
        }

        void _DeleteBookmarks(ProjectItem item)
        {
            if (item == null)
                return;
            if (item.IsDir)
            {
                foreach (var i in item.SubDirs)
                    _DeleteBookmarks(i);
                foreach (var i in item.SubFiles)
                    i.Bookmarks.Clear();
            }
            else
            {
                item.Bookmarks.Clear();
            }
        }

    }

    
}
