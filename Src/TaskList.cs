using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Text;

namespace NppPIALexer2
{
    public class Task
    {
        public Task(string file, int lineNo, string info, TaskDefinition type)
        {
            File = file;
            LineNo = lineNo;
            Info = info;
            Type = type;
        }

        public string File { get; set; }
        public int LineNo { get; set; }
        public string Info { get; set; }
        public TaskDefinition Type { get; set; }
    }

    public class TaskDefinition
    {
        TaskDefinition(string name, string regex)
        {
            Name = name;
            RegexString = regex;
        }

        string _regexString;
        Regex _re;

        public string Name { get; set; }
        public string RegexString 
        {
            get { return _regexString; }
            set
            {
                Regex re = new Regex(value, RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline);
                _re = re;
                _regexString = value;
            }
        }
        public Regex Regex { get { return _re; } }

        public override string ToString()
        {
            return string.Format("{0}@@@@@DARKBULL@@@@@{1}", Name, RegexString);
        }

        static TaskDefinition()
        {
            if (File.Exists(Config.Instance.TaskDefinitionConfigFile))
            {
                foreach (string line in File.ReadAllLines(Config.Instance.TaskDefinitionConfigFile))
                {
                    string line1 = line.Trim();
                    string[] arr = line1.Split(new string[] { "@@@@@DARKBULL@@@@@"}, StringSplitOptions.RemoveEmptyEntries);
                    if (arr.Length != 2)
                        continue;
                    string name = arr[0];
                    string regex = arr[1];
                    var def = new TaskDefinition(name, regex);
                    if (_Cache.ContainsKey(def.Name.ToLower()))
                        continue;   // throw new exception?
                    _Cache[def.Name.ToLower()] = def;
                }
            }

            if (!_Cache.ContainsKey("todo"))
                _Cache["todo"] = new TaskDefinition("todo", @"[^a-zA-Z0-9_]\s*(todo\s*:[\s\S]*)");
        }

        static Dictionary<string, TaskDefinition> _Cache = new Dictionary<string, TaskDefinition>();
        static void _Save()
        {
            List<string> ret = new List<string>();
            foreach (var item in _Cache.Values)
                ret.Add(item.ToString());
            File.WriteAllLines(Config.Instance.TaskDefinitionConfigFile, ret.ToArray());
        }

        public static void Add(string name, string regex)
        {
            string key = name.ToLower();
            if (_Cache.ContainsKey(key))
                throw new Exception("TaskDefinition name exists.");
            var def = new TaskDefinition(name, regex);
            _Cache[key] = def;
            _Save();
        }

        public static void Remove(string name)
        {
            string key = name.ToLower();
            if (key == "todo")
                return;
            if (_Cache.ContainsKey(key))
            {
                _Cache.Remove(key);
                _Save();
            }
        }

        public static List<TaskDefinition> Defs
        {
            get 
            {
                var ret = new List<TaskDefinition>(_Cache.Values);
                //ret.Sort(new Comparison<TaskDefinition>(delegate(TaskDefinition a, TaskDefinition b)
                //    {
                //        if (a.Name.ToLower() == "todo")
                //            return -1;
                //        else if (b.Name.ToLower() == "todo")
                //            return 1;
                //        else
                //            return string.Compare(a.Name, b.Name);
                //    }));
                return ret;
            }
        }


        public static TaskDefinition Get(string name)
        {
            string key = name.ToLower();
            if (_Cache.ContainsKey(key))
                return _Cache[key];
            return null;
        }
    }

    public delegate void TaskChanged(string file, List<Task> tasks);
    
    public class TaskUpdater
    {
        public static event TaskChanged TaskChanged;

        public static void Work()
        {
            Thread t = new Thread(_Work);
            t.Start();
        }

        static Dictionary<string, List<Task>> _Cache = new Dictionary<string, List<Task>>();
        static Dictionary<string, int> _UpdatedFiles = new Dictionary<string, int>();
        static Dictionary<string, int> _RemovedFiles = new Dictionary<string, int>();

        static bool _IsEqual(List<Task> lst1, List<Task> lst2)
        {
            lst1.Sort(new Comparison<Task>(delegate (Task a, Task b)
                {
                    return a.LineNo - b.LineNo; 
                }));
            lst2.Sort(new Comparison<Task>(delegate(Task a, Task b)
                {
                    return a.LineNo - b.LineNo;
                }));

            for (int i = 0; i < lst1.Count; ++i)
            {
                if (lst1[i].Info != lst2[i].Info || lst1[i].LineNo != lst2[i].LineNo)
                    return false;
            }
            return true;
        }

        static void _Work()
        {
            List<string> addedFile = new List<string>();
            foreach (Project project in ProjectManager.Projects)
            {
                List<string> t = new List<string>();
                foreach (string file in project.Root.SubFiles2)
                {
                    var tasks = _GetTask(file);
                    if (tasks != null && tasks.Count > 0)
                    {
                        t.Add(file);
                        _Cache[file] = tasks;
                    }

                    // Unload items during loading
                    if (ProjectManager.GetProjectIndex(project) == -1)
                    {
                        foreach (string f in t)
                            if (_Cache.ContainsKey(file))
                                _Cache.Remove(file);
                        t.Clear();
                        break;
                    }
                }
                if (t.Count > 0)
                    addedFile.AddRange(t);
            }
            if (addedFile.Count > 0 && TaskChanged != null)
                foreach (string file in addedFile)
                    TaskChanged(file, _Cache[file]);


            while (true)
            {
                DateTime current = DateTime.Now;

                try
                {
                    Dictionary<string, int> r = null;
                    Dictionary<string, int> u = null;
                    lock (typeof(TaskUpdater))
                    {
                        r = _RemovedFiles;
                        _RemovedFiles = new Dictionary<string, int>();
                        u = _UpdatedFiles;
                        _UpdatedFiles = new Dictionary<string, int>();
                    }
                    foreach (string file in r.Keys)
                        if (_Cache.ContainsKey(file))
                            _Cache.Remove(file);

                    List<string> updatedFiles = new List<string>();
                    foreach (string file in u.Keys)
                    {
                        var tasks = _GetTask(file);
                        if (_Cache.ContainsKey(file))
                        {
                            var oldTasks = _Cache[file];
                            if (!_IsEqual(tasks, oldTasks))
                            {
                                _Cache[file] = tasks;
                                updatedFiles.Add(file);
                            }
                        }
                        else
                        {
                            _Cache[file] = _GetTask(file);
                            updatedFiles.Add(file);
                        }
                    }

                    if (TaskChanged != null)
                    {
                        foreach (string file in r.Keys)
                            TaskChanged(file, null);
                        foreach (string file in updatedFiles)
                            TaskChanged(file, _Cache[file]);
                    }
                }
                catch
                {
                }

                var d = DateTime.Now - current;
                if (d.TotalSeconds < 1)
                    Thread.Sleep((int)(1 - d.TotalSeconds) * 1000);
            }
        }

        static string[] _ExcludeExt = new string[] { ".exe", ".msi",
                ".zip", ".bz", ".7z", ".bz2", ".gz", ".rar", ".bzip2", ".tbz2", 
                ".pdf", ".doc", ".xls", ".ppt",
                ".jpg", ".jpeg", ".gif", ".png", ".bmp", 
                ".o", ".obj", ".dll", ".lib", ".pdb",
                ".ttf", ".ttc",
                ".swf", ".fla",
                ".db",
            };
        static bool _IsExclude(string file)
        {
            string ext = Path.GetExtension(file).ToLower();
            foreach (string e in _ExcludeExt)
                if (e == ext)
                    return true;
            FileInfo f = new FileInfo(file);
            if (f.Length > 1024 * 1024)
                return false;
            return false;
        }
        static List<Task> _GetTask(string file)
        {
            List<Task> ret = new List<Task>();
            if (!File.Exists(file) || _IsExclude(file))
                return ret;

            string[] lines = File.ReadAllLines(file);
            if(lines.Length > 10000)   // There's such a big source file.
                return ret;
            for (int lineno = 0; lineno < lines.Length; ++lineno)
            {
                string line = lines[lineno];
                if(line.Length > 1000) // Is there a long code line?
                    continue;
                line = line.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;
                foreach (var def in TaskDefinition.Defs)
                {
                    Match m = def.Regex.Match(line);
                    if (m.Success)
                    {
                        var task = new Task(file, lineno, m.Groups[1].Value, def);
                        ret.Add(task);
                    }
                }
            }
            return ret;
        }

        public static void Update(params string[] files)
        {
            lock (typeof(TaskUpdater))
            {
                foreach (string file in files)
                    _UpdatedFiles[file] = 1;
            }
        }

        public static void Remove(params string[] files)
        {
            lock (typeof(TaskUpdater))
            {
                foreach (string file in files)
                    _RemovedFiles[file] = 1;
            }
        }

        public static List<Task> GetTasks(params string[] files)
        {
            List<Task> ret = new List<Task>();
            lock (typeof(TaskUpdater))
            {
                foreach (string file in files)
                {
                    List<Task> tmp = null;
                    if (_Cache.TryGetValue(file, out tmp))
                        ret.AddRange(tmp);
                }
            }
            return ret;
        }
    }
}