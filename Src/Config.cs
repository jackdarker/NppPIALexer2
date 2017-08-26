using System.Collections.Generic;
using System.Xml;
using System;
using System.Text;
using NppPluginNET;
using System.IO;
namespace NppPIALexer2
{
    /// <summary>
    /// 项目配置类
    /// </summary>
    public class Config
    {
        static Config _Config;  // singleton
        string _configDir;
        string _npppDir;    // NppPIALexer2使用的文件夹
        string _configFile;
        string _templateDir;    // 模板使用的文件

        //启动时需要打开的项目
        List<string> _projects;

        public static Config Instance
        {
            get
            {
                if (_Config == null)
                {
                    _Config = new Config();
                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(_Config.ConfigFile);
                        XmlElement nppprojNode = doc.GetElementsByTagName("NppPIALexer2")[0] as XmlElement;
                        _Config.Visible = bool.Parse(nppprojNode.GetAttribute("visible"));
                        _Config.AutoCompletion = bool.Parse(nppprojNode.GetAttribute("autoCompletion"));
                        //_Config.AutoCompletionIgnoreCase = bool.Parse(nppprojNode.GetAttribute("autoCompletionIgnoreCase"));
                        _Config.AutoApplyTemplate = bool.Parse(nppprojNode.GetAttribute("autoApplyTemplate"));
                        _Config.AutoApplyTemplateLangs = nppprojNode.GetAttribute("autoApplyTemplateLangs");  // "0,3,5,20"
                        foreach (XmlElement projNode in nppprojNode.GetElementsByTagName("project"))
                            _Config.Projects.Add(projNode.GetAttribute("path"));
                        // sort the project by name
                        _Config.Projects.Sort(new Comparison<string>(delegate(string a, string b)
                        {
                            return string.Compare(a, b);
                        }));
                    }
                    catch
                    {
                    }
                }
                return _Config;
            }
        }



        protected Config()
        {
            Visible = false;
            AutoCompletion = true;
            AutoApplyTemplateLangs = "";
            AutoApplyTemplate = true;

            StringBuilder sb = new StringBuilder(Win32.MAX_PATH);
            Win32.SendMessage(PluginBase.nppData._nppHandle, NppMsg.NPPM_GETPLUGINSCONFIGDIR, Win32.MAX_PATH, sb);
            _configDir = sb.ToString();
            if (!Directory.Exists(_configDir))
                Directory.CreateDirectory(_configDir);
            char pathSpliter = _configDir.IndexOf('\\') == -1 ? '/' : '\\';
            int t = _configDir.LastIndexOf(pathSpliter);
            if (t == _configDir.Length - 1)
                t = _configDir.Substring(0, _configDir.Length - 1).LastIndexOf(pathSpliter);
            string pluginDir = _configDir.Substring(0, t);
            _npppDir = Path.Combine(pluginDir, "NppPIALexer2");
            if (!Directory.Exists(_npppDir))
                Directory.CreateDirectory(_npppDir);
            _templateDir = Path.Combine(_npppDir, "Templates");
            if (!Directory.Exists(_templateDir))
                Directory.CreateDirectory(_templateDir);
            _configFile = Path.Combine(_configDir, "NppPIALexer2.xml");
            ExternalToolConfigFile = Path.Combine(_npppDir, "ExternalTools.txt");
            TaskDefinitionConfigFile = Path.Combine(_npppDir, "TaskDefinition.txt");
            _projects = new List<string>();
        }

        /// <summary>
        /// 启动时是否显示NppPIALexer2窗口
        /// </summary>
        public bool Visible
        {
            get;
            set;
        }

        /// <summary>
        /// 是否启用智能提示
        /// </summary>
        public bool AutoCompletion
        {
            get;
            set;
        }

        ///Notepad++的AutoComplete区分大小
        //public bool AutoCompletionIgnoreCase
        //{
        //    get;
        //    set;
        //}

        public bool AutoApplyTemplate
        {
            get;
            set;
        }

        public string AutoApplyTemplateLangs
        {
            get;
            set;
        }

        public List<string> Projects
        {
            get { return _projects; }
        }

        public string ConfigDir
        {
            get { return _configDir; }
        }

        public string NppPIALexer2Dir
        {
            get { return _npppDir; }
        }

        public string TemplateDir
        {
            get { return _templateDir; }
        }

        public string TagDir
        {
            get { return Path.Combine(NppPIALexer2Dir, "Tags"); }
        }

        public string ConfigFile
        {
            get { return _configFile; }
        }

        public string ExternalToolConfigFile
        {
            get;
            set;
        }

        public string TaskDefinitionConfigFile
        {
            get;
            set;
        }

        public void Save()
        {
            XmlDocument doc = new XmlDocument();
            XmlElement nppprojNode = doc.CreateElement("NppPIALexer2");
            nppprojNode.SetAttribute("visible", Main.FrmMain == null ? "False" : Main.FrmMain.Visible.ToString());
            nppprojNode.SetAttribute("autoCompletion", AutoCompletion.ToString());
            nppprojNode.SetAttribute("autoApplyTemplate", AutoApplyTemplate.ToString());
            nppprojNode.SetAttribute("autoApplyTemplateLangs", AutoApplyTemplateLangs);
            foreach (var project in ProjectManager.Projects)
            {
                XmlElement projNode = doc.CreateElement("project");
                projNode.SetAttribute("path", project.ProjectFile);
                nppprojNode.AppendChild(projNode);
            }
            doc.AppendChild(nppprojNode);
            File.WriteAllText(_configFile, doc.OuterXml);
        }
    }
}