using System;
using System.Runtime.InteropServices;
using NppPluginNET;
using NppPlugin.DllExport;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Collections.Generic;
using NppPIALexer2.Tag;

namespace NppPIALexer2
{
    class UnmanagedExports
    {
        [DllExport(CallingConvention=CallingConvention.Cdecl)]
        static bool isUnicode()
        {
            return true;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void setInfo(NppData notepadPlusData)
        {
            PluginBase.nppData = notepadPlusData;
            Main.CommandMenuInit();
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr getFuncsArray(ref int nbF)
        {
            nbF = PluginBase._funcItems.Items.Count;
            return PluginBase._funcItems.NativePointer;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static uint messageProc(uint Message, IntPtr wParam, IntPtr lParam)
        {
            return 1;
        }

        static IntPtr _ptrPluginName = IntPtr.Zero;
        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static IntPtr getName()
        {
            if (_ptrPluginName == IntPtr.Zero)
                _ptrPluginName = Marshal.StringToHGlobalUni(Main.PluginName);
            return _ptrPluginName;
        }

        [DllExport(CallingConvention = CallingConvention.Cdecl)]
        static void beNotified(IntPtr notifyCode)
        {
            SCNotification nc = (SCNotification)Marshal.PtrToStructure(notifyCode, typeof(SCNotification));
            if (nc.nmhdr.code == (uint)NppMsg.NPPN_TBMODIFICATION)
            {
                PluginBase._funcItems.RefreshItems();
                Main.SetToolBarIcon();
                Main.InitNppPIALexer2();
            }
            else if (nc.nmhdr.code == (uint)NppMsg.NPPN_SHUTDOWN)
            {
                Main.PluginCleanUp();
                Marshal.FreeHGlobal(_ptrPluginName);
            }
            else
            {
                // open NppPIALexer2 file
                if ((NppMsg)nc.nmhdr.code == NppMsg.NPPN_FILEOPENED)
                {
                    StringBuilder sb = new StringBuilder(Win32.MAX_PATH);
                    if ((int)Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETFULLPATHFROMBUFFERID, (int)nc.nmhdr.idFrom, sb) != -1)
                    {
                        string path = sb.ToString();
                        if (Path.GetExtension(path).ToLower() == ".nppproj")
                        {
                            if (Main.FrmMain == null)
                                Main.ShowNppPIALexer2View();
                            Main.FrmMain.OpenProject(path);
                            NPP.CloseFile(path);
                            return;
                        }
                    }
                }


                if (Main.FrmMain == null)
                    return;

                try
                {
                    switch ((NppMsg)nc.nmhdr.code)
                    {
                        case NppMsg.NPPN_FILEOPENED:    // 项目中已打开的文件名加下划线
                            {
                                if (Main.FrmMain.Visible)
                                {
                                    StringBuilder sb = new StringBuilder(Win32.MAX_PATH);
                                    if ((int)Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETFULLPATHFROMBUFFERID, (int)nc.nmhdr.idFrom, sb) != -1)
                                    {
                                        string path = sb.ToString();
                                        Utility.UnderlineTreeNode(path);
                                    }
                                }
                                break;
                            }

                        case NppMsg.NPPN_FILEBEFORECLOSE:  // 项目文件关闭时，取消下划线
                            {
                                string file = NPP.GetCurrentFile();
                                if (Main.FrmMain.Visible)
                                    Utility.UnUnderlineTreeNode(file);
                            }
                            break;

                        case NppMsg.NPPN_BUFFERACTIVATED:   // 高亮显示当前Active文件
                            {
                                StringBuilder sb = new StringBuilder(Win32.MAX_PATH);
                                if ((int)Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETFULLPATHFROMBUFFERID, (int)nc.nmhdr.idFrom, sb) != -1)
                                {
                                    string file = sb.ToString();
                                    Utility.HighlightActiveTreeNode(file);

                                    //ProjectItem item = ProjectManager.GetProjectItemByFile(file);
                                    //if (item != null && item.Bookmarks.Count > 0)   // 设置书签
                                    //{
                                    //    foreach (Bookmark book in item.Bookmarks)
                                    //        NPP.SetBookmark(book.LineNo);
                                    //}
                                }
                            }
                            break;

                        case NppMsg.NPPN_FILESAVED: // 有文件更新保存时，更新ctag标签
                            {
                                StringBuilder sb = new StringBuilder(Win32.MAX_PATH);
                                if ((int)Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETFULLPATHFROMBUFFERID, (int)nc.nmhdr.idFrom, sb) != -1)
                                {
                                    string path = sb.ToString();
                                    int index = ProjectManager.GetProjectIndex(path);
                                    if (index != -1)
                                    {
                                        //AutoCompletionHelper.SetUpdateFlag(proj.ProjectFile);
                                        TagUpdater.Update(index, path);
                                        TaskUpdater.Update(path);
                                    }
                                }
                            }
                            break;
                    }

                    switch ((SciMsg)nc.nmhdr.code)
                    {
                        case SciMsg.SCN_CHARADDED:      // 智能提示
                        case SciMsg.SCN_AUTOCCHARDELETED:
                        //case SciMsg.SCN_AUTOCCANCELLED:
                            {
                                if (Config.Instance.AutoCompletion)
                                {
                                    Main.ShowAutoCompletion(SciMsg.SCN_AUTOCCHARDELETED == (SciMsg)nc.nmhdr.code);
                                }
                            }
                            break;
                        case SciMsg.SCN_MARGINCLICK:
                            int line=NPP.GetLineFromPosition(nc.position);
                            NPP.ToggleFold(line);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Utility.Debug("{0}: {1}", ex.Message, ex.StackTrace);
                }
            }
        }


    } // end class
}
