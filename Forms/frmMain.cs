using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NppPluginNET;
using System.IO;
using NppPIALexer2.Tag;
using System.Text.RegularExpressions;

namespace NppPIALexer2.Forms
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();

            tvProj.ImageList = Resource.ImageList;
            Resource.ProjectTreeView = tvProj;
        }

        public void BindProject(Project project)
        {
            TreeNode root = _ProjectItemToTreeNode(project.Root);
            tvProj.Nodes.Add(root);

            int projIndex = ProjectManager.GetProjectIndex(project);
            foreach (string file in project.Root.SubFiles2)
                TagUpdater.Update(projIndex, file);
            TaskUpdater.Update(project.Root.SubFiles2.ToArray());

            root.ImageIndex = root.SelectedImageIndex = Resource.Icon_Project;
            if (project.Root.isExpand)
                root.Expand();
            else
                root.Collapse();
        }

        /// <summary>
        /// 递归绑定各结点
        /// </summary>
        /// <param name="item"></param>
        TreeNode _ProjectItemToTreeNode(ProjectItem item)
        {
            TreeNode node = new TreeNode(item.Name);
            node.ToolTipText = item.AbsPath;
            node.Tag = item;
            if (item.IsDir)
            {
                if (item.isExpand)
                {
                    node.ImageIndex = node.SelectedImageIndex = Resource.Icon_FolderOpen;
                    node.Expand();
                }
                else
                {
                    node.ImageIndex = node.SelectedImageIndex = Resource.Icon_FolderClose;
                    node.Collapse();
                }

                if (item.SubDirs != null)
                    foreach (ProjectItem i in item.SubDirs)
                        node.Nodes.Add(_ProjectItemToTreeNode(i));
                if (item.SubFiles != null)
                    foreach (ProjectItem i in item.SubFiles)
                        node.Nodes.Add(_ProjectItemToTreeNode(i));
            }
            else
            {
                node.ImageIndex = node.SelectedImageIndex = Utility.GetExtIcon(Path.GetExtension(item.Name));
            }

            return node;
        }

        private void tbtnAddProj_Click(object sender, EventArgs e)
        {
            while (dlgCreateProject.ShowDialog() == DialogResult.OK)
            {
                string path = dlgCreateProject.FileName;
                if (File.Exists(path))
                {
                    Utility.Error("Project file '{0}' already exists.", Path.GetFileNameWithoutExtension(path));
                    continue;
                }

                ProjectItem item = new ProjectItem(Path.GetFileNameWithoutExtension(path), true);
                Project proj = new Project(path, item);
                item.Project = proj;
                ProjectManager.AddProject(proj);
                BindProject(proj);
                if (Main.FrmTagList != null)
                    Main.FrmTagList.RefreshClassView();
                if (Main.FrmTaskList != null)
                    Main.FrmTaskList.BindTasks();
                break;
            }
        }

        

        /// <summary>
        /// 加载项目
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbtnLoadProj_Click(object sender, EventArgs e)
        {
            if (dlgOpenProject.ShowDialog() == DialogResult.OK)
            {
                foreach (string path in dlgOpenProject.FileNames)
                {
                    bool gotoNext = false;
                    foreach (Project proj in ProjectManager.Projects)
                    {
                        if (proj.ProjectFile.ToLower() == path.ToLower())
                        {
                            Utility.Warn("'{0}' already opened.", path);
                            gotoNext = true;
                            break;
                        }
                    }
                    if (gotoNext)
                        continue;

                    try
                    {
                        Project proj = Project.Load(path);
                        ProjectManager.AddProject(proj);
                        BindProject(proj);
                        if (Main.FrmTagList != null)
                            Main.FrmTagList.RefreshClassView();
                        //if (Main.FrmBookmark != null)
                        //    Main.FrmBookmark.BindBookmarks();
                        if (Main.FrmTaskList != null)
                            Main.FrmTaskList.BindTasks();
                    }
                    catch
                    {
                        Utility.Error("Invalid project file '{0}'", path);
                    }
                }
            }
        }

        /// <summary>
        /// 打开项目
        /// </summary>
        /// <param name="file">项目文件路径</param>
        public void OpenProject(string file)
        {
            foreach (Project proj in ProjectManager.Projects)
            {
                if (proj.ProjectFile.ToLower() == file.ToLower()) // 项目已经打开
                {
                    foreach (TreeNode root in tvProj.Nodes)
                        if ((root.Tag as ProjectItem).Project == proj)
                        {
                            tvProj.Select();
                            tvProj.SelectedNode = root;
                            break;
                        }
                    return;
                }
            }

            try
            {
                Project proj = Project.Load(file);
                ProjectManager.AddProject(proj);
                BindProject(proj);
                if (Main.FrmTagList != null)
                    Main.FrmTagList.RefreshClassView();
                if (Main.FrmTaskList != null)
                    Main.FrmTaskList.BindTasks();
                
            }
            catch
            {
                Utility.Error("Invalid project file '{0}'", file);
            }
        }

        ///// <summary>
        ///// 显示项目目录树
        ///// </summary>
        ///// <param name="proj"></param>
        //public void ShowProject(Project proj)
        //{
        //    TreeNode root = _ToTreeNode(proj.Root);
        //    tvProj.Nodes.Add(root);

        //    root.ImageIndex = root.SelectedImageIndex = (int)ItemType.Project;
        //    root.Text = Path.GetFileNameWithoutExtension(root.Text);
        //}

        ///// <summary>
        ///// 递归显示项目具体信息
        ///// </summary>
        ///// <param name="item"></param>
        //TreeNode _ToTreeNode(ProjectItem item)
        //{
        //    TreeNode node = new TreeNode(item.Name);
        //    node.ToolTipText = item.AbsPath;
        //    //node.BackColor = Color.Chartreuse;
        //    //node.NodeFont = new Font(Font, FontStyle.Underline);

        //    node.Tag = item;
        //    if (item.IsDir)
        //    {

        //        if (item.isExpand)
        //        {
        //            node.ImageIndex = node.SelectedImageIndex = (int)ItemType.FolderOpen;
        //            node.Expand();
        //        }
        //        else
        //        {
        //            node.ImageIndex = node.SelectedImageIndex = (int)ItemType.FolderClose;
        //            node.Collapse();
        //        }

        //        if (item.SubDirs != null)
        //            foreach (ProjectItem i in item.SubDirs)
        //                node.Nodes.Add(_ToTreeNode(i));
        //        if (item.SubFiles != null)
        //            foreach (ProjectItem i in item.SubFiles)
        //                node.Nodes.Add(_ToTreeNode(i));
        //    }
        //    else
        //    {
        //        //node.ImageIndex = node.SelectedImageIndex = (int)ItemType.File;
        //        node.ImageIndex = node.SelectedImageIndex = Helper.GetExtIcon(Path.GetExtension(item.Name));
        //    }


        //    return node;
        //}

        private void tvProj_AfterExpand(object sender, TreeViewEventArgs e)
        {
            ProjectItem item = (ProjectItem)e.Node.Tag;
            if (item.IsDir && item.Parent != null) // 项目根结点不用更换图标
                e.Node.SelectedImageIndex = e.Node.ImageIndex = Resource.Icon_FolderOpen;
            item.isExpand = true;
        }

        private void tvProj_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            var item = (ProjectItem)e.Node.Tag;
            if (item.IsDir && item.Parent != null) // 项目根结点不用更换图标
                e.Node.SelectedImageIndex = e.Node.ImageIndex = Resource.Icon_FolderClose;
            item.isExpand = false;

        }

        /// <summary>
        /// 重命名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvProj_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (e.Label == null)  // 内容没有改变
                return;
            if ("" == e.Label)   // 内容变为空
            {
                e.CancelEdit = true;
                Utility.Error("Name can't be empty.");
                e.Node.BeginEdit();
                return;
            }

            ProjectItem item = e.Node.Tag as ProjectItem;
            ProjectItem parent = item.Parent;
            if (parent == null) // （根结点）修改项目名称
            {
                item.Name = e.Label;
                item.Project.Save();
                return;
            }
            
            // 源文件（夹）如果不存在，询问用户
            if (item.IsDir && !Directory.Exists(item.AbsPath) || !item.IsDir && !File.Exists(item.AbsPath))
            {
                if (Utility.Warn2("'{0}' not exists on disk, remove it from project?", item.Name) == DialogResult.Yes)
                {
                    Project proj = item.Project;
                    item.Parent = null;
                    proj.Save();
                    return;
                }
            }

            // 判断是否有同名文件
            ProjectItem[] children;
            if (item.IsDir)
                children = parent.SubDirs;
            else
                children = parent.SubFiles;
            if (children != null && children.Length > 0)
            {
                foreach (ProjectItem t in children)
                {
                    if (t == item)
                        continue;
                    if (t.Name.ToLower() == e.Label.Trim().ToLower())
                    {
                        e.CancelEdit = true;
                        Utility.Error("'{0}' already exists.", t.Name);
                        e.Node.BeginEdit();
                        return;
                    }
                }
            }

            //string temp = item.Name;
            //string src = item.AbsPath;
            //List<string> subFiles2 = null;
            //if (item.IsDir)
            //    subFiles2 = item.SubFiles2;
            //item.Name = e.Label.Trim();
            //string dest = item.AbsPath;

            string temp = item.Name;
            item.Name = e.Label.Trim();
            string dest = item.AbsPath;
            item.Name = temp;

            if (!item.IsDir && File.Exists(dest) || item.IsDir && Directory.Exists(dest))
            {
                e.CancelEdit = true;
                Utility.Error("'{0}' exists on disk.", e.Label);
                e.Node.BeginEdit();
                return;
            }
            try
            {
                int index = ProjectManager.GetProjectIndex(item.Project);
                Utility.Assert(index != -1);

                // 关闭所有已打开的子文件
                if (item.IsDir)
                {
                    // 文件夹改名前，文件夹内已打开的文件全部关闭
                    foreach (string file in NPP.FilterOpenedFiles(item.SubFiles2))
                    {
                        NPP.CloseFile(file);
                        Utility.UnUnderlineTreeNode(file);
                    }

                    if (Directory.Exists(item.AbsPath))
                        Directory.Move(item.AbsPath, dest);
                    else
                        Directory.CreateDirectory(dest);

                    // 更新ClassView
                    string[] subFiles = item.SubFiles2.ToArray();
                    TagUpdater.Remove(index, subFiles);
                    //BookmarkUpdater.Remove(subFiles);
                    TaskUpdater.Remove(subFiles);

                    item.Name = e.Label.Trim();
                    foreach (string file in item.SubFiles2)
                    {
                        TagUpdater.Update(index, file);
                        TaskUpdater.Update(file);
                        //BookmarkUpdater.Update(file);
                    }
                }
                else
                {
                    bool isOpened = NPP.IsOpened(item.AbsPath);
                    if (isOpened)
                        NPP.CloseFile(item.AbsPath);

                    if (!Utility.TryApplyTemplate(item.AbsPath, dest, item.Project))
                    {
                        if (File.Exists(item.AbsPath))
                            File.Move(item.AbsPath, dest);
                        else
                            File.Create(dest).Close();
                    }
                    TagUpdater.Remove(index, item.AbsPath);
                    //BookmarkUpdater.Remove(item.AbsPath);
                    TaskUpdater.Remove(item.AbsPath);
                    item.Name = e.Label.Trim();
                    TagUpdater.Update(index, item.AbsPath);
                    TaskUpdater.Update(item.AbsPath);
                    //BookmarkUpdater.Update(item.AbsPath);
                    
                    if (isOpened)   // reopen this file
                        NPP.OpenFile(item.AbsPath);
                    e.Node.SelectedImageIndex = e.Node.ImageIndex = Utility.GetExtIcon(Path.GetExtension(dest));
                }

                _UpdateTreeNodeTooltip(e.Node);
                item.Project.Save();

            }
            catch (Exception ex)
            {
                item.Name = temp;
                e.CancelEdit = true;
                Utility.Error("Rename '{0}' failed: {1}, {2}", temp, ex.Message, ex.StackTrace);
                e.Node.BeginEdit();
            }
        }

        /// <summary>
        /// 右键单键，显示菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvProj_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                tvProj.ContextMenuStrip = null;
                TreeNode selectedNode = tvProj.GetNodeAt(e.X, e.Y);
                tvProj.SelectedNode = selectedNode;
                ProjectItem item = selectedNode.Tag as ProjectItem;
                if (item.IsDir)
                {
                    if (selectedNode.Parent == null)
                    {
                        int itemCount = 10;    // 右键菜单默认只有9个项，其余为动态添加
                        while (ctntProject.Items.Count > itemCount)
                            ctntProject.Items.RemoveAt(itemCount);

                        foreach (Command cmd in CommandManager.Commands)
                        {
                            var addedItem = new ToolStripMenuItem(cmd.Title);
                            addedItem.Click += new EventHandler(CustomMenu_Click);
                            addedItem.Tag = cmd;
                            ctntProject.Items.Add(addedItem);
                        }
                        tvProj.ContextMenuStrip = ctntProject;
                    }
                    else
                        tvProj.ContextMenuStrip = ctntFolder;
                }
                else
                {
                    tvProj.ContextMenuStrip = ctntFile;
                }
            }
        }

        #region 右键菜单事件

        /// <summary>
        /// 自定义命令工具
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomMenu_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            var cmd = item.Tag as Command;
            try
            {
                cmd.Execute();
            }
            catch (Exception ex)
            {
                Utility.Error("Execute external tool '{0}' failed: {1}", cmd.Title, ex.Message);
            }
        }


        /// <summary>
        /// 新建文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddNewItem_Click(object sender, EventArgs e)
        {
            ProjectItem item = tvProj.SelectedNode.Tag as ProjectItem;
            ProjectItem newItem = new ProjectItem(Utility.NEW_FILE_NAME, false);
            if (!item.AppendChild(newItem))
            {
                // 如果"New File"存在, 创建"New File0"来代替
                int n = -1;
                Regex re = new Regex(string.Format(@"^{0}(\d+)$", Utility.NEW_FILE_NAME), RegexOptions.Compiled | RegexOptions.IgnoreCase);
                foreach (ProjectItem i in item.SubFiles)
                {
                    Match m = re.Match(i.Name);
                    if (m.Success)
                    {
                        int v = int.Parse(m.Groups[1].Value);
                        if (v > n)
                            n = v;
                    }
                }
                newItem.Name = string.Format("{0}{1}", newItem.Name, n + 1);
                if (!item.AppendChild(newItem))
                {
                    MessageBox.Show("Add new item failed.");
                    return;
                }
            }

            // 创建并打开文件
            File.Create(newItem.AbsPath).Close();
            NPP.OpenFile(newItem.AbsPath);

            // 添加TreeNode
            int imgIndex = Utility.GetExtIcon(Path.GetExtension(newItem.AbsPath));
            TreeNode treeNode = new TreeNode(newItem.Name, imgIndex, imgIndex);
            treeNode.Tag = newItem;
            treeNode.ToolTipText = newItem.AbsPath;
            Utility.InsertNode(tvProj.SelectedNode.Nodes, treeNode);

            if (!tvProj.SelectedNode.IsExpanded)    // 添加子结点后，展开该目录
            {
                tvProj.SelectedNode.Expand();
                item.isExpand = true;
            }
            tvProj.SelectedNode = treeNode;
            treeNode.BeginEdit();
            item.Project.Save();
        }

        /// <summary>
        /// 新建新文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddNewDir_Click(object sender, EventArgs e)
        {
            ProjectItem item = tvProj.SelectedNode.Tag as ProjectItem;
            ProjectItem newItem = new ProjectItem(Utility.NEW_FOLDER_NAME, true);
            if (!item.AppendChild(newItem))
            {
                // 如果文件夹"New Folder"存在, 创建"New Folder0"
                int n = -1;
                Regex re = new Regex(string.Format(@"^{0}(\d+)$", Utility.NEW_FOLDER_NAME), RegexOptions.Compiled | RegexOptions.IgnoreCase);
                foreach (ProjectItem i in item.SubDirs)
                {
                    Match m = re.Match(i.Name);
                    if (m.Success)
                    {
                        int v = int.Parse(m.Groups[1].Value);
                        
                        if (v > n)
                            n = v;
                    }
                }
                newItem.Name = string.Format("{0}{1}", Utility.NEW_FOLDER_NAME, n + 1);
                if (!item.AppendChild(newItem))
                {
                    MessageBox.Show("Error");
                    return;
                }
            }

            // 创建文件夹
            Directory.CreateDirectory(newItem.AbsPath);

            // 添加TreeNode
            TreeNode treeNode = new TreeNode(newItem.Name, Resource.Icon_FolderClose, Resource.Icon_FolderClose);
            treeNode.Tag = newItem;
            treeNode.ToolTipText = newItem.AbsPath;
            Utility.InsertNode(tvProj.SelectedNode.Nodes, treeNode);

            if (!tvProj.SelectedNode.IsExpanded)    // 添加子结点后，展开该目录
            {
                tvProj.SelectedNode.Expand();
                item.isExpand = true;
            }
            tvProj.SelectedNode = treeNode;
            treeNode.BeginEdit();
            item.Project.Save();
        }

        /// <summary>
        /// Add an existing file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddExistingItem_Click(object sender, EventArgs e)
        {
            ProjectItem item = tvProj.SelectedNode.Tag as ProjectItem;
            dlgAddExistingItem.InitialDirectory = item.AbsPath;
            if (dlgAddExistingItem.ShowDialog() == DialogResult.OK)
            {
                // Judgment with the directory, whether there is the same name file
                foreach (string fileName in dlgAddExistingItem.FileNames)
                {
                    string t = Path.GetFileName(fileName);
                    if (item.SubFiles != null)
                        foreach (ProjectItem i in item.SubFiles)
                            if (i.Name.ToLower() == t.ToLower())
                            {
                                Utility.Error("'{0}' is already in current project.", t);
                                return;
                            }
                }

                try
                {
                    // 拷贝文件
                    foreach (string fileName in dlgAddExistingItem.FileNames)
                    {
                        string t = Path.GetFileName(fileName);
                        string dest = Path.Combine(item.AbsPath, t);
                        if (fileName.ToLower() != dest.ToLower())
                        {
                            if (!File.Exists(dest) ||
                                Utility.Warn2("'{0}' existed on disk, overwrite it?", dest) == DialogResult.Yes)
                                File.Copy(fileName, dest, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utility.Error("Copy files failed: {0}", ex.Message);
                    return;
                }

                int projIndex = ProjectManager.GetProjectIndex(item.Project);
                foreach (string fileName in dlgAddExistingItem.FileNames)
                {
                    string t = Path.GetFileName(fileName);
                    ProjectItem i = new ProjectItem(t, false);
                    item.AppendChild(i);

                    int imgType = Utility.GetExtIcon(Path.GetExtension(t));
                    TreeNode treeNode = new TreeNode(t, imgType, imgType);
                    treeNode.Tag = i;
                    treeNode.ToolTipText = i.AbsPath;
                    tvProj.SelectedNode.Nodes.Add(treeNode);
                    TagUpdater.Update(projIndex, i.AbsPath);
                    TaskUpdater.Update(i.AbsPath);
                }
                if (!tvProj.SelectedNode.IsExpanded)    // 添加子结点后，展开该目录
                {
                    tvProj.SelectedNode.Expand();
                    item.isExpand = true;
                }

                item.Project.Save();
            }
        }

        /// <summary>
        /// 添加已存在的文件夹
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddExistingFolder_Click(object sender, EventArgs e)
        {
            ProjectItem item = tvProj.SelectedNode.Tag as ProjectItem;
            item = item.Project.Root;
            if (dlgAddExistFolder.ShowDialog() != DialogResult.OK)
                return;

            string dir = dlgAddExistFolder.SelectedPath;

            // To determine whether the same name folder exists
            if (item.SubDirs != null && item.SubDirs.Length > 0)
                foreach (ProjectItem i in item.SubDirs)
                {
                    if (i.AbsPath.ToLower() == dir.ToLower())
                    {
                        Utility.Error("Folder '{0}' is already in current project.", dir);
                        return;
                    }
                }

            string destDir = dir;//Path.Combine(item.AbsPath, Utility.GetDirName(dir));
            if (Directory.Exists(destDir) && destDir.ToLower() != dir.ToLower())
            {
                Utility.Error("Folder '{0}' exists on disk.", destDir);
                return;
            }
            
            try
            {
                //if (dir.ToLower() != destDir.ToLower())
                    Utility.AddDir(dir);
            }
            catch (Exception ex)
            {
                Utility.Error("Copy directory from '{0}' to '{1}' failed: {2}", dir, destDir, ex.Message);
                try { 
                    //Utility.DeleteDir(destDir); 
                }
                catch { }
                return;
            }

            ProjectItem newItem = ProjectItem.FromDir(destDir);
            newItem.Parent = item;
            newItem.Project = item.Project;
            int projIndex = ProjectManager.GetProjectIndex(item.Project);
            foreach (string file in newItem.SubFiles2)
                TagUpdater.Update(projIndex, file);
            TaskUpdater.Update(newItem.SubFiles2.ToArray());

            TreeNode node = _ProjectItemToTreeNode(newItem);
            tvProj.SelectedNode.Nodes.Insert(0, node);
            if (!tvProj.SelectedNode.IsExpanded)    // After adding the child nodes, expand the directory
            {
                tvProj.SelectedNode.Expand();
                item.isExpand = true;
            }
            item.Project.Save();
        }

        /// <summary>
        /// 重命名
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Rename_Click(object sender, EventArgs e)
        {
            if (tvProj.SelectedNode == null)
                return;
            tvProj.SelectedNode.BeginEdit();
        }

        /// <summary>
        /// 移除Item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Remove_Click(object sender, EventArgs e)
        {
            if (tvProj.SelectedNode == null)
                return;

            ProjectItem item = tvProj.SelectedNode.Tag as ProjectItem;
            DialogResult ret;
            if (item.Parent == null)
                ret = Utility.Warn2("'{0}' will be unloaded.\n[Project: {1}]", item.Project.Root.Name, item.Project.ProjectFile);
            else if (item.IsDir)
                ret = Utility.Warn2("'{0}' will be removed.\n[Folder: {1}]", item.Name, item.AbsPath);
            else
                ret = Utility.Warn2("'{0}' will be removed.\n[File: {1}]", item.Name, item.AbsPath);

            if (ret == DialogResult.Yes)
            {
                if (item.Parent == null) // 卸载项目
                {
                    item.Project.Save();
                    ProjectManager.RemoveProject(item.Project.ProjectFile);
                    if (Main.FrmTagList != null)
                        Main.FrmTagList.RefreshClassView();
                    if (Main.FrmTaskList != null)
                        Main.FrmTaskList.BindTasks();
                }
                else
                {
                    //// 关闭已经打开的文件
                    //if (item.IsDir)
                    //    foreach (string file in item.SubFiles2)
                    //        NPP.CloseFile(file);
                    //else
                    //    NPP.CloseFile(item.AbsPath);
                    int projIndex = ProjectManager.GetProjectIndex(item.Project);
                    if (item.IsDir)
                    {
                        string[] subFiles = item.SubFiles2.ToArray();
                        TagUpdater.Remove(projIndex, subFiles);
                        TaskUpdater.Remove(subFiles);
                    }
                    else
                    {
                        TagUpdater.Remove(projIndex, item.AbsPath);
                        TaskUpdater.Remove(item.AbsPath);
                    }

                    Project proj = item.Project;
                    item.Parent.RemoveChild(item);
                    proj.Save();
                }
               
                tvProj.SelectedNode.Remove();
            }
        }

        /// <summary>
        /// 删除 文件夹或文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Delete_Click(object sender, EventArgs e)
        {
            if (tvProj.SelectedNode == null)
                return;
            ProjectItem item = tvProj.SelectedNode.Tag as ProjectItem;
            if (item.Parent == null)    // can't delete project
                return;

            DialogResult ret;
            if (item.IsDir)
                ret = Utility.Warn2("'{0}' will be deleted permanently.\n[Folder: {1}]", item.Name, item.AbsPath);
            else
                ret = Utility.Warn2("'{0}' will be deleted permanently.\n[File: {1}]", item.Name, item.AbsPath);

            if (ret == DialogResult.Yes)
            {
                try
                {
                    //// 关闭已经打开的文件
                    //if (item.IsDir)
                    //    foreach (string file in item.SubFiles2)
                    //        NPP.CloseFile(file);
                    //else
                    //    NPP.CloseFile(item.AbsPath);

                    if (item.IsDir)
                    {
                        string[] subFiles = item.SubFiles2.ToArray();
                        TaskUpdater.Remove(subFiles);
                        //BookmarkUpdater.Remove(subFiles);
                    }
                    else
                    {
                        TaskUpdater.Remove(item.AbsPath);
                        //BookmarkUpdater.Remove(item.AbsPath);
                    }
                     
                    
                    item.DeleteRelativeFiles();
                    Project proj = item.Project;
                    item.Parent.RemoveChild(item);
                    proj.Save();
                    tvProj.SelectedNode.Remove();
                }
                catch (Exception ex)
                {
                    Utility.Error("Delete '{0}' failed: {1}", item.Name, ex.Message);
                }
            }
        }

        #endregion

        /// <summary>
        /// 双击文件时，在npp中打开
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvProj_DoubleClick(object sender, EventArgs e)
        {
            if (tvProj.SelectedNode == null)
                return;
            ProjectItem item = tvProj.SelectedNode.Tag as ProjectItem;
            if (item == null)
                return;

            if (!item.IsDir)
            {
                if (!File.Exists(item.AbsPath))
                {
                    if (MessageBox.Show(string.Format(" '{0}' does't exist, create it?", item.Name), "Notepad++ Project", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        string dir = Path.GetDirectoryName(item.AbsPath);
                        if (!Directory.Exists(dir))
                            Directory.CreateDirectory(dir);
                        File.Create(item.AbsPath).Close();
                        if (!Utility.TryApplyTemplate("", item.AbsPath, item.Project))
                            File.Create(item.AbsPath).Close();
                        NPP.OpenFile(item.AbsPath);
                    }
                    else  // delete this node
                    {
                        Project proj = item.Project;
                        item.Parent.RemoveChild(item);
                        proj.Save();
                        tvProj.Nodes.Remove(tvProj.SelectedNode);
                    }
                }
                else
                {
                    NPP.OpenFile(item.AbsPath);
                }
            }
        }

        private void frmMain_VisibleChanged(object sender, EventArgs e)
        {
            Config.Instance.Visible = Visible;
            if (Visible)
            {
                Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[Main.IdFrmMain]._cmdID, 1);
                Utility.UnderLineTreeView();
            }
            else
            {
                Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[Main.IdFrmMain]._cmdID, 0);
            }
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            Main.ShowNppSettings();
        }

        private void externalTool_Click(object sender, EventArgs e)
        {
            if (Main.FrmExternalTool == null)
            {
                Main.FrmExternalTool = new Forms.frmExternalTool();
            }
            //Main.FrmExternalTool.ShowDialog();
            Main.FrmExternalTool.Show();
        }

        /// <summary>
        /// 回车，打开选中文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvProj_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && tvProj.SelectedNode != null)
            {
                ProjectItem item = tvProj.SelectedNode.Tag as ProjectItem;
                if (item == null)
                    return;
                NPP.OpenFile(item.AbsPath);
            }
        }

        # region 拖动结点

        /// <summary>
        /// 移动文件到指定文件夹
        /// 允许跨项目移动
        /// </summary>
        /// <param name="srcNode"></param>
        /// <param name="targetNode"></param>
        /// <param name="index"></param>
        void _MoveFileToDir(TreeNode srcNode, TreeNode targetNode, int index)
        {
            ProjectItem srcItem = srcNode.Tag as ProjectItem;
            ProjectItem targetItem = targetNode.Tag as ProjectItem;
            Utility.Assert(!srcItem.IsDir && targetItem.IsDir);

            // 判断文件名是否存在
            foreach (var item in targetItem.SubFiles)
            {
                if (item.Name.ToLower() == srcItem.Name.ToLower())  // windows文件系统不区分大小写
                {
                    Utility.Error("You are attempting to move a project item to a location where a project item of the same name '{0}' already exist.", srcItem.Name);
                    return;
                }
            }

            string dest = Path.Combine(targetItem.AbsPath, srcItem.Name);
            if (File.Exists(dest))
            {
                Utility.Error("A file with the name '{0}' already exists.", dest);
                return;
            }

            bool isOpened = NPP.IsOpened(srcItem.AbsPath);
            if (isOpened)
                NPP.CloseFile(srcItem.AbsPath);
            File.Move(srcItem.AbsPath, dest);
            if (isOpened)   // reopen this file
                NPP.OpenFile(dest);

            Project frmProj = srcItem.Project;
            TagUpdater.Remove(ProjectManager.GetProjectIndex(frmProj), srcItem.AbsPath);
            TaskUpdater.Remove(srcItem.AbsPath);
            // 更新ProjectItem的层次信息
            targetItem.AppendChild(srcItem);
            Project toProj = targetItem.Project;
            TagUpdater.Update(ProjectManager.GetProjectIndex(toProj), srcItem.AbsPath);
            TaskUpdater.Update(srcItem.AbsPath);
            frmProj.Save();
            if (toProj != frmProj)
                toProj.Save();

            // update ui
            TreeNode srcNodeCpy = (TreeNode)srcNode.Clone();
            targetNode.Nodes.Insert(index, srcNodeCpy);
            srcNode.Remove();
            _UpdateTreeNodeTooltip(srcNodeCpy);
            targetNode.TreeView.SelectedNode = srcNodeCpy;
        }

        /// <summary>
        /// 移动文件夹到文件夹
        /// </summary>
        /// <param name="srcNode"></param>
        /// <param name="targetNode"></param>
        /// <param name="index"></param>
        void _MoveDirToDir(TreeNode srcNode, TreeNode targetNode, int index)
        {
            ProjectItem srcItem = srcNode.Tag as ProjectItem;
            ProjectItem targetItem = targetNode.Tag as ProjectItem;
            Utility.Assert(srcItem.IsDir && targetItem.IsDir);

            // 判断文件夹名是否存在
            foreach (var item in targetItem.SubDirs)
            {
                if (item.Name.ToLower() == srcItem.Name.ToLower())  // windows文件系统不区分大小写
                {
                    Utility.Error("Cannot move the folder '{0}'. A folder with that name already exists in the destination directory.", srcItem.Name);
                    return;
                }
            }
            string dest = Path.Combine(targetItem.AbsPath, srcItem.Name);
            if (Directory.Exists(dest))
            {
                Utility.Error("Cannot move the folder '{0}'. A folder with that name already exists in the destination directory.", srcItem.Name);
                return;
            }
            // 文件夹移动前，文件夹内已打开的文件全部关闭
            foreach (string file in NPP.FilterOpenedFiles(srcItem.SubFiles2))
            {
                NPP.CloseFile(file);
                Utility.UnUnderlineTreeNode(file);
            }
            try
            {
                Utility.MoveDir(srcItem.AbsPath, targetItem.AbsPath);
            }
            catch (Exception ex)
            {
                Utility.Error("Move folder from '{0}' to '{1'} failed: {2}", srcItem.AbsPath, targetItem.AbsPath, ex.Message);
                return;
            }

            Project frmProject = srcItem.Project;
            string[] subFiles = srcItem.SubFiles2.ToArray();
            TagUpdater.Remove(ProjectManager.GetProjectIndex(frmProject), subFiles);
            TaskUpdater.Remove(subFiles);
            // 更新ProjectItem的层次信息
            if (!targetItem.AppendChild(srcItem))
            {
                Utility.Error("Failed");
                return;
            }
            Project toProject = targetItem.Project;
            int projIndex = ProjectManager.GetProjectIndex(toProject);
            foreach (string file in srcItem.SubFiles2)
                TagUpdater.Update(projIndex, file);
            TaskUpdater.Update(srcItem.SubFiles2.ToArray());

            frmProject.Save();
            if (toProject != frmProject)
                toProject.Save();

            // update ui
            TreeNode srcNodeCpy = (TreeNode)srcNode.Clone();
            targetNode.Nodes.Insert(index, srcNodeCpy);
            srcNode.Remove();
            _UpdateTreeNodeTooltip(srcNodeCpy);
            targetNode.TreeView.SelectedNode = srcNodeCpy;
        }

        void _UpdateTreeNodeTooltip(TreeNode node)
        {
            node.ToolTipText = (node.Tag as ProjectItem).AbsPath;
            if (node.Nodes.Count > 0)
                foreach (TreeNode n in node.Nodes)
                    _UpdateTreeNodeTooltip(n);
        }

        /// <summary>
        /// 释放拖动的结点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvProj_DragDrop(object sender, DragEventArgs e)
        {
            if (_LastDropOverTreeNode != null)
            {
                _LastDropOverTreeNode.BackColor = Color.White;
                _LastDropOverTreeNode.ForeColor = Color.Black;
            }

            TreeNode srcNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
            Point pt = ((TreeView)(sender)).PointToClient(new Point(e.X, e.Y));
            TreeNode targetNode = tvProj.GetNodeAt(pt);
            if (srcNode == targetNode)
                return;

            ProjectItem srcItem = srcNode.Tag as ProjectItem;
            ProjectItem targetItem = targetNode.Tag as ProjectItem;

            if (srcItem.IsDir)
            {
                if (srcItem.Parent == targetItem.Parent)  // 同层目录里移动整个文件夹
                {
                    if (targetItem.IsDir)  // 文件夹到文件夹
                        _MoveDirToDir(srcNode, targetNode, 0);
                    else
                    {
                        TreeNode srcNodeCpy = (TreeNode)srcNode.Clone();
                        targetNode.Parent.Nodes.Insert(targetNode.Index, srcNodeCpy);
                        srcNode.Remove();
                        srcNodeCpy.TreeView.SelectedNode = srcNodeCpy;
                    }
                }
                else // 不同的目录之间移动文件夹
                {
                    // 检查targetItem是否是srcItem的子结点
                    ProjectItem item = targetItem.Parent;
                    while (item != null)
                    {
                        if (item == srcItem)
                        {
                            Utility.Error("Cannot move '{0}'. The destination folder is a subfolder of the source folder.", srcItem.Name);
                            return;
                        }
                        item = item.Parent;
                    }
                    if (targetItem.IsDir)
                        _MoveDirToDir(srcNode, targetNode, 0);
                    else
                        _MoveDirToDir(srcNode, targetNode.Parent, targetNode.Index);
                }
            }
            else  // 移动单个文件
            {
                if (srcItem.Parent == targetItem.Parent)    // 在同一目录内
                {
                    if (targetItem.IsDir)  // eg: d:\file.txt -> d:\dir
                        _MoveFileToDir(srcNode, targetNode, 0);
                    else
                    {
                        TreeNode srcNodeCpy = (TreeNode)srcNode.Clone();
                        targetNode.Parent.Nodes.Insert(targetNode.Index, srcNodeCpy);
                        srcNode.Remove();
                        srcNodeCpy.TreeView.SelectedNode = srcNodeCpy;
                    }
                }
                else  // 在不同的目录里
                {
                    if (targetItem.IsDir)  // 目标是文件夹，则移动到文件夹内. eg: d:\file.txt -> d:\dir\subdir\
                        _MoveFileToDir(srcNode, targetNode, 0);
                    else  // eg: d:\file.txt -> d:\dir\subdir\dest.txt
                        _MoveFileToDir(srcNode, targetNode.Parent, targetNode.Index);
                }
                srcItem.Project.Save();
            }
        }

        /// <summary>
        /// 检验拖放的数据是否适用于目标控件,即是否为TreeNode对象
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvProj_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode"))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        private void tvProj_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left && (tvProj.SelectedNode.Tag as ProjectItem).Parent != null) // 不能移动项目结点
            {
                DoDragDrop(e.Item, DragDropEffects.Move);
            }
        }

        TreeNode _LastDropOverTreeNode = null;
        /// <summary>
        /// 拖动结点时，移动其他结点上时，其他结点的状态变化
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvProj_DragOver(object sender, DragEventArgs e)
        {
            Point pt = ((TreeView)(sender)).PointToClient(new Point(e.X, e.Y));
            TreeNode overNode = tvProj.GetNodeAt(pt);
            if (overNode == null)
                return;
            if (overNode == _LastDropOverTreeNode)
                return;

            if (_LastDropOverTreeNode != null)
            {
                _LastDropOverTreeNode.BackColor = Color.White;
                _LastDropOverTreeNode.ForeColor = Color.Black;
            }
            overNode.BackColor = Color.DarkBlue;
            overNode.ForeColor = Color.White;
            _LastDropOverTreeNode = overNode;
        }

        #endregion

        private void tbtnShowTagDlg_Click(object sender, EventArgs e)
        {
            Main.ShowNppTagList();
        }

        private void tbtnRefresh_Click(object sender, EventArgs e)
        {
            tvProj.Nodes.Clear();
            // 加载项目
            foreach (Project proj in ProjectManager.Projects)
                BindProject(proj);
            Utility.UnderLineTreeView();
            Utility.HighlightActiveTreeNode(NPP.GetCurrentFile());
        }

        private void tbtnBookmark_Click(object sender, EventArgs e)
        {
            //Main.ShowNppBookmark();
        }

        /// <summary>
        /// 进入定义处
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GotoDefinition_Click(object sender, EventArgs e)
        {
            Main.GotoDefinition();
        }

        private void jumpBackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Main.JumpBack();
        }

        private void jumpForwardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Main.JumpForward();
        }

        private void clearJumpListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Jump.ClearList();
            while (jumpListToolStripMenuItem.DropDownItems.Count > 2)
                jumpListToolStripMenuItem.DropDownItems.RemoveAt(2);
        }

        private void jumpToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            while (jumpListToolStripMenuItem.DropDownItems.Count > 2)
                jumpListToolStripMenuItem.DropDownItems.RemoveAt(2);
            foreach (Jump j in  Jump.JumpList)
            {
                ToolStripMenuItem item = new ToolStripMenuItem(j.ToString());
                item.Tag = j;
                item.Click += new EventHandler(delegate(object src, EventArgs ex) 
                {
                    Jump jmp = (src as ToolStripMenuItem).Tag as Jump;
                    Jump.Cursor = jmp;
                    NPP.GoToDefinition(jmp.File, jmp.LineNo, jmp.Info);
                });
                if (j == Jump.Cursor)
                    item.Image = Properties.Resources.Cursor;
                jumpListToolStripMenuItem.DropDownItems.Add(item);
            }
        }

        private void fileSwitcherToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Main.ShowFileSwitcher();
        }

        private void autoCompletionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Main.ShowAutoCompletion();
        }

        private void taskListToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Main.ShowNppTaskList();
        }

        private void tbtnGenerateProject_Click(object sender, EventArgs e)
        {
            if (dlgGeneratePeoj.ShowDialog() == DialogResult.OK)
            {
                string dir = dlgGeneratePeoj.SelectedPath;
                string dirName = Utility.GetDirName(dir);
                string path = Path.Combine(dir, dirName + ".nppproj");
                if (File.Exists(path) && Utility.Warn2("Project file '{0}' exists, overwrite it?", path) != DialogResult.Yes)
                    return;
                // 项目是否已经打开
                foreach (var proj in ProjectManager.Projects)
                    if (proj.ProjectFile == path)
                    {
                        foreach (TreeNode rootNode in tvProj.Nodes)
                            if ((rootNode.Tag as ProjectItem).Project == proj)
                            {
                                rootNode.Remove();
                                break;
                            }
                        ProjectManager.RemoveProject(path);
                        break;
                    }
                ProjectItem item = ProjectItem.FromDir(dir);
                Project project = new Project(path, item);
                item.Project = project;
                project.Save();
                ProjectManager.AddProject(project);
                BindProject(project);
                if (Main.FrmTagList != null)
                    Main.FrmTagList.RefreshClassView();
            }
        }

        private void unloadAllProjectsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (int i = tvProj.Nodes.Count - 1; i >= 0; --i)
            //foreach (TreeNode root in tvProj.Nodes)
            {
                var root = tvProj.Nodes[i];
                var rootItem = root.Tag as ProjectItem;
                rootItem.Project.Save();
                ProjectManager.RemoveProject(rootItem.Project);
                root.Remove();
            }
            if (Main.FrmTagList != null)
                Main.FrmTagList.RefreshClassView();
            if (Main.FrmTaskList != null)
                Main.FrmTaskList.BindTasks();

        }

        private void tbtnToggleFold_Click(object sender, EventArgs e) {
            if (tvProj.SelectedNode == null)
                return;
            ProjectItem item = tvProj.SelectedNode.Tag as ProjectItem;
            item.Project.Model.RebuildAll();
        }
    }
}
