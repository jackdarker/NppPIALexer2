using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace NppPIALexer2.Forms
{
    public partial class frmTaskList : Form
    {
        delegate void UpdateTaskViewDelegate(string file, List<Task> lst);
        UpdateTaskViewDelegate _updateTaskView;

        public frmTaskList()
        {
            InitializeComponent();

            lviewTaskList.Columns.Add("Description", -2, HorizontalAlignment.Left);
            lviewTaskList.Columns.Add("Project", -2, HorizontalAlignment.Left);
            lviewTaskList.Columns.Add("File", -2, HorizontalAlignment.Left);
            lviewTaskList.Columns.Add("Line", -2, HorizontalAlignment.Left);

            _updateTaskView = new UpdateTaskViewDelegate(_UpdateTaskView);
            TaskUpdater.TaskChanged += new TaskChanged(TaskUpdater_TaskChanged);

            BindTasks();
        }

        void TaskUpdater_TaskChanged(string file, List<Task> tasks)
        {
            while (!this.IsHandleCreated)
                Thread.Sleep(1);
            this.Invoke(_updateTaskView, new object[] { file, tasks });
        }

        void _UpdateTaskView(string file, List<Task> lst)
        {
            Utility.Debug("update task view: {0}", file);
            for (int i = lviewTaskList.Items.Count - 1; i >= 0; --i)
            {
                var task = lviewTaskList.Items[i].Tag as Task;
                if (task != null && task.File == file)
                    lviewTaskList.Items.RemoveAt(i);
            }
            if (lst != null && lst.Count > 0)
            {
                Project proj = ProjectManager.GetProjectByItsFile(file);
                if (proj != null)
                    foreach (var task in lst)
                    {
                        var item = _Task2Item(proj.Name, task);
                        if (item != null)
                            lviewTaskList.Items.Insert(0, item);
                    }
            }
        }

        /// <summary>
        /// 将目标树中的所有todo绑定到列表里
        /// </summary>
        public void BindTasks()
        {
            lviewTaskList.Items.Clear();
            while (tddiProjects.DropDownItems.Count > 1)
                tddiProjects.DropDownItems.RemoveAt(1);
            while (tddiTaskType.DropDownItems.Count > 1)
                tddiTaskType.DropDownItems.RemoveAt(1);

            foreach (var def in TaskDefinition.Defs)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(def.Name);
                item.Click +=new EventHandler(taskType_Click);
                item.Tag = def;
                tddiTaskType.DropDown.Items.Add(item);
            }

            foreach (Project proj in ProjectManager.Projects)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(proj.Name);
                item.Click += new EventHandler(item_Click);
                tddiProjects.DropDownItems.Add(item);
                _BindTasks(proj);
            }
            tddiProjects.Tag = null;
            tddiProjects.Text = tddiProjects.DropDownItems[0].Text;
        }

        void _BindTasks(Project proj)
        {
            foreach (var task in TaskUpdater.GetTasks(proj.Root.SubFiles2.ToArray()))
            {
                var item = _Task2Item(proj.Name, task);
                if (item != null)
                    lviewTaskList.Items.Add(item);
            }
        }

        ListViewItem _Task2Item(string projName, Task task)
        {
            if (tddiTaskType.Tag == null || (tddiTaskType.Tag as TaskDefinition) == task.Type)
            //if (tddiTaskType.Text == "All Tasks" || task.Info.ToLower().StartsWith(tddiTaskType.Text.ToLower()))
            {
                ListViewItem item = new ListViewItem();
                item.SubItems[0].Text = task.Info;
                item.SubItems.Add(projName);
                item.SubItems.Add(task.File);
                item.SubItems.Add((task.LineNo + 1).ToString());
                item.Tag = task;
                return item;
            }
            else
                return null;
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
                BindTasks();
            }
            else
            {
                Project proj = ProjectManager.Projects[index - 1];
                lviewTaskList.Items.Clear();
                _BindTasks(proj);
            }
            tddiProjects.Tag = index;
        }

        private void lviewTaskList_DoubleClick(object sender, EventArgs e)
        {
            var items = lviewTaskList.SelectedItems;
            if (items.Count > 0)
            {
                var task = items[0].Tag as Task;
                Utility.Debug("{0} == {1}, {2} == {3}", task.LineNo, NPP.GetCurrentLine(), task.File, NPP.GetCurrentFile());
                if (task != null && (task.LineNo != NPP.GetCurrentLine() || task.File != NPP.GetCurrentFile()))
                {
                    Jump.Add(task.Type.Name, task.File, task.LineNo);
                    Jump.Cursor.Go();
                }
            }
        }

        private void taskType_Click(object sender, EventArgs e)
        {
            var src = sender as ToolStripMenuItem;
            tddiTaskType.Text = src.Text;
            tddiTaskType.Tag = src.Tag;
       
            int index = 0;
            for (; index < tddiProjects.DropDownItems.Count; ++index)   // 如果有两个名称相同的项目或者项目的名称为"All Projects"，就会有问题
            {
                var item = tddiProjects.DropDownItems[index];
                if (item.Text == tddiProjects.Text)
                    break;
            }

            if (index == 0 || index >= tddiProjects.DropDownItems.Count)
                BindTasks();
            else
            {
                lviewTaskList.Items.Clear();
                _BindTasks(ProjectManager.Projects[index - 1]);
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Main.ShowNppTaskDefinition();
        }

        private void lviewTaskList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var items = lviewTaskList.SelectedItems;
                if (items.Count > 0)
                {
                    var task = items[0].Tag as Task;
                    if (task != null && (task.LineNo != NPP.GetCurrentLine() || task.File != NPP.GetCurrentFile()))
                    {
                        Jump.Add(task.Type.Name, task.File, task.LineNo);
                        Jump.Cursor.Go();
                    }
                }
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e) {

        }
    }
}
