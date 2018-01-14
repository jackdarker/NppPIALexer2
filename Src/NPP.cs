using System.Text;
using NppPluginNET;
using System.Collections.Generic;
using System;
using System.Drawing;
namespace NppPIALexer2
{
    /// <summary>
    /// 封装与Notepad++ API相关操作
    /// </summary>
    public class NPP
    {
        /// <summary>
        /// Get the currently open file
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentFile()
        {
            // NPPM_GETFULLCURRENTPATH;
            // NPPM_GETFILENAME;
            // NPPM_GETCURRENTDIRECTORY;
            string filename = "";
            StringBuilder sb = new StringBuilder(Win32.MAX_PATH);
            if ((int)Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETFULLCURRENTPATH, Win32.MAX_PATH, sb) == 1)
                filename = sb.ToString();
            return filename;
        }

        /// <summary>
        /// Get list of open files
        /// </summary>
        /// <returns></returns>
        public static List<string> GetOpenedFiles()
        {
            List<string> openedFiles = new List<string>();
            int nbFile = (int)Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETNBOPENFILES, 0, 0);
            using (ClikeStringArray cStrArray = new ClikeStringArray(nbFile, Win32.MAX_PATH))
            {
                if (Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETOPENFILENAMES, cStrArray.NativePointer, nbFile) != IntPtr.Zero)
                    foreach (string s in cStrArray.ManagedStringsUnicode)
                        openedFiles.Add(s);
            }
            return openedFiles;
        }

        /// <summary>
        /// 对数组进行过滤，返回npp打开的文件列表
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public static List<string> FilterOpenedFiles(List<string> files)
        {
            List<string> ret = new List<string>();
            List<string> opened = GetOpenedFiles();
            foreach (string file in files)
                foreach (string o in opened)
                {
                    if (file.ToLower() == o.ToLower())
                    {
                        ret.Add(file);
                        break;
                    }
                }
            return ret;
        }

        /// <summary>
        /// 关闭NPP已经打开的文件
        /// </summary>
        /// <param name="file"></param>
        public static void CloseFile(string file)
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DOOPEN, 0, file);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_MENUCOMMAND, 0, NppMenuCmd.IDM_FILE_CLOSE);
        }

        /// <summary>
        /// 在NPP中打开文件
        /// </summary>
        /// <param name="file"></param>
        public static void OpenFile(string file)
        {
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_DOOPEN, 0, file);
        }

        /// <summary>
        /// 判断某个文件是否打开
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool IsOpened(string file)
        {
            file = file.ToLower();
            foreach (string f in GetOpenedFiles())
            {
                if (f.ToLower() == file)
                    return true;
            }
            return false;
        }

        static char[] _Spliter = { ' ', '.', '\n', '\t', '(', ')' };
        /// <summary>
        /// Get the current cursor at the forward word
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentWord()
        {
            int currentPos = (int)Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GETCURRENTPOS, 0, 0);
            int size = 64;
            int beg = currentPos - (size - 1);
            beg = beg > 0 ? beg : 0;
            int end = currentPos;
            size = end - beg;
            Sci_TextRange txtRange = new Sci_TextRange(beg, end, 64);
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GETTEXTRANGE, 0, txtRange.NativePointer);
            string[] arr = txtRange.lpstrText.Split(_Spliter);
            return arr[arr.Length - 1];
        }

        /// <summary>
        /// Gets the word where the current cursor is located (the word is selected and the edit focus is lost
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentWord2()
        {
            StringBuilder sb = new StringBuilder(128);
            if (Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETCURRENTWORD, 128, sb) != IntPtr.Zero)
            {
                Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GRABFOCUS, 0, 0);
                return sb.ToString();
            }
            else
                return "";
        }

        /// <summary>
        /// Gets the current row, starting with 0
        /// </summary>
        /// <returns></returns>
        public static int GetCurrentLine()
        {
            return (int)Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETCURRENTLINE, 0, 0);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static int GetLineFromPosition(int pos) {
            return (int) Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_LINEFROMPOSITION, pos, 0);
        }
        /// <summary>
        /// Get the specified row contents
        /// </summary>
        /// <param name="lineNo">行号，从0开始</param>
        /// <returns></returns>
        public static string GetLine(int lineNo)
        {
            int lineLength = (int)Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GETLINE, lineNo, 0);

            StringBuilder sb = new StringBuilder(lineLength);
            if((int)Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GETLINE, lineNo, sb) != -1) {
                return sb.ToString(0,lineLength); //seems there is always on char to much returned
            }
            return "";

        }

        /// <summary>
        /// Show smart tips
        /// </summary>
        /// <param name="count"></param>
        /// <param name="lst"></param>
        public static void ShowAutoCompletion(int count, List<string> lst)
        {
            if (lst.Count == 0)
                return;

            StringBuilder sb = new StringBuilder();
            foreach (string item in lst)
            {
                if (sb.Length == 0)
                    sb.Append(item);
                else
                    sb.Append(" " + item);
            }
            
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_AUTOCSHOW, count, sb.ToString());
        }
        public static void ShowAutoCompletion(int count, List<NppPIALexer2.ObjDecl> lst) {
            if (lst.Count == 0)
                return;

            StringBuilder sb = new StringBuilder();
            foreach (NppPIALexer2.ObjDecl item in lst) {
                if (sb.Length == 0)
                    sb.Append(item.Function());
                else
                    sb.Append(" " + item.Function());
            }

            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_AUTOCSHOW, count, sb.ToString());
          //?? can only display AC or CT  Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_CALLTIPSHOW, 1, sb.ToString());

        }

        /// <summary>
        /// Get the current cursor position and convert it to point
        /// </summary>
        /// <returns></returns>
        public static Point GetCurrentPoint()
        {
            IntPtr curSci = PluginBase.GetCurrentScintilla();
            int currentPos = (int)Win32.SendMessage(curSci, SciMsg.SCI_GETCURRENTPOS, 0, 0);
            Point pt = new Point();
            pt.X = (int)Win32.SendMessage(curSci, SciMsg.SCI_POINTXFROMPOSITION, 0, currentPos);
            pt.Y = (int)Win32.SendMessage(curSci, SciMsg.SCI_POINTYFROMPOSITION, 0, currentPos);
            Win32.ClientToScreen(curSci, ref pt);
            return pt;
        }

        public static int GetCurrentPosition()
        {
            return (int)Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GETCURRENTPOS, 0, 0);
        }

        ///// <summary>
        ///// 获取当前语言(NPP中定义的语言类型)
        ///// </summary>
        ///// <returns></returns>
        //public static int GetCurrentLang()
        //{
        //    int langType;
        //    Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETCURRENTLANGTYPE, 0, out langType);
        //    return langType;
        //}

        /// <summary>
        /// Go to the specified line of the file
        /// </summary>
        /// <param name="file"></param>
        /// <param name="lineNo">行号,从0开始</param>
        public static void GoToLine(string file, int lineNo)
        {
            OpenFile(file);
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GOTOLINE, lineNo, 0);
        }

        /// <summary>
        /// Go to function definition
        /// </summary>
        /// <param name="file"></param>
        /// <param name="lineNo">行号，从0开始</param>
        /// <param name="tagName"></param>
        public static void GoToDefinition(string file, int lineNo, string tagName)
        {
            GoToLine(file, lineNo);
            int startPos = (int)Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_POSITIONFROMLINE, lineNo, 0);
            int endPos = (int)Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_POSITIONFROMLINE, lineNo + 1, 0);
            if (endPos <= startPos)
                return;
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_SETTARGETSTART, startPos, 0);
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_SETTARGETEND, endPos, 0);
            int pos = (int)Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_SEARCHINTARGET, tagName.Length, tagName);
            if (pos != -1)
            {
                Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GOTOPOS, pos, 0);
                //Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GRABFOCUS, 0, 0);
                NPP.GetCurrentWord2();
            }
        }

        public static void GoToDefinition(string file, int pos)
        {
            NPP.OpenFile(file);
            int _line =GetLineFromPosition(pos);
            GoToLine(_line);
            //Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GOTOPOS, pos, 0);
            //NPP.GetCurrentWord2();
        }

        /// <summary>
        /// Go to the specified line of the current file
        /// </summary>
        /// <param name="lineNo">行号，从0开始</param>
        public static void GoToLine(int lineNo)
        {
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GOTOLINE, lineNo, 0);
        }

        /// <summary>
        /// Gets the bookmark in the specified file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static List<int> GetBookmarks()
        {
            List<int> lst = new List<int>();
            int nextLine = 0;
            while (true)
            {
                nextLine = (int)Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_MARKERNEXT, nextLine, 1 << 24); // 参考notepad源码：int(_pEditView->execute(sci_marker, lineStart, 1 << MARK_BOOKMARK));
                if (nextLine == -1)
                    break;
                lst.Add(nextLine);
                nextLine++;
            }
            return lst;
        }

        ///// <summary>
        ///// 获取当前页的书签
        ///// </summary>
        ///// <returns></returns>
        //public static List<int> GetBookmarks()
        //{
        //    return GetBookmarks(GetCurrentFile());
        //}


        const int _MARK_BOOKMARK = 1 << 24;  // 查阅notepad++ src
        /// <summary>
        /// 对当前Active文件设置 书签
        /// </summary>
        /// <param name="lineNo"></param>
        public static void SetBookmark(int lineNo)
        {
            int state = (int)Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_MARKERGET, lineNo, 0);
            bool bookExist = (_MARK_BOOKMARK & state) != 0;
            if (bookExist)
                return;

            //int backLine = (int)Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETCURRENTLINE, 0, 0);
            int pos = (int)Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GETCURRENTPOS, 0, 0);
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GOTOLINE, lineNo, 0);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_MENUCOMMAND, 0, NppMenuCmd.IDM_SEARCH_TOGGLE_BOOKMARK);
            //Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GOTOLINE, backLine, 0);
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GOTOPOS, pos, 0);
            

            // //下面代码无法设置书签，why？
            //Utility.Info("SetBookmark: {0}", (int)Win32.SendMessage(PluginBase.nppData._nppHandle, SciMsg.SCI_MARKERADD, lineNo, _MARK_BOOKMARK)); 
        }

        public static void DeleteBookmark(int lineNo)
        {
            int state = (int)Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_MARKERGET, lineNo, 0);
            bool bookExist = (_MARK_BOOKMARK & state) != 0;
            if (!bookExist)
                return;

            //int backLine = (int)Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETCURRENTLINE, 0, 0);
            int pos = (int)Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GETCURRENTPOS, 0, 0);
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GOTOLINE, lineNo, 0);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_MENUCOMMAND, 0, NppMenuCmd.IDM_SEARCH_TOGGLE_BOOKMARK);
            //Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GOTOLINE, backLine, 0);
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GOTOPOS, pos, 0);
        }
        /// <summary>
        /// Toggles a fold marker
        /// </summary>
        /// <returns></returns>
        public static void SetFoldLevel(int line, uint level, bool FoldHeader) {
            if (FoldHeader) level = level | (uint)SciMsg.SC_FOLDLEVELHEADERFLAG;
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_SETFOLDLEVEL, line, level );
        }
        /// <summary>
        /// Toggles a fold marker
        /// </summary>
        /// <returns></returns>
        public static void ToggleFold(int line) {
            if (line < 0) {
                return;
            }
            int state = (int)Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_TOGGLEFOLD, line, 0);
            state = (int)Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_GETFOLDEXPANDED, line, 0);
            /*if(state!=0) {
                Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_MARKERDELETE, line, SciMsg.SC_MARKNUM_FOLDER);
                Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_MARKERADD, line, SciMsg.SC_MARKNUM_FOLDEROPEN);
            } else {
                Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_MARKERDELETE, line, SciMsg.SC_MARKNUM_FOLDEROPEN);
                Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_MARKERADD, line, SciMsg.SC_MARKNUM_FOLDER);
            }*/
        }
        public static void SetupFolding() {
            //see here https://www.garybeene.com/code/gbsnippets_gbs_00669.htm
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_SETMARGINWIDTHN, 0, 20);//display line numbers
            //Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_SETMARGINWIDTHN, 1, 20);//no breakpoints
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_SETMARGINWIDTHN, 2,20);//display folding in margin2
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_SETMARGINMASKN, 2, SciMsg.SC_MASK_FOLDERS);//margin2 set to hold folder symbols
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_SETMARGINSENSITIVEN, 2, 1); //margin2 set as sensitive to clicks
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_MARKERDEFINE, SciMsg.SC_MARKNUM_FOLDEROPEN, SciMsg.SC_MARK_ARROWDOWN);
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_MARKERDEFINE, SciMsg.SC_MARKNUM_FOLDER, SciMsg.SC_MARK_ARROW);
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_MARKERDEFINE, SciMsg.SC_MARKNUM_FOLDERSUB, SciMsg.SC_MARK_VLINE);
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_MARKERDEFINE, SciMsg.SC_MARKNUM_FOLDERTAIL, SciMsg.SC_MARK_LCORNER);
            //Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_MARKERDEFINE, SciMsg.SC_MARKNUM_FOLDERMIDTAIL, SciMsg.SC_MARK_EMPTY);
            //Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_MARKERDEFINE, SciMsg.SC_MARKNUM_FOLDEROPENMID, SciMsg.SC_MARK_EMPTY);
            //Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_MARKERDEFINE, SciMsg.SC_MARKNUM_FOLDEREND, SciMsg.SC_MARK_EMPTY);
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_MARKERSETBACK, SciMsg.SC_MARKNUM_FOLDERSUB, 0);
            Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_MARKERSETBACK, SciMsg.SC_MARKNUM_FOLDERTAIL, 0);
        }
        public static void SetupLexer() {
            // see https://www.garybeene.com/code/gbsnippets_gbs_00664.htm
            //this will enable the SCN_STYLENEEDED notification
         //??   Win32.SendMessage(PluginBase.GetCurrentScintilla(), SciMsg.SCI_SETLEXER, SciMsg.SCLEX_CONTAINER, 0);
        }
    }
}