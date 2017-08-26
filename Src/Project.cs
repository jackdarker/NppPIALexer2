using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Windows.Forms;
namespace NppPIALexer2
{
    /// <summary>
    /// 
    /// </summary>
    public class ProjectItem
    {
        string _name;
        bool _isDir = false;
        bool _isExpand = false;
        List<ProjectItem> _subDirs = null;
        List<ProjectItem> _subFiles = null;
        ProjectItem _parent = null;
        Project _project = null;

        public ProjectItem(string name, bool isDir, ProjectItem parent)
        {
            if (string.IsNullOrEmpty(name.Trim()))
            {
                if (isDir)
                    throw new Exception("Folder name can't be empty.");
                else
                    throw new Exception("File name can't be empty.");
            }
            _subDirs = new List<ProjectItem>();
            _subFiles = new List<ProjectItem>();
            _name = name.Trim();
            _isDir = isDir;
            Parent = parent;
        }

        public ProjectItem(string name, bool isDir)
            : this(name, isDir, null)
        { }


        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public bool IsDir
        {
            get { return _isDir; }
        }

        public bool isExpand
        {
            get { return _isExpand; }
            set { _isExpand = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public ProjectItem Parent
        {
            get { return _parent; }
            set
            {
                if (_parent == value)
                    return;

                if (value == null)  
                {
                    if (_parent != null)
                        if (IsDir)
                        {
                            _parent._subDirs.Remove(this);
                        }
                        else
                        {
                            _parent._subFiles.Remove(this);
                        }

                    var tmp = _parent._subFiles;
                    _parent = null;
                    Project = null;
                }
                else
                {
                    // Check if the name exists
                    List<ProjectItem> container = IsDir ? value._subDirs : value._subFiles;
                    foreach (ProjectItem i in container)
                        if (i.Name.ToLower() == Name.ToLower())
                            throw new Exception(string.Format("'{0}' exists in parent item.", Name));

                    if (_parent != null)
                        if (IsDir)
                            _parent._subDirs.Remove(this);
                        else
                            _parent._subFiles.Remove(this);
                    container.Add(this);
                    _parent = value;
                    Project = value.Project;
                    container.Sort(new Comparison<ProjectItem>(
                        delegate(ProjectItem a, ProjectItem b)
                        {
                            return string.Compare(a.Name, b.Name);
                        }));
                }
            }
        }

        /// <summary>
        /// Gets or sets the items to which the node belongs. When set, all child nodes of the Project property will be set at the same time
        /// </summary>
        public Project Project
        {
            get { return _project; }
            set 
            { 
                _project = value;
                if (IsDir)
                {
                    foreach (var item in SubDirs)
                        item.Project = value;
                    foreach (var item in SubFiles)
                        item.Project = value;
                }
            }
        }

        /// <summary>
        /// Read-only, only through the AppendXXX to add child nodes
        /// </summary>
        public ProjectItem[] SubDirs
        {
            get { return _subDirs.ToArray(); }
        }

        public ProjectItem[] SubFiles
        {
            get { return _subFiles.ToArray(); }
        }
        
        /// <summary>
        /// Get all the subfiles (recursive)
        /// </summary>
        public List<string> SubFiles2
        {
            get
            {
                if (IsDir)
                {
                    List<string> ret = new List<string>();
                    _GetFiles(this, ret);
                    return ret;
                }
                else
                    throw new Exception("File item havenot sub files.");

            }
        }

        void _GetFiles(ProjectItem item, List<string> lst)
        {
            foreach (ProjectItem i in item.SubFiles)
                lst.Add(i.AbsPath);
            foreach (var i in item.SubDirs)
                _GetFiles(i, lst);
        }

        /// <summary>
        /// Relative to the path of the project file
        /// </summary>
        public string RelativePath
        {
            get
            {
                string path = "";
                ProjectItem i = this;
                while (i.Parent != null)
                {
                    if (path == "")
                        path = i.Name;
                    else
                        path = Path.Combine(i.Name, path);
                    i = i.Parent;
                }
                return path;
            }
        }

        public string AbsPath
        {
            get { return Path.Combine(Project.BaseDir, RelativePath); }
        }

        /// <summary>
        /// Add a project node
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool AppendChild(ProjectItem item)
        {
            Utility.Assert(item != null);
            try
            {
                item.Parent = this;
                return true;
            }
            catch (Exception ex)
            {
                Utility.Debug("Append Child Failed: {0}, {1}", ex.Message, ex.StackTrace);
                return false;
            }
        }

        public void RemoveChild(ProjectItem item)
        {
            Utility.Assert(item.Parent == this);
            item.Parent = null;
        }

        /// <summary>
        /// Delete the corresponding file (folder)
        /// </summary>
        public void DeleteRelativeFiles()
        {
            if (IsDir && Directory.Exists(AbsPath))
                Utility.DeleteDir(AbsPath);
            else if (!IsDir && File.Exists(AbsPath))
                File.Delete(AbsPath);
        }

        public static ProjectItem FromDir(string dir)
        {
            if (!Directory.Exists(dir))
                return null;
            return _FromDir(dir, null);
        }

        static ProjectItem _FromDir(string dir, ProjectItem parent)
        {
            //Todo if adding sub-sub-dir create parent-node for subdir
            ProjectItem item = new ProjectItem(Utility.GetDirName(dir), true, parent);
            foreach (string subDir in Directory.GetDirectories(dir))
                _FromDir(subDir, item);
            foreach (string subFile in Directory.GetFiles(dir))
            {
                if (Path.GetExtension(subFile).ToLower() == ".nppproj")
                    continue;
                string fileName = Path.GetFileName(subFile);
                ProjectItem item1 = new ProjectItem(fileName, false, item);
            }
            return item;
        }
    }

    public class Project
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="projFile"></param>
        /// <param name="root"></param>
        public Project(string projFile, ProjectItem root)
        {
            ProjectFile = projFile;
            Root = root;
            m_Model = new ModelDocument(projFile);
        }
        private ModelDocument m_Model;
        public ModelDocument Model {
            get{return m_Model;}
        }

        public string ProjectFile
        {
            get;
            set;
        }

        public ProjectItem Root
        {
            get;
            set;
        }

        public string Name
        {
            get { return Root.Name; }
        }

        //public List<Bookmark> Bookmarks
        //{
        //    get
        //    {
        //        List<Bookmark> lst = new List<Bookmark>();
        //        _GetBookmarks(Root, lst);
        //        return lst;
        //    }
        //}

        //void _GetBookmarks(ProjectItem item, List<Bookmark> lst)
        //{
        //    if (item == null)
        //        return;
        //    if (item.IsDir)
        //    {
        //        foreach (ProjectItem i in item.SubDirs)
        //            _GetBookmarks(i, lst);
        //        foreach (ProjectItem i in item.SubFiles)
        //            if (i.Bookmarks != null && i.Bookmarks.Count > 0)
        //                lst.AddRange(i.Bookmarks);
        //    }
        //    else
        //    {
        //        if (item.Bookmarks != null && item.Bookmarks.Count > 0)
        //            lst.AddRange(item.Bookmarks);
        //    }
        //}

        public string BaseDir
        {
            get { return Path.GetDirectoryName(ProjectFile); }
        }

        /// <summary>
        /// 保存到项目文件
        /// </summary>
        public void Save()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement proj = doc.CreateElement("project");
            proj.SetAttribute("author", "");
            doc.AppendChild(proj);
            if (Root != null)
                proj.AppendChild(_ToXmlElement(doc, Root));
            doc.Save(ProjectFile);
        }

        /// <summary>
        /// ProjectItem -> XmlElement
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        XmlElement _ToXmlElement(XmlDocument doc, ProjectItem item)
        {
            XmlElement node = doc.CreateElement("node");
            node.SetAttribute("name", item.Name);
            node.SetAttribute("isDir", item.IsDir.ToString());
            node.SetAttribute("isExpand", item.isExpand.ToString());
            if (item.IsDir)
            {
                foreach (ProjectItem i in item.SubDirs)
                    node.AppendChild(_ToXmlElement(doc, i));
                foreach (ProjectItem i in item.SubFiles)
                    node.AppendChild(_ToXmlElement(doc, i));
            }
            //else
            //{
            //    if (item.Bookmarks != null && item.Bookmarks.Count > 0)
            //    {
            //        foreach (var bk in item.Bookmarks)
            //        {
            //            var bmark = doc.CreateElement("bookmark");
            //            bmark.SetAttribute("name", bk.Name);
            //            bmark.SetAttribute("lineno", bk.LineNo.ToString());
            //            bmark.SetAttribute("line", bk.Line);
            //            node.AppendChild(bmark);
            //        }
            //    }
            //}
            return node;
        }

        //XmlElement _CreateBookmarkElement(XmlDocument doc)
        //{
        //    XmlElement bmarks = doc.CreateElement("bookmarks");
        //    foreach (var item in _bookmarks)
        //    {
        //        var bmark = doc.CreateElement("bookmark");
        //        bmark.SetAttribute("name", item.Name);
        //        bmark.SetAttribute("file", item.File);
        //        bmark.SetAttribute("lineno", item.LineNo.ToString());
        //        bmarks.AppendChild(bmark);
        //    }
        //    return bmarks;
        //}

        /// <summary>
        /// 文件反序列化为Project对象
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Project Load(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(path);
            var root = _GetRootProjectItem(doc);
            //var bookmarks = _GetBookmarks(doc);
            Project proj = new Project(path, root);
            root.Project = proj;
            //// root添加到Project之后，才能初始化bookmark的file属性
            //_SetBookmarkFilePath(root);
            return proj;
        }

        ///// <summary>
        ///// 设置书签所在文件为绝对路径
        ///// </summary>
        ///// <param name="item">必须是已经添加到Project中的ProjectItem</param>
        //static void _SetBookmarkFilePath(ProjectItem item)
        //{
        //    if (item.IsDir)
        //    {
        //        foreach (ProjectItem i in item.SubDirs)
        //            _SetBookmarkFilePath(i);
        //        foreach (ProjectItem i in item.SubFiles)
        //            _SetBookmarkFilePath(i);
        //    }
        //    else
        //    {
        //        if (item.Bookmarks != null && item.Bookmarks.Count > 0)
        //            foreach (Bookmark bk in item.Bookmarks)
        //                bk.File = item.AbsPath;
        //    }
        //}

        static ProjectItem _FromNode(XmlElement node)
        {
            string name = node.GetAttribute("name");
            bool isDir = bool.Parse(node.GetAttribute("isDir"));
            bool isExpand = bool.Parse(node.GetAttribute("isExpand"));
            ProjectItem item = new ProjectItem(name, isDir);
            item.isExpand = isExpand;
            if (isDir)
                foreach (XmlElement n in node.ChildNodes)
                    item.AppendChild(_FromNode(n));
            //else
            //{
            //    List<Bookmark> bkLst = new List<Bookmark>();
            //    foreach (XmlElement bkNode in node.GetElementsByTagName("bookmark"))
            //    {
            //        string bkname = bkNode.GetAttribute("name");
            //        int lineno = int.Parse(bkNode.GetAttribute("lineno"));
            //        string line = bkNode.GetAttribute("line");
            //        Bookmark bk = new Bookmark(bkname, "", lineno, line);
            //        bkLst.Add(bk);
            //    }
            //    if (bkLst.Count > 0)
            //        item.Bookmarks = bkLst;
            //}
            return item;
        }

        static ProjectItem _GetRootProjectItem(XmlDocument doc)
        {
            XmlElement projNode = doc.GetElementsByTagName("project")[0] as XmlElement;
            return _FromNode((projNode.GetElementsByTagName("node")[0] as XmlElement));
        }

        

        //static List<Bookmark> _GetBookmarks(XmlDocument doc)
        //{
        //    List<Bookmark> lst = new List<Bookmark>();
        //    var bmarks = doc.GetElementsByTagName("bookmarks")[0] as XmlElement;
        //    foreach (XmlElement node in bmarks.GetElementsByTagName("bookmark"))
        //    {
        //        string name = node.GetAttribute("name");
        //        string file = node.GetAttribute("file");
        //        int lineno = int.Parse(node.GetAttribute("lineno"));
        //        lst.Add(new Bookmark(name, file, lineno));
        //    }
        //    return lst;

        //}

        /// <summary>
        /// 判断文件是否在当前项目管理中
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public bool IsFileInProject(string file)
        {
            return GetProjectItemByFileName(file) != null;
        }

        //bool _IsInProject(string file, ProjectItem item)
        //{
        //    if (item.IsDir)
        //    {
        //        foreach (ProjectItem i in item.SubFiles)
        //            if (i.AbsPath == file)
        //                return true;
        //        foreach (ProjectItem i in item.SubDirs)
        //            if (_IsInProject(file, i))
        //                return true;
        //        return false;
        //    }
        //    else
        //        return item.AbsPath == file;
        //}

        /// <summary>
        /// 根据文件名，找到对应的结点
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public ProjectItem GetProjectItemByFileName(string file)
        {
            return _GetProjectItemByFilePath(file, Root);
        }

        ProjectItem _GetProjectItemByFilePath(string file, ProjectItem item)
        {
            if (item == null)
                return null;

            if (item.IsDir)
            {
                foreach (ProjectItem i in item.SubFiles)
                    if (i.AbsPath == file)
                        return i;
                foreach (ProjectItem i in item.SubDirs)
                {
                    ProjectItem t = _GetProjectItemByFilePath(file, i);
                    if (t != null)
                        return t;
                }
                return null;
            }
            else
                return item.AbsPath == file ? item : null;
        }
    }

    public class ProjectManager
    {
        static List<Project> _Projects = null;

        /// <summary>
        /// 所有项目
        /// </summary>
        public static Project[] Projects
        {
            get
            {
                if (_Projects == null)
                    LoadFromConfig();
                return _Projects.ToArray();
            }
        }

        public static void AddProject(Project proj)
        {
            foreach (Project p in _Projects)
                if (p.ProjectFile == proj.ProjectFile)
                {
                    Utility.Error("'{0}' already opened.", proj.ProjectFile);
                    return;
                }
            _Projects.Add(proj);
            //AutoCompletionHelper.SetUpdateFlag(proj.ProjectFile);
        }

        public static void RemoveProject(string projectFile)
        {
            //Project t = null;
            //_Projects.FindIndex
            //foreach (Project p in _Projects)
            //    if (p.ProjectFile == projectFile)
            //    {
            //        t = p;
            //        break;
            //    }
            Project t = _Projects.Find(new Predicate<Project>(delegate(Project proj)
                {
                    return proj.ProjectFile == projectFile;
                }));
            if (t == null)
            {
                Utility.Debug("'{0}' has't opened, can't close it.", projectFile);
                return;
            }
            _Projects.Remove(t);
            //AutoCompletionHelper.RemoveProjectTags(projectFile);
        }

        public static void RemoveProject(Project proj)
        {
            _Projects.Remove(proj);
        }

        /// <summary>
        /// 找到文件所在的项目
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Project GetProjectByItsFile(string file)
        {
            foreach (Project p in Projects)
                if (p.IsFileInProject(file))
                    return p;
            return null;
        }

        /// <summary>
        /// 获取文件对应的ProjectItem
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static ProjectItem GetProjectItemByFile(string file)
        {
            foreach (Project p in Projects)
            {
                var item = p.GetProjectItemByFileName(file);
                if (item != null)
                    return item;
            }
            return null;
        }

        /// <summary>
        /// 指定项目在目录树中的索引
        /// </summary>
        /// <param name="proj"></param>
        /// <returns></returns>
        public static int GetProjectIndex(Project proj)
        {
            for (int i = 0; i < Projects.Length; ++i)
                if (Projects[i] == proj)
                    return i;
            return -1;
        }

        /// <summary>
        /// 根据文件，找到其所在项目在目录树中的索引
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static int GetProjectIndex(string file)
        {
            for (int i = 0; i < Projects.Length; ++i)
                if (Projects[i].IsFileInProject(file))
                    return i;
            return -1;
        }

        /// <summary>
        /// 从配置文件中加载项目(起动的时候初始化)
        /// </summary>
        static void LoadFromConfig()
        {
            _Projects = new List<Project>();
            if (Config.Instance.Projects.Count > 0)
            {
                foreach (string projPath in Config.Instance.Projects)
                {
                    if (!File.Exists(projPath))
                    {
                        Utility.Error("Project file '{0}' not found.", projPath);
                        continue;
                    }

                    try { AddProject(Project.Load(projPath)); }
                    catch (Exception ex) { Utility.Error("Load project '{0}' failed: {1}", projPath, ex.Message); }
                }
            }
        }
    }
}