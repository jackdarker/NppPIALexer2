using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using NppPluginNET;
using System.Collections.Generic;
using NppPIALexer2.Forms;
using NppPIALexer2.Tag;



namespace NppPIALexer2
{

    class Main
    {
        #region " Fields "

        internal const string PluginName = "NppPIALexer2";

        internal static frmMain FrmMain = null;
        internal static frmSettings FrmSettings = null;
        internal static frmExternalTool FrmExternalTool = null;
        internal static frmAbout FrmAbout = null;
        //internal static frmBookmark FrmBookmark = null;
        internal static frmTagList FrmTagList = null;
        internal static frmFileSwitcher FrmFileSwitcher = null;
        internal static frmTaskList FrmTaskList = null;
        internal static frmLogList FrmLogList = null;
        internal static frmTaskDefinition FrmTaskDefinition = null;

        internal static int IdFrmMain = -1;

        static Bitmap tbBmp = Properties.Resources.Proj; 
        static Bitmap tbBmp_tbTab = Properties.Resources.ProjTab;
        static Icon tbIcon = null;
        

        #endregion

        #region "StartUp/CleanUp "
        private Main() { }
        private static Main s_Instance;
        public static Main getInstance() {
            if(s_Instance == null) {
                s_Instance = new Main();
            }
            return s_Instance;
        }
        internal static void CommandMenuInit()
        {
            // 设置菜单命令
            int index = 0;
            PluginBase.SetCommand(index, "NppPIALexer2", ShowNppPIALexer2View, new ShortcutKey(true, false, true, Keys.P)); 
            IdFrmMain = index;

            PluginBase.SetCommand(++index, "", null);
            PluginBase.SetCommand(++index, "Class View", ShowNppTagList, new ShortcutKey(true, false, true, Keys.T));
            //PluginBase.SetCommand(++index, "Bookmark", ShowNppBookmark, new ShortcutKey(true, false, true, Keys.B));
            PluginBase.SetCommand(++index, "Task List", ShowNppTaskList);
            PluginBase.SetCommand(++index, "Log List", ShowNppLogList, new ShortcutKey(true, false, true, Keys.B));
            PluginBase.SetCommand(++index, "File Switcher", ShowFileSwitcher, new ShortcutKey(true, false, true, Keys.F));
            PluginBase.SetCommand(++index, "", null);
            PluginBase.SetCommand(++index, "Auto Completion", ShowAutoCompletion, new ShortcutKey(true, false, false, Keys.J));
            PluginBase.SetCommand(++index, "Go To Definition", GotoDefinition, new ShortcutKey(false, false, false, Keys.F12));
            PluginBase.SetCommand(++index, "Jump Back", JumpBack, new ShortcutKey(false, true, false, Keys.Left));
            PluginBase.SetCommand(++index, "Jump Farword", JumpForward, new ShortcutKey(false, true, false, Keys.Right));
            PluginBase.SetCommand(++index, "", null);
            PluginBase.SetCommand(++index, "Settings", ShowNppSettings);
            PluginBase.SetCommand(++index, "About", ShowNppAbout);
        }

        internal static void SetToolBarIcon()
        {
            toolbarIcons tbIcons = new toolbarIcons();
            tbIcons.hToolbarBmp = tbBmp.GetHbitmap();
            IntPtr pTbIcons = Marshal.AllocHGlobal(Marshal.SizeOf(tbIcons));
            Marshal.StructureToPtr(tbIcons, pTbIcons, false);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_ADDTOOLBARICON, PluginBase._funcItems.Items[IdFrmMain]._cmdID, pTbIcons);
            Marshal.FreeHGlobal(pTbIcons);
        }
        public event FileBufferChanged EvtFileBufferChanged;
        public delegate void FileBufferChanged(String filepath);
        public void ChangeFileBuffer(String File) {
            if(EvtFileBufferChanged != null) EvtFileBufferChanged(File);
        }

        internal static void InitNppPIALexer2()
        {
            if(Config.Instance.Visible) {
                ShowNppPIALexer2View();
                ShowNppLogList();
                ShowNppTagList();
            }

            // NppPIALexer2 All the components are started after the NppPIALexer2 window opens
            // so the associated initialization operation is placed in the ShowNppPIALexer2View () function
        }

        /// <summary>
        /// 将信息记录到配置文件
        /// </summary>
        internal static void PluginCleanUp()
        {
            try { Config.Instance.Save(); }
            catch { }
            foreach (Project proj in ProjectManager.Projects)
            {
                try { proj.Save(); }
                catch { }
            }
            CommandManager.Save();  
        }

        #endregion

        #region " Menu functions "

        static void _RegisterTagParser()
        {
            // 注册语句解析器
            TagParser.Register(new CSharpTagParser());
            //TagParser.Register(new PythonTagParser());
            TagParser.Register(new CppTagParser());
            TagParser.Register(new CTagParser());
            TagParser.Register(new JavaTagParser());
            TagParser.Register(new JavaScriptTagParser());
            TagParser.Register(new FlexTagParser());
            TagParser.Register(new PHPTagParser());
            TagParser.Register(new AsmTagParser());
            TagParser.Register(new RubyTagParser());
            TagParser.Register(new PascalTagParser());
        }

        /// <summary>
        /// 显示/隐藏 项目树
        /// </summary>
        internal static void ShowNppPIALexer2View()
        {
            if (FrmMain == null)
            {
                string path = Path.Combine(Config.Instance.NppPIALexer2Dir, "ctags.exe");
                if (!File.Exists(path))
                {
                    //??Utility.Error(@"Can't find '${Config}\NppPIALexer2\ctags.exe'.");
                 //??   return;
                }

                _RegisterTagParser();
             //??   TagParser.LoadExt2LangMapping();

                CommandManager.Load();

                FrmMain = new frmMain();

                // 设置状态栏图标
                using (Bitmap newBmp = new Bitmap(16, 16))
                {
                    Graphics g = Graphics.FromImage(newBmp);
                    ColorMap[] colorMap = new ColorMap[1];
                    colorMap[0] = new ColorMap();
                    colorMap[0].OldColor = Color.White;
                    colorMap[0].NewColor = Color.FromKnownColor(KnownColor.ButtonFace);
                    ImageAttributes attr = new ImageAttributes();
                    attr.SetRemapTable(colorMap);
                    g.DrawImage(tbBmp_tbTab, new Rectangle(0, 0, 16, 16), 0, 0, 16, 16, GraphicsUnit.Pixel, attr);
                    tbIcon = Icon.FromHandle(newBmp.GetHicon());
                }

                NppTbData _nppTbData = new NppTbData();
                _nppTbData.hClient = FrmMain.Handle;
                _nppTbData.pszName = "NppPIALexer2";
                _nppTbData.dlgID = IdFrmMain;
                _nppTbData.uMask = NppTbMsg.DWS_DF_CONT_LEFT | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
                _nppTbData.hIconTab = (uint)tbIcon.Handle;
                _nppTbData.pszModuleName = PluginName;
                IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
                Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);

                Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
                Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_SETMENUITEMCHECK, PluginBase._funcItems.Items[Main.IdFrmMain]._cmdID, 1);

                Win32.SendMessage(PluginBase.nppData._scintillaMainHandle, SciMsg.SCI_AUTOCSETMAXHEIGHT, 15, 0);
                Win32.SendMessage(PluginBase.nppData._scintillaSecondHandle, SciMsg.SCI_AUTOCSETMAXHEIGHT, 15, 0);
                NPP.SetupFolding();
                // load Project 
                foreach (Project proj in ProjectManager.Projects)
                    FrmMain.BindProject(proj);

                // Themen erstellen, Etiketten und Lesezeichen Überwachung von Änderungen
                TagUpdater.Work();
                TaskUpdater.Work();
                //BookmarkUpdater.Work();

                Utility.UnderLineTreeView();
            }
            else
            {
                if (FrmMain.Visible)
                    Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMHIDE, 0, FrmMain.Handle);
                else
                {
                    Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMSHOW, 0, FrmMain.Handle);
                    Utility.UnderLineTreeView();
                }
            }
        }

        /// <summary>
        /// 显示/隐藏 关于
        /// </summary>
        internal static void ShowNppAbout()
        {
            if (FrmMain == null)
                ShowNppPIALexer2View();


            if (FrmAbout == null)
            {
                FrmAbout = new frmAbout();
                FrmAbout.ShowDialog(Main.FrmMain);
            }
            else
            {
                if (FrmAbout.Visible)
                    FrmAbout.Hide();
                else
                    FrmAbout.ShowDialog(Main.FrmMain);
            }
        }

        /// <summary>
        /// 显示/隐藏 ClassView
        /// </summary>
        internal static void ShowNppTagList()
        {
            if (FrmMain == null)
                ShowNppPIALexer2View();

            
            

            if (FrmTagList == null)
            {
                FrmTagList = new frmTagList();

                NppTbData _nppTbData = new NppTbData();
                _nppTbData.hClient = FrmTagList.Handle;
                _nppTbData.pszName = "NppPIALexer2 Class View";
                _nppTbData.uMask = NppTbMsg.DWS_DF_CONT_RIGHT;// | NppTbMsg.DWS_ICONTAB | NppTbMsg.DWS_ICONBAR;
                _nppTbData.pszModuleName = "NppPIALexer2 Class View";
                IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
                Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);

                Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
            }
            else
            {
                if (FrmTagList.Visible)
                    Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMHIDE, 0, FrmTagList.Handle);
                else
                    Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMSHOW, 0, FrmTagList.Handle);
            }
        }

        ///// <summary>
        ///// 显示/隐藏 书签
        ///// </summary>
        //internal static void ShowNppBookmark()
        //{
        //    if (FrmMain == null)
        //        ShowNppPIALexer2View();

        //    if (FrmBookmark == null)
        //    {
        //        FrmBookmark = new frmBookmark();

        //        NppTbData _nppTbData = new NppTbData();
        //        _nppTbData.hClient = FrmBookmark.Handle;
        //        _nppTbData.pszName = "NppPIALexer2 Bookmark";
        //        _nppTbData.uMask = NppTbMsg.DWS_DF_CONT_BOTTOM;// | NppTbMsg.DWS_ICONTAB;// | NppTbMsg.DWS_ICONBAR;
        //        _nppTbData.pszModuleName = "NppPIALexer2 Bookmark";
        //        IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
        //        Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);

        //        Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
        //    }
        //    else
        //    {
        //        if (FrmBookmark.Visible)
        //            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMHIDE, 0, FrmBookmark.Handle);
        //        else
        //            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMSHOW, 0, FrmBookmark.Handle);
        //    }
        //}

        /// <summary>
        /// Show/Hide Task List
        /// </summary>
        internal static void ShowNppTaskList()
        {
            if (FrmMain == null)
                ShowNppPIALexer2View();

            if (FrmTaskList == null)
            {
                FrmTaskList = new frmTaskList();

                NppTbData _nppTbData = new NppTbData();
                _nppTbData.hClient = FrmTaskList.Handle;
                _nppTbData.pszName = "NppPIALexer2 TaskList";
                _nppTbData.uMask = NppTbMsg.DWS_DF_CONT_BOTTOM;// | NppTbMsg.DWS_ICONTAB;// | NppTbMsg.DWS_ICONBAR;
                _nppTbData.pszModuleName = "NppPIALexer2 TaskList";
                IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
                Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);

                Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);
            }
            else
            {
                if (FrmTaskList.Visible)
                    Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMHIDE, 0, FrmTaskList.Handle);
                else
                    Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMSHOW, 0, FrmTaskList.Handle);
            }
        }
        internal static void ShowNppLogList() {
            if(FrmMain == null)
                ShowNppPIALexer2View();

            if(FrmLogList == null) {
                FrmLogList = new frmLogList();

                NppTbData _nppTbData = new NppTbData();
                _nppTbData.hClient = FrmLogList.Handle;
                _nppTbData.pszName = "NppPIALexer2 LogList";
                _nppTbData.uMask = NppTbMsg.DWS_DF_CONT_BOTTOM;// | NppTbMsg.DWS_ICONTAB;// | NppTbMsg.DWS_ICONBAR;
                _nppTbData.pszModuleName = "NppPIALexer2 LogList";
                IntPtr _ptrNppTbData = Marshal.AllocHGlobal(Marshal.SizeOf(_nppTbData));
                Marshal.StructureToPtr(_nppTbData, _ptrNppTbData, false);

                Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMREGASDCKDLG, 0, _ptrNppTbData);

            } else {
                if(FrmLogList.Visible)
                    Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMHIDE, 0, FrmLogList.Handle);
                else
                    Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DMMSHOW, 0, FrmLogList.Handle);
            }
        }

        internal static void ShowNppSettings()
        {
            if (FrmMain == null)
                ShowNppPIALexer2View();

            if (Main.FrmSettings == null)
            {
                Main.FrmSettings = new Forms.frmSettings();
                Main.FrmSettings.ShowDialog();
            }
            else
            {
                if (!Main.FrmSettings.Visible)
                    Main.FrmSettings.ShowDialog();
                else
                    Main.FrmSettings.Hide();
            }
        }

        internal static void ShowNppTaskDefinition()
        {
            if (Main.FrmTaskDefinition == null)
            {
                Main.FrmTaskDefinition = new frmTaskDefinition();
                Main.FrmTaskDefinition.ShowDialog();
            }
            else
            {
                if (!Main.FrmTaskDefinition.Visible)
                    Main.FrmTaskDefinition.ShowDialog();
                else
                    Main.FrmTaskDefinition.Hide();
            }
        }

        /// <summary>
        /// Show/Hide file quick Position Window
        /// </summary>
        internal static void ShowFileSwitcher()
        {
            if (FrmMain == null)
                ShowNppPIALexer2View();

            if (FrmFileSwitcher == null)
            {
                FrmFileSwitcher = new frmFileSwitcher();
                FrmFileSwitcher.Show();
            }
            else
            {
                FrmFileSwitcher.Show();
            }
        }

        static ContextMenuStrip _ctntGoToDefinition = null;
        internal static void GotoDefinition()
        {
            if (FrmMain == null)
                ShowNppPIALexer2View();

            string tagName = NPP.GetCurrentWord2();
            List<ITag> lst = TagCache.SearchTag(tagName, TagParser.GetDefaultLang(NPP.GetCurrentFile()));
            if (lst.Count == 0)
                return;

            if (lst.Count == 1)
            {
                Jump.Add(tagName, lst[0].SourceFile, lst[0].LineNo - 1);
                //NPP.GoToDefinition(lst[0].SourceFile, lst[0].LineNo - 1, lst[0].TagName);
                Jump.Cursor.Go();
            }
            else
            {
                if (_ctntGoToDefinition == null)
                    _ctntGoToDefinition = new ContextMenuStrip();

                Point pos = NPP.GetCurrentPoint();
                _ctntGoToDefinition.Items.Clear();
                foreach (ITag tag in lst)
                {
                    string txt = string.Format("{0}    [{1}] {2}", tag.FullName, tag.LineNo, tag.SourceFile);
                    ToolStripMenuItem item = new ToolStripMenuItem(txt);
                    item.Tag = tag;
                    item.ToolTipText = tag.Signature;
                    _ctntGoToDefinition.Items.Add(item);
                    item.Click += new EventHandler(delegate(object src, EventArgs ex)
                    {
                        ToolStripMenuItem i = src as ToolStripMenuItem;
                        if (i != null)
                        {
                            var t = item.Tag as ITag;
                            Jump.Add(t.TagName, t.SourceFile, t.LineNo - 1);
                            Jump.Cursor.Go();
                            //NPP.GoToDefinition(t.SourceFile, t.LineNo - 1, t.TagName);
                        }
                    });
                }
                _ctntGoToDefinition.Show(pos);
            }
        }

        internal static void JumpBack()
        {
            if (FrmMain == null)
                ShowNppPIALexer2View();

            Jump cur = Jump.Cursor;
            if (cur != null)
            {
                string file = NPP.GetCurrentFile();  // After entering the function, the cursor changes and goes back to the function
                int line = NPP.GetCurrentPosition(); //?? .GetCurrentLine();    
                if (file != cur.File || line != cur.Pos) //??.LineNo)   Todo LineNo is not properly set when jumping
                    cur.Go();
                else  // After entering the function, the cursor does not leave the current line and returns to the previous position
                {
                    Jump back = Jump.Back;
                    if (back != null)
                        back.Go();
                }
            }
        }

        internal static void JumpForward()
        {
            if (FrmMain == null)
                ShowNppPIALexer2View();

            Jump cur = Jump.Cursor;
            if (cur != null)
            {
                string file = NPP.GetCurrentFile();
                int line = NPP.GetCurrentLine();
                if (file != cur.File || line != cur.LineNo)
                    cur.Go();
                else
                {
                    Jump fard = Jump.Forward;
                    if (fard != null)
                        fard.Go();
                }
            }
        }

        static List<string> _Last = null;
        static bool _IsEqual(List<string> cur)
        {
            if (cur != null && _Last != null)
            {
                if (_Last.Count != cur.Count)
                    return false;
                for (int i = 0; i < _Last.Count; ++i)
                    if (_Last[i] != cur[i])
                        return false;
                return true;
            }
            return false;
        }

        internal static void ShowAutoCompletion()
        {
            ShowAutoCompletion(true);
        }

        /// <summary>
        /// 智能提示
        /// </summary>
        /// <param name="enfocus"></param>
        internal static void ShowAutoCompletion(bool enfocus)
        {
            if (FrmMain == null)
                ShowNppPIALexer2View();

            string curFile = NPP.GetCurrentFile();
            string ext = Path.GetExtension(curFile);
            //TagParser.Ext2Lang.TryGetValue(ext, out lang);
            //??if (Utility.IsAllowedAutoCompletion(lang))
            if (ext.Equals(ModelDocument.FileExtension, StringComparison.OrdinalIgnoreCase))
            {
                Project proj = ProjectManager.GetProjectByItsFile(curFile);
                if (proj != null)
                {
                    string word = NPP.GetCurrentWord();
                    string line = NPP.GetLine(NPP.GetCurrentLine());
                    if (word.Length >= 2)
                    {
                        List<NppPIALexer2.ObjDecl> lst = proj.Model.lookupAll(word,line,proj.Model.GetRelativePath( curFile));
                        //proj.Model.GetObjects(word, curFile, "", out lst);
                        if (enfocus || lst.Count > 0 )//&& !_IsEqual(lst))
                            NPP.ShowAutoCompletion(word.Length, lst);
                        //_Last = lst;

                        //if (lst.Count > 0)
                        //{
                        //    if (focus)
                        //        NPP.ShowAutoCompletion(word.Length, lst);
                        //    else
                        //    {
                        //        if (!_IsEqual(lst))
                        //        {
                        //            NPP.ShowAutoCompletion(word.Length, lst);
                        //        }
                        //    }
                        //}
                        //_Last = lst;

                        //if (lst.Count > 0)
                        //{
                        //    if (_LastWord != lst[0] || _Count != lst.Count)
                        //    {
                        //        _LastWord = lst[0];
                        //        _Count = lst.Count;
                        //        NPP.ShowAutoCompletion(word.Length, lst);
                        //    }
                        //}
                        //else
                        //{
                        //    _LastWord = "";
                        //    _Count = 0;
                        //}
                    }
                }
            }
        }

        #endregion
    }
}