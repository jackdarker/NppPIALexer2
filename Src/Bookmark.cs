//using System;
//using System.Collections.Generic;
//using System.Threading;
//using System.Text;


//namespace NppProject
//{
//    public class Bookmark
//    {
//        string _guid;

//        public Bookmark(string name, string file, int lineno, string line)
//        {
//            Name = name;
//            File = file;
//            LineNo = lineno;
//            _guid = Guid.NewGuid().ToString();
//            Line = line;
//        }

//        public Bookmark(string file, int lineno, string line)
//            : this("", file, lineno, line)
//        { }

//        /// <summary>
//        /// 自定义书签名 todo:xxx
//        /// </summary>
//        public string Name { get; set; }
//        /// <summary>
//        /// 书签所在文件(在加载项目时动态设置成绝对路径)
//        /// </summary>
//        public string File { get; set; }
//        public int LineNo { get; set; }
//        public string ID { get { return _guid; } }
//        public string Line { get; set; }

//        public override string ToString()
//        {
//            return string.Format("Bookmark|Name: {0}, File: {1}, LineNo: {2}, Line: {3}", Name, File, LineNo, Line);
//        }
//    }


//    public delegate void BookmarkChanged(string file, List<Bookmark> removed, List<Bookmark> added, List<Bookmark> updated);

//    /// <summary>
//    /// 跟踪书签变化
//    /// </summary>
//    public class BookmarkUpdater
//    {
//        public static event BookmarkChanged BookmarkChanged;

//        public static void Work()
//        {
//            Thread t = new Thread(_Work);
//            t.Start();
//        }

//        public static string CurrentFile = "";  // 当前在NPP中激活的文档, 在NPPN_BUFFERACTIVATED中设置

//        static void _Work() 
//        {
//            while (true)
//            {
//                Thread.Sleep(1000);

//                string file = NPP.GetCurrentFile();
//                //string file = CurrentFile;
//                //if (string.IsNullOrEmpty(file))
//                //    continue;
//                ProjectItem item = ProjectManager.GetProjectItemByFile(file);
//                if (item == null)
//                    continue;
//                List<int> newBk = NPP.GetBookmarks();
//                List<string> bkLines = new List<string>();
//                foreach (int bk in newBk)
//                    bkLines.Add(NPP.GetLine(bk));

//                List<Bookmark> removed = new List<Bookmark>();  // 删除的书签
//                for (int i = item.Bookmarks.Count - 1; i >= 0; --i)
//                {
//                    bool exist = false;
//                    for (int j = 0; j < newBk.Count; ++j )
//                        if (bkLines[j] == item.Bookmarks[i].Line)
//                        {
//                            exist = true;
//                            break;
//                        }
//                    if (!exist)
//                    {
//                        removed.Add(item.Bookmarks[i]);
//                        item.Bookmarks.RemoveAt(i);
//                    }
//                }

//                List<Bookmark> added = new List<Bookmark>();  // 新增的书签
//                for (int j = 0; j < newBk.Count; ++j)
//                {
//                    int lineno = newBk[j];
//                    bool exist = false;
//                    foreach (Bookmark bk in item.Bookmarks)
//                        if (bk.Line == bkLines[lineno])
//                        {
//                            exist = true;
//                            break;
//                        }
//                    if (!exist)
//                        added.Add(new Bookmark(file, lineno, bkLines[lineno]));
//                }
//                item.Bookmarks.AddRange(added);

//                List<Bookmark> updated = new List<Bookmark>();
//                //for (int r = removed.Count - 1; r >= 0; --r)
//                //    for (int a = added.Count - 1; a >= 0; --a)
//                //        if (removed[r].Line == added[a].Line && !string.IsNullOrEmpty(added[a].Line))   // 有问题!
//                //        {
//                //            var bk = removed[r];
//                //            bk.LineNo = added[a].LineNo;
//                //            updated.Add(bk);
//                //            item.Bookmarks.Add(bk);
//                //            item.Bookmarks.Remove(added[a]);
//                //            removed.RemoveAt(r);
//                //            added.RemoveAt(a);
//                //            break;
//                //        }
                

//                if ((added.Count > 0 || removed.Count > 0 || updated.Count > 0) && BookmarkChanged != null)
//                {
//                    item.Project.Save();
//                    BookmarkChanged(file, removed, added, updated);
//                }
//            }
//        }

//        public static void Remove(params string[] files)
//        {
//            if (BookmarkChanged == null)
//                return;

//            foreach (string file in files)
//            {
//                var item = ProjectManager.GetProjectItemByFile(file);
//                if (item != null)
//                    BookmarkChanged(file, item.Bookmarks, null, null);
//            }
//        }

//        public static void Update(params string[] files)
//        {
//            if (BookmarkChanged == null)
//                return;

//            foreach (string file in files)
//            {
//                Utility.Debug("bookmark update: {0}", file);
//                var item = ProjectManager.GetProjectItemByFile(file);
//                if (item != null)
//                {
//                    foreach (var bk in item.Bookmarks)
//                        bk.File = file; // 文件改名
//                    Utility.Debug("---- bookmarks: {0}", item.Bookmarks.Count);
//                    BookmarkChanged(file, null, null, item.Bookmarks);
//                }
//            }
//        }
//    }
//}