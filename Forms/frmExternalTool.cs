using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace NppPIALexer2.Forms
{
    public partial class frmExternalTool : Form
    {
        public frmExternalTool()
        {
            InitializeComponent();
        }

        private void btnArgs_Click(object sender, EventArgs e)
        {
            ctntArgs.Show(btnArgs, -2 + btnArgs.Width, 2 - ctntArgs.Height);
        }

        private void btnSelectCmd_Click(object sender, EventArgs e)
        {
            if (dlgSelectCmd.ShowDialog() == DialogResult.OK)
            {
                txtCmd.Text = dlgSelectCmd.FileName;
            }
        }

        private void btnInitDir_Click(object sender, EventArgs e)
        {
            ctntInitDir.Show(btnInitDir, -2 + btnInitDir.Width, 2 - ctntInitDir.Height);
        }

        private void frmExternalTool_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        private void frmExternalTool_Load(object sender, EventArgs e)
        {
            foreach (object item in ctntArgs.Items)
            {
                ToolStripMenuItem i = item as ToolStripMenuItem;
                if (i == null)
                    continue;
                i.Click += new EventHandler(ctntArgsMenuItem_Click);
            }

            foreach (object item in ctntInitDir.Items)
            {
                ToolStripMenuItem i = item as ToolStripMenuItem;
                if (i == null)
                    continue;
                i.Click += new EventHandler(ctntInitDirMenuItem_Click);
            }

            BindCmdList();
        }

        void ctntArgsMenuItem_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            txtArgs.Text += item.Text;
        }

        void ctntInitDirMenuItem_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            txtInitDir.Text += item.Text;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtTitle.Text.Trim()))
            {
                Utility.Error("Title empty!");
                return;
            }
            if (string.IsNullOrEmpty(txtCmd.Text.Trim()))
            {
                Utility.Error("Command empty!");
                return;
            }

            if (btnAdd.Text == "Add")
            {
                CommandManager.AddCommand(txtTitle.Text, txtCmd.Text, txtArgs.Text, txtInitDir.Text);
                BindCmdList();
                lboxMenuItems.SelectedIndex = CommandManager.Commands.Length - 1;
                txtArgs.Text = txtCmd.Text = txtInitDir.Text = txtTitle.Text = "";
            }
            else
            {
                int index = lboxMenuItems.SelectedIndex;
                CommandManager.UpdateCommand(index, txtTitle.Text, txtCmd.Text, txtArgs.Text, txtInitDir.Text);
                BindCmdList();
                lboxMenuItems.SelectedIndex = index;
                btnAdd.Text = "Add";
                txtArgs.Text = txtCmd.Text = txtInitDir.Text = txtTitle.Text = "";
            }
        }

        /// <summary>
        /// 绑定命令列表
        /// </summary>
        void BindCmdList()
        {
            lboxMenuItems.Items.Clear();
            foreach (var cmd in CommandManager.Commands)
            {
                lboxMenuItems.Items.Add(cmd.Title);
            }
        }

        /// <summary>
        /// 选项改变的时候，按钮变成有效
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lboxMenuItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnDelete.Enabled = lboxMenuItems.SelectedIndex != -1;
            btnMoveUP.Enabled = btnMoveDown.Enabled = lboxMenuItems.Items.Count >= 2 && lboxMenuItems.SelectedIndex != -1; 
        }

        /// <summary>
        /// 删除命令
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            int sindex = lboxMenuItems.SelectedIndex;
            if (sindex == -1)
                return;
            //string title = lboxMenuItems.SelectedItem as string;
            //CommandManager.RemoveCommand(title);
            CommandManager.RemoveCommand(sindex);
            btnAdd.Text = "Add";
            BindCmdList();
            if (sindex >= 1)
                lboxMenuItems.SelectedIndex = sindex - 1;
            else
            {
                if (lboxMenuItems.Items.Count > 0)
                    lboxMenuItems.SelectedIndex = 0;
                else
                    btnDelete.Enabled = false;
            }
            if (lboxMenuItems.Items.Count < 2)
            {
                btnMoveDown.Enabled = btnMoveUP.Enabled = false;
            }

        }

        /// <summary>
        /// 双击进入编辑
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lboxMenuItems_DoubleClick(object sender, EventArgs e)
        {
            if (lboxMenuItems.SelectedIndex != -1)
            {
                var cmd = CommandManager.Commands[lboxMenuItems.SelectedIndex];
                btnAdd.Text = "Save";
                txtTitle.Text = cmd.Title;
                txtCmd.Text = cmd.Cmd;
                txtArgs.Text = cmd.Args;
                txtInitDir.Text = cmd.InitDir;
            }
        }

        private void btnMoveUP_Click(object sender, EventArgs e)
        {
            int sindex = lboxMenuItems.SelectedIndex;
            if (sindex == -1 || sindex == 0 || lboxMenuItems.Items.Count < 2)
                return;
            int frm = sindex;
            int to = sindex - 1;
            CommandManager.Swap(sindex, to);
            object temp = lboxMenuItems.Items[to];
            lboxMenuItems.Items[to] = lboxMenuItems.Items[frm];
            lboxMenuItems.Items[frm] = temp;
            lboxMenuItems.SelectedIndex = to;

            //BindCmdList();
            //lboxMenuItems.SelectedIndex = sindex - 1;
        }

        private void btnMoveDown_Click(object sender, EventArgs e)
        {
            int sindex = lboxMenuItems.SelectedIndex;
            if (sindex == -1 || sindex == lboxMenuItems.Items.Count - 1 || lboxMenuItems.Items.Count < 2)
                return;
            int frm = sindex;
            int to = sindex + 1;
            CommandManager.Swap(sindex, to);
            object temp = lboxMenuItems.Items[to];
            lboxMenuItems.Items[to] = lboxMenuItems.Items[frm];
            lboxMenuItems.Items[frm] = temp;
            lboxMenuItems.SelectedIndex = to;

        }

        private void frmExternalTool_Activated(object sender, EventArgs e)
        {
            this.Opacity = 1;
        }

        private void frmExternalTool_Deactivate(object sender, EventArgs e)
        {
            this.Opacity = 0.4;
        }
    }
}
