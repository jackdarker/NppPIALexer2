using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace NppPIALexer2.Forms
{
    public partial class frmTaskDefinition : Form
    {
        public frmTaskDefinition()
        {
            InitializeComponent();

            _BindTaskDefinition();
        }

        void _BindTaskDefinition()
        {
            lboxTaskDefList.Items.Clear();
            foreach (var def in TaskDefinition.Defs)
                lboxTaskDefList.Items.Add(def.Name);
        }

        void _RegreshTags()
        {
            foreach (Project proj in ProjectManager.Projects)
                TaskUpdater.Update(proj.Root.SubFiles2.ToArray());
            Main.FrmTaskList.BindTasks();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (btnAdd.Text == "Add")
            {
                string name = txtName.Text.Trim();
                string regex = txtRegex.Text.Trim();
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(regex))
                {
                    Utility.Error("Task name and regex can't be empty.");
                    return;
                }

                if (TaskDefinition.Get(name.ToLower()) != null)
                {
                    Utility.Error("Task name '{0}' exists.", name);
                    return;
                }

                TaskDefinition.Add(name, regex);
                lboxTaskDefList.Items.Add(name);
                txtName.Text = txtRegex.Text = "";
            }
            else if (btnAdd.Text == "Save")
            {
                if (txtName.Enabled)    // "todo"是内定义，只读
                {
                    string name = txtName.Text.Trim();
                    string regex = txtRegex.Text.Trim();
                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(regex))
                    {
                        Utility.Error("Task name and regex can't be empty.");
                        return;
                    }


                    string newKey = name.ToLower();
                    string oldKey = (lboxTaskDefList.SelectedItem as string).ToLower();
                    if (newKey != oldKey && TaskDefinition.Get(newKey) != null)
                    {
                        Utility.Error("Task name '{0}' exists.", name);
                        return;
                    }
                    TaskDefinition.Remove(oldKey);
                    TaskDefinition.Add(name, regex);
                    lboxTaskDefList.SelectedItem = name;
                }
                txtName.Text = txtRegex.Text = "";
                txtName.Enabled = txtRegex.Enabled = true;
                btnAdd.Text = "Add";
            }

            _RegreshTags();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (lboxTaskDefList.SelectedIndex == -1 || (lboxTaskDefList.SelectedItem as string).ToLower() == "todo")
            {
                btnRemove.Enabled = false;
                return;
            }

            string key = (lboxTaskDefList.SelectedItem as string).ToLower();
            TaskDefinition.Remove(key);
            lboxTaskDefList.Items.RemoveAt(lboxTaskDefList.SelectedIndex);
            txtName.Text = txtRegex.Text = "";
            btnAdd.Text = "Add";
            btnRemove.Enabled = lboxTaskDefList.SelectedIndex != -1;
            _RegreshTags();
        }

        private void lboxTaskDefList_DoubleClick(object sender, EventArgs e)
        {
            if (lboxTaskDefList.SelectedIndex != -1)
            {
                string key = (lboxTaskDefList.SelectedItem as string).ToLower();
                var def = TaskDefinition.Get(key);
                if (def != null)
                {
                    txtName.Text = def.Name;
                    txtRegex.Text = def.RegexString;
                    btnAdd.Text = "Save";
                    txtName.Enabled = txtRegex.Enabled = key != "todo";
                }
            }
        }

        private void lboxTaskDefList_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnRemove.Enabled = lboxTaskDefList.SelectedIndex != -1;
        }

        private void frmTaskDefinition_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
