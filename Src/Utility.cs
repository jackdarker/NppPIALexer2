using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.IO;
using System.Drawing;
using Microsoft.Win32;
using NppPIALexer2.Tag;

namespace NppPIALexer2
{
    public class AssertException : Exception
    {
        public AssertException(string message)
            : base(message)
        { }
    }

    public class Utility
    {
        public const string NEW_FILE_NAME = "New File";
        public const string NEW_FOLDER_NAME = "New Folder";
        const string DIALOG_TITLE = "Notepad++ Project";
        
        public static void Assert(bool assert, string reason)
        {
#if DEBUG
            if (!assert)
                throw new AssertException(reason);
#endif
        }

        public static void Assert(bool assert)
        {
#if DEBUG
            Assert(assert, "");
#endif
        }

        public static void Debug(string fmt, params object[] args)
        {
#if DEBUG
            lock (typeof(Utility))
            {
                string str = string.Format(fmt, args);
                str = DateTime.Now.ToString() + "|" + str + "\n";
                string path = Path.Combine(Config.Instance.NppPIALexer2Dir, "NppPIALexer2.log");
                File.AppendAllText(path, str);
            }
#endif
        }

        public static DialogResult Error(string fmt, params object[] args)
        {
            return MessageBox.Show(string.Format(fmt, args), DIALOG_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static DialogResult Warn(string fmt, params object[] args)
        {
            return MessageBox.Show(string.Format(fmt, args), DIALOG_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static DialogResult Info(string fmt, params object[] args)
        {
            return MessageBox.Show(string.Format(fmt, args), DIALOG_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static DialogResult Warn2(string fmt, params object[] args)
        {
            return MessageBox.Show(string.Format(fmt, args), DIALOG_TITLE, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
        }

        static Dictionary<string, int> _ExtIcons = new Dictionary<string, int>();
        //static Icon _DefaultIcon = Icon.FromHandle(Resource.ImageList[Resource.Icon_File]..File.GetHicon());   // 默认图形
        /// <summary>
        /// 获取后缀对应的图标
        /// </summary>
        /// <param name="ext"></param>
        /// <returns></returns>
        public static int GetExtIcon(string ext)
        {
            int defaultIcon = Resource.Icon_File;
            ext = ext.ToLower();
            if (string.IsNullOrEmpty(ext))
                return defaultIcon;

            if (_ExtIcons.ContainsKey(ext))
                return _ExtIcons[ext];

            Icon large, small;
            _GetExtsIconAndDescription(ext, out large, out small);

            if (small != null || large != null)
            {
                Resource.ImageList.Images.Add(small == null ? large : small);
                _ExtIcons[ext] = Resource.ImageList.Images.Count - 1;
                return _ExtIcons[ext];
            }
            else
            {
                return defaultIcon;
            }
        }

        /// <summary>  
        /// 通过扩展名得到图标和描述  
        /// </summary>  
        /// <param name="ext">扩展名(如“.txt”)</param>  
        /// <param name="largeIcon">得到大图标</param>  
        /// <param name="smallIcon">得到小图标</param>  
        /// <param name="description">得到类型描述或者空字符串</param>  
        static void _GetExtsIconAndDescription(string ext, out Icon largeIcon, out Icon smallIcon)
        {
            largeIcon = smallIcon = null;
            RegistryKey extsubkey = Registry.ClassesRoot.OpenSubKey(ext);
            if (extsubkey == null)
                return;

            string extdefaultvalue = extsubkey.GetValue(null) as string;
            if (extdefaultvalue == null)
                return;

            if (extdefaultvalue.Equals("exefile", StringComparison.OrdinalIgnoreCase))  //扩展名类型是可执行文件  
            {
                System.IntPtr exefilePhiconLarge = new IntPtr();
                System.IntPtr exefilePhiconSmall = new IntPtr();
                NativeMethods.ExtractIconExW(Path.Combine(Environment.SystemDirectory, "shell32.dll"), 2, ref exefilePhiconLarge, ref exefilePhiconSmall, 1);
                if (exefilePhiconLarge.ToInt32() > 0) largeIcon = Icon.FromHandle(exefilePhiconLarge);
                if (exefilePhiconSmall.ToInt32() > 0) smallIcon = Icon.FromHandle(exefilePhiconSmall);
                return;
            }

            RegistryKey typesubkey = Registry.ClassesRoot.OpenSubKey(extdefaultvalue);  //从注册表中读取文件类型名称的相应子键  
            if (typesubkey == null)
                return;
            RegistryKey defaulticonsubkey = typesubkey.OpenSubKey("DefaultIcon");  //取默认图标子键  
            if (defaulticonsubkey == null)
                return;

            //得到图标来源字符串  
            string defaulticon = defaulticonsubkey.GetValue(null) as string; //取出默认图标来源字符串  
            if (defaulticon == null)
                return;
            string[] iconstringArray = defaulticon.Split(',');
            int nIconIndex = 0; //声明并初始化图标索引  
            if (iconstringArray.Length > 1)
                if (!int.TryParse(iconstringArray[1], out nIconIndex))
                    nIconIndex = 0;     //int.TryParse转换失败，返回0  

            //得到图标  
            System.IntPtr phiconLarge = new IntPtr();
            System.IntPtr phiconSmall = new IntPtr();
            NativeMethods.ExtractIconExW(iconstringArray[0].Trim('"'), nIconIndex, ref phiconLarge, ref phiconSmall, 1);
            if (phiconLarge.ToInt32() > 0)
                largeIcon = Icon.FromHandle(phiconLarge);
            if (phiconSmall.ToInt32() > 0)
                smallIcon = Icon.FromHandle(phiconSmall);
        }

        public partial class NativeMethods
        {

            /// Return Type: UINT->unsigned int
            ///lpszFile: LPCWSTR->WCHAR*
            ///nIconIndex: int
            ///phiconLarge: HICON*
            ///phiconSmall: HICON*
            ///nIcons: UINT->unsigned int
            [System.Runtime.InteropServices.DllImportAttribute("shell32.dll", EntryPoint = "ExtractIconExW", CallingConvention = System.Runtime.InteropServices.CallingConvention.StdCall)]
            public static extern uint ExtractIconExW([System.Runtime.InteropServices.InAttribute()] [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPWStr)] string lpszFile, int nIconIndex, ref System.IntPtr phiconLarge, ref System.IntPtr phiconSmall, uint nIcons);

        }


        /// <summary>
        /// 在项目管理树中 下划线显示所有打开的文件
        /// </summary>
        public static void UnderLineTreeView()
        {
            TreeView tv = Resource.ProjectTreeView;
            foreach (TreeNode node in tv.Nodes)
                _ClearUnderLine(node);

            foreach (string file in NPP.GetOpenedFiles())
                UnderlineTreeNode(file);
        }

        static void _ClearUnderLine(TreeNode node)
        {
            node.NodeFont = new Font(node.TreeView.Font, FontStyle.Regular);
            if (node.Nodes != null && node.Nodes.Count > 0)
            {
                foreach (TreeNode n in node.Nodes)
                    _ClearUnderLine(n);
            }
        }

        /// <summary>
        /// 结点文字添加下划线
        /// </summary>
        /// <param name="path"></param>
        public static void UnderlineTreeNode(string path)
        {
            TreeNode node = _FindNode(path);
            if (node != null)
                node.NodeFont = new Font(node.TreeView.Font, FontStyle.Underline);
        }

        static TreeNode _LastActive;
        /// <summary>
        /// 高亮显示一个结点
        /// </summary>
        /// <param name="path"></param>
        public static void HighlightActiveTreeNode(string path)
        {
            if (_LastActive != null)
                _LastActive.BackColor = Color.White;

            TreeNode node = _FindNode(path);
            if (node != null)
            {
                node.BackColor = Color.LightSkyBlue;
                node.TreeView.SelectedNode = node;
                _LastActive = node;
            }
        }

        /// <summary>
        /// 取消下划线
        /// </summary>
        /// <param name="path"></param>
        public static void UnUnderlineTreeNode(string path)
        {
            TreeNode node = _FindNode(path);
            if (node != null)
                node.NodeFont = new Font(node.TreeView.Font, FontStyle.Regular);
        }

        /// <summary>
        /// 从TreeView中查找指定结点
        /// </summary>
        /// <param name="node"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        static TreeNode _FindNode(string path)
        {
            try
            {
                TreeView tv = Resource.ProjectTreeView;
                foreach (TreeNode projNode in tv.Nodes)
                {
                    ProjectItem item = projNode.Tag as ProjectItem;
                    TreeNode ret = __FindNode(projNode, path);
                    if (ret != null)
                        return ret;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 递归查找指定结点（查找速度相对较快，比二分法慢）
        /// </summary>
        /// <param name="node"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        static TreeNode __FindNode(TreeNode node, string path)
        {
            var item = node.Tag as ProjectItem;
            if (item.AbsPath == path)
                return node;
            if (path.StartsWith(Path.GetDirectoryName(item.AbsPath)))
            {
                if (node.Nodes != null)
                {
                    TreeNode temp;
                    foreach (TreeNode n in node.Nodes)
                    {
                        temp = __FindNode(n, path);
                        if (temp != null)
                            return temp;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 按名称顺序插入到结点列表中
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="node"></param>
        public static void InsertNode(TreeNodeCollection nodes, TreeNode node)
        {
            // 先文件夹，后文件
            List<string> dirs = new List<string>();
            List<string> files = new List<string>();
            foreach (TreeNode n in nodes)
            {
                if ((n.Tag as ProjectItem).IsDir)
                    dirs.Add(n.Name);
                else
                    files.Add(n.Name);
            }

            ProjectItem item = node.Tag as ProjectItem;
            List<string> lst = item.IsDir ? dirs : files;
            int index = -1;
            for (int i = 0; i < lst.Count; ++i)
            {
                int compare = string.Compare(node.Name, lst[i]);
                if (compare < 0)
                {
                    index = i;
                    break;
                }
            }
            if (index == -1) // 添加到末尾
            {
                if (item.IsDir && files.Count > 0)
                    nodes.Insert(dirs.Count, node);
                else
                    nodes.Add(node);
            }
            else
            {
                if (!item.IsDir)
                    index += dirs.Count;
                nodes.Insert(index, node);
            }
        }

        public static void SortNodes(TreeNodeCollection nodes)
        {
            List<TreeNode> dirs = new List<TreeNode>();
            List<TreeNode> files = new List<TreeNode>();
            foreach (TreeNode n in nodes)
            {
                if ((n.Tag as ProjectItem).IsDir)
                    dirs.Add(n);
                else
                    files.Add(n);
            }
            dirs.Sort(new Comparison<TreeNode>(delegate(TreeNode a, TreeNode b)
            {
                return string.Compare((a.Tag as ProjectItem).Name, (b.Tag as ProjectItem).Name);
            }));
            files.Sort(new Comparison<TreeNode>(delegate(TreeNode a, TreeNode b)
            {
                return string.Compare((a.Tag as ProjectItem).Name, (b.Tag as ProjectItem).Name);
            }));
            nodes.Clear();
            nodes.AddRange(dirs.ToArray());
            nodes.AddRange(files.ToArray());
        }

        /// <summary>
        /// 判断语言当前是否支持智能提示
        /// </summary>
        /// <param name="langType"></param>
        /// <returns></returns>
        public static bool IsAllowedAutoCompletion(Language lang)
        {
            if (!Config.Instance.AutoCompletion)
                return false;
            string allowAutoC = Config.Instance.AutoApplyTemplateLangs;
            if (string.IsNullOrEmpty(allowAutoC))
                return false;

            string l = lang.ToString();
            foreach (string item in allowAutoC.Split(','))
            {
                if (string.IsNullOrEmpty(item))
                    continue;
                if (l == item)
                    return true;
            }
            return false;
        }

        public static bool IsAllowedAutoCompletion(string file)
        {
            string ext = Path.GetExtension(file);
            ext = ext.ToLower();
            if (TagParser.Ext2Lang.ContainsKey(ext))
            {
                return IsAllowedAutoCompletion(TagParser.Ext2Lang[ext]);
            }
            else
                return false;
        }

        /// <summary>
        /// 尝试使用模板
        /// </summary>
        /// <param name="source"></param>
        /// <param name="dest"></param>
        /// <returns></returns>
        public static bool TryApplyTemplate(string source, string dest, Project proj)
        {
            if (!Config.Instance.AutoApplyTemplate)
                return false;
            string ext = Path.GetExtension(dest);
            if (string.IsNullOrEmpty(ext))
                return false;
            if (ext[0] == '.')
                ext = ext.Substring(1);
            string templateFile = Path.Combine(Config.Instance.TemplateDir, ext);
            if (!File.Exists(templateFile))
                return false;
            if (!string.IsNullOrEmpty(source) && !Path.GetFileNameWithoutExtension(source).StartsWith(NEW_FILE_NAME))
                return false;
            if (File.Exists(source))  // 如果源文件有内容，不使用模板
            {
                if (File.ReadAllText(source).Trim() != "")
                    return false;
                File.Delete(source);
            }

            string content = File.ReadAllText(templateFile);
            content = content.Replace("$(DateTime)", DateTime.Now.ToString());
            content = content.Replace("$(Date)", DateTime.Now.ToShortDateString());
            content = content.Replace("$(Time)", DateTime.Now.ToLongTimeString());
            content = content.Replace("$(FilePath)", dest);
            content = content.Replace("$(FileName)", Path.GetFileName(dest));
            content = content.Replace("$(ProjectName)", proj.Root.Name);
            content = content.Replace("$(ProjectDir)", proj.BaseDir);
            File.WriteAllText(dest, content);

            return true;

        }

        /// <summary>
        /// 移动文件夹
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        public static void MoveDir(string src, string dest)
        {
            if (src.ToLower() == dest.ToLower())
                return;
            Utility.Debug("{0} -> {1}", src, dest);
            CopyDir(src, dest);
            DeleteDir(src);
        }

        /// <summary>
        /// Copy the folder
        /// eg: d:\\a -> e:\\b, ====> e:\\b\\a
        /// </summary>
        /// <param name="src"></param>
        /// <param name="target"></param>
        public static void CopyDir(string src, string target)
        {
            if (src[src.Length - 1] == Path.DirectorySeparatorChar)
                src = src.Substring(0, src.Length - 1);
            string srcDirName = Path.GetFileName(src);

            if (target[target.Length - 1] != Path.DirectorySeparatorChar)
                target += Path.DirectorySeparatorChar;
            target = Path.Combine(target, srcDirName);
            if (!Directory.Exists(target))
                Directory.CreateDirectory(target);
            target += Path.DirectorySeparatorChar;

            string[] dirs = Directory.GetDirectories(src);
            foreach (string dir in dirs)
                CopyDir(dir, target);
            foreach (string file in Directory.GetFiles(src))
            {
                string destFile = Path.Combine(target, Path.GetFileName(file));
                File.Copy(file, destFile, true);
                //File.SetAttributes(destFile, FileAttributes.Archive);
                
            }
            
        }
        /// <summary>
        /// Add the folder
        /// </summary>
        /// <param name="target"></param>
        public static void AddDir( string target) {
            if (!Directory.Exists(target))
                return;

            string[] dirs = Directory.GetDirectories(target);
            foreach (string dir in dirs)
                AddDir(dir);
            //foreach (string file in Directory.GetFiles(target)) {
            //    string destFile = Path.Combine(target, Path.GetFileName(file));
           // }

        }
        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="dir"></param>
        public static void DeleteDir(string dir)
        {
            if (Directory.Exists(dir))
            {
                foreach (string file in Directory.GetFiles(dir))
                    File.Delete(file);
                foreach (string subDir in Directory.GetDirectories(dir))
                    DeleteDir(subDir);
                Directory.Delete(dir);
            }
        }

        public static string GetDirName(string dir)
        {
            while (!string.IsNullOrEmpty(dir) && (dir.EndsWith("\\") || dir.EndsWith("/")))
                dir = dir.Substring(0, dir.Length - 1);
            return Path.GetFileName(dir);
        }
    }
}