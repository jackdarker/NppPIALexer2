using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace NppPIALexer2.Forms
{
    public partial class frmFileSwitcher : Form
    {
        public frmFileSwitcher()
        {
            InitializeComponent();

            lviewFileList.Columns.Add("File Name", -2, HorizontalAlignment.Left);
            lviewFileList.Columns.Add("Project", -2, HorizontalAlignment.Left);
            lviewFileList.Columns.Add("Path", -2, HorizontalAlignment.Left);

            _LoadFileList();
            _BindData(_fileList);
        }

        List<string> _fileList = new List<string>();

        /// <summary>
        /// 加载项目文件列表
        /// </summary>
        void _LoadFileList()
        {
            lviewFileList.Items.Clear();
            _fileList.Clear();

            foreach (Project proj in ProjectManager.Projects)
                foreach (string file in proj.Root.SubFiles2)
                    _fileList.Add(file + "|" + proj.Root.Name);
        }

        void _SetSelectedItemColor(int index)
        {
            if (lviewFileList.Items.Count > 0)
            {
                foreach (ListViewItem item in lviewFileList.Items)
                {
                    item.BackColor = Color.White;
                    item.ForeColor = Color.Black;
                }

                if (0 <= index && 0 < lviewFileList.Items.Count)
                {
                    lviewFileList.Items[index].BackColor = Color.DarkSlateBlue;
                    lviewFileList.Items[index].ForeColor = Color.White;
                }
            }
        }

        int _GetSelectedItemIndex()
        {
            if (lviewFileList.Items.Count > 0)
                foreach (ListViewItem item in lviewFileList.Items)
                    if (item.BackColor == Color.DarkSlateBlue && item.ForeColor == Color.White)
                        return item.Index;
            
            return -1;
        }

        /// <summary>
        /// 绑定数据到ListView
        /// </summary>
        /// <param name="items"></param>
        void _BindData(List<string> items)
        {
            lviewFileList.Items.Clear();
            foreach (string item in items)
            {
                string[] arr = item.Split('|');
                string file = arr[0];
                string proj = arr[1];
                ListViewItem i = new ListViewItem();
                i.SubItems[0].Text = string.Format(Path.GetFileName(file));
                i.SubItems.Add(proj);
                i.SubItems.Add(file);
                lviewFileList.Items.Add(i);
            }
            if (items.Count > 0)
                _SetSelectedItemColor(0);
        }

        private void txtFileName_TextChanged(object sender, EventArgs e)
        {
            List<string> matchList = new List<string>();
            string head = txtFileName.Text.Trim();
            foreach (string item in _fileList)
            {
                string[] arr = item.Split('|');
                string fileName = Path.GetFileName(arr[0]);
                if (fileName.StartsWith(head))
                    matchList.Add(item);
            }
            _BindData(matchList);
        }

        private void lviewFileList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lviewFileList.SelectedItems.Count > 0)
            {
                NPP.OpenFile(lviewFileList.SelectedItems[0].SubItems[2].Text);
                Hide();
            }
        }

        private void frmFileSwitcher_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void lviewFileList_KeyDown(object sender, KeyEventArgs e)
        {
            _ProcessKeyDown(e.KeyCode);
        }

        void _ProcessKeyDown(Keys key)
        {
            if ((key == Keys.Enter || key == Keys.Return))
            {
                int index = _GetSelectedItemIndex();
                if (index != -1)
                {
                    NPP.OpenFile(lviewFileList.Items[index].SubItems[2].Text);
                    Hide();
                }
            }
            else if (key == Keys.Down)
            {
                int index = 0;
                foreach (ListViewItem item in lviewFileList.Items)
                {
                    if (item.BackColor == Color.DarkSlateBlue && item.ForeColor == Color.White)
                    {
                        index = item.Index + 1;
                        break;
                    }
                }
                if (index < lviewFileList.Items.Count)
                {
                    _SetSelectedItemColor(index);
                    //lviewFileList.Items[index].Selected = true;
                }
            }
            else if (key == Keys.Up)
            {
                int index = lviewFileList.Items.Count - 1;
                foreach (ListViewItem item in lviewFileList.Items)
                {
                    if (item.BackColor == Color.DarkSlateBlue && item.ForeColor == Color.White)
                    {
                        index = item.Index - 1;
                        break;
                    }
                }
                if (index >= 0)
                {
                    _SetSelectedItemColor(index);
                    //lviewFileList.Items[index].Selected = true;
                }
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            _OpenSelectedFile();
        }

        void _OpenSelectedFile()
        {
            foreach (ListViewItem item in lviewFileList.Items)
            {
                if (item.BackColor == Color.DarkSlateBlue && item.ForeColor == Color.White)
                {
                    NPP.OpenFile(item.SubItems[2].Text);
                    Hide();
                }
            }
        }

        private void lviewFileList_MouseClick(object sender, MouseEventArgs e)
        {
            if (lviewFileList.SelectedItems.Count > 0)
            {
                _SetSelectedItemColor(lviewFileList.SelectedItems[0].Index);
            }
        }

        private void frmFileSwitcher_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                _LoadFileList();
                _BindData(_fileList);
            }
        }

        private void txtFileName_KeyDown(object sender, KeyEventArgs e)
        {
            _ProcessKeyDown(e.KeyCode);
            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down)
                e.Handled = true;
        }

        private void txtFileName_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
                txtFileName.Text = "";
        }

        private void frmFileSwitcher_Deactivate(object sender, EventArgs e)
        {
            this.Opacity = 0.4;
        }

        private void frmFileSwitcher_Activated(object sender, EventArgs e)
        {
            this.Opacity = 1;
        }
    }
}
