using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;
using System;
namespace NppPIALexer2
{
    /// <summary>
    /// 自定义命令
    /// </summary>
    public class Command
    {
        public Command(string title, string cmd, string args, string initDir)
        {
            Title = title;
            Cmd = cmd;
            Args = args;
            InitDir = initDir;
        }

        public string Title { get; set; }
        public string Cmd { get; set; }
        public string Args { get; set; }
        public string InitDir { get; set; }

        public void Execute()
        {
            Project project = (Resource.ProjectTreeView.SelectedNode.Tag as ProjectItem).Project;
            string activeFile = NPP.GetCurrentFile();
            string initDir = InitDir;
            initDir = initDir.Replace("$(ProjectDir)", project.BaseDir);
            initDir = initDir.Replace("$(ActiveFileDir)", Path.GetDirectoryName(activeFile));

            string args = Args;
            args = args.Replace("$(ProjectDir)", project.BaseDir);
            args = args.Replace("$(ProjectFile)", project.ProjectFile);
            args = args.Replace("$(ProjectName)", project.Root.Name);
            args = args.Replace("$(ActiveFile)", activeFile);
            args = args.Replace("$(ActiveFileDir)", Path.GetDirectoryName(activeFile));
            args = args.Replace("$(ActiveFileName)", Path.GetFileName(activeFile));

            Process p = new Process();
            p.StartInfo.WorkingDirectory = initDir;
            p.StartInfo.FileName = Cmd;
            p.StartInfo.Arguments = args;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            p.StartInfo.CreateNoWindow = false;
            p.StartInfo.UseShellExecute = false;
            p.Start();
        }

        const string _Spliter = "@@DARKBULL@@";
        public override string ToString()
        {
            return string.Format("{0}{4}{1}{4}{2}{4}{3}", Title, Cmd, Args, InitDir, _Spliter);
        }

        public static Command FromString(string cmdStr)
        {
            cmdStr = cmdStr.Trim();
            string[] items = Regex.Split(cmdStr, _Spliter);
            return new Command(items[0], items[1], items[2], items[3]);
        }
    }

    public class CommandManager
    {
        static List<Command> _Cmds = new List<Command>();

        public static Command[] Commands
        {
            get { return _Cmds.ToArray(); }
        }

        public static void AddCommand(string title, string cmd, string args, string initDir)
        {
            Command c = new Command(title, cmd, args, initDir);
            _Cmds.Add(c);
        }

        public static void RemoveCommand(string title)
        {
            for (int i = 0; i < _Cmds.Count; ++i)
            {
                if (title == _Cmds[i].Title)
                {
                    _Cmds.RemoveAt(i);
                    break;
                }
            }
        }

        public static void RemoveCommand(int index)
        {
            if (index < _Cmds.Count)
                _Cmds.RemoveAt(index);
        }

        public static void UpdateCommand(int index, string title, string cmd, string args, string initDir)
        {
            if (index < _Cmds.Count)
            {
                Command c = _Cmds[index];
                c.Title = title;
                c.Cmd = cmd;
                c.Args = args;
                c.InitDir = initDir;
            }
        }

        public static void Swap(int frm, int to)
        {
            Utility.Assert(0 <= frm && frm < _Cmds.Count && 0 <= to && to < _Cmds.Count && frm != to);
            Command cmd = _Cmds[frm];
            _Cmds[frm] = _Cmds[to];
            _Cmds[to] = cmd;
        }

        /// <summary>
        /// 保存外部命令
        /// </summary>
        public static void Save()
        {
            StringBuilder sb = new StringBuilder();
            foreach (Command cmd in _Cmds)
                sb.AppendFormat("{0}\n", cmd.ToString());
            File.WriteAllText(Config.Instance.ExternalToolConfigFile, sb.ToString());
        }

        /// <summary>
        /// 加载外部命令
        /// </summary>
        public static void Load()
        {
            _Cmds.Clear();
            if (File.Exists(Config.Instance.ExternalToolConfigFile))
            {
                try
                {
                    foreach (string line in File.ReadAllLines(Config.Instance.ExternalToolConfigFile))
                        _Cmds.Add(Command.FromString(line));
                }
                catch (Exception ex)
                {
                    File.Delete(Config.Instance.ExternalToolConfigFile);
                    Utility.Error("Load external tools failed: {0}", ex.Message);
                }
            }
        }
    }
}