using System.Collections.Generic;
using System;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace NppPIALexer2.Tag
{
    /// <summary>
    /// Ctag 5.8.4.SC 所支持的语言. 
    /// ctags --list-languages
    /// </summary>
    public enum Language
    {
        Ant,
        Asm,
        Asp,
        Awk,
        Basic,
        BETA,
        C,
        Cpp,
        CSharp,
        Cobol,
        DosBatch,
        Eiffel,
        Erlang,
        Flex,
        Fortran,
        HTML,
        Java,
        JavaScript,
        Lisp,
        Lua,
        Make,
        MatLab,
        OCaml,
        Pascal,
        Perl,
        PHP,
        SmartLV,
        REXX,
        Ruby,
        Scheme,
        Sh,
        SLang,
        SML,
        SQL,
        Tcl,
        Tex,
        Vera,
        Verilog,
        VHDL,
        Vim,
        YACC,

        Other,  // 其他所有无法使用ctags.exe解析的语言
    }

    /// <summary>
    /// 对应 --list-kind 
    /// 类视图里 同层次标签类型显示的顺序按该枚举值从小到大排列
    /// 注意：这里的顺序
    /// </summary>
    public enum TagType         
    {
        C_Class,                // c
        C_Macro,                // d
        C_Enumerator,           // e
        C_Function,             // f
        C_Enumeration,          // g
        C_LocalVar,             // l
        C_ClassMember,          // m
        C_Namespace,            // n
        C_FunctionPrototype,    // p
        C_Struct,               // s
        C_Typedef,              // t
        C_Union,                // u
        C_Variable,             // v
        C_Declaration,          // x

        Cpp_Declaration,        // x
        Cpp_Macro,              // d
        Cpp_Namespace,          // n
        Cpp_Class,              // c
        Cpp_Struct,             // s
        Cpp_Union,              // u
        Cpp_Enumeration,        // g
        Cpp_Enumerator,         // e
        Cpp_ClassMember,        // m
        Cpp_Function,           // f
        Cpp_LocalVar,           // l
        Cpp_FunctionPrototype,  // p
        Cpp_Typedef,            // t
        Cpp_Variable,           // v

        CSharp_Namespace,       // n
        CSharp_Macro,           // d
        CSharp_Interface,       // i
        CSharp_Enum,            // e // 枚举值
        CSharp_Class,           // c
        CSharp_Struct,          // s
        CSharp_Event,           // E
        CSharp_EnumerationName, // g // 枚举类型名
        CSharp_Field,           // f
        CSharp_Property,        // p
        CSharp_Method,          // m
        CSharp_LocalVar,        // l
        CSharp_Typedef,         // t

        Python_Import,          // i
        Python_Variable,        // v
        Python_Class,           // c
        Python_Method,          // m
        Python_Function,        // f
        Python_Field,           // F    // 正则表达式扩展

        Java_Class,             // c
        Java_EnumConstant,      // e
        Java_Field,             // f
        Java_Enum,              // g
        Java_Interface,         // i
        Java_LocalVar,          // l
        Java_Method,            // m
        Java_Package,           // p

        JavaScript_Function,    // f
        JavaScript_Class,       // c
        JavaScript_Method,      // m
        JavaScript_Property,    // p
        JavaScript_GlobalVar,   // v

        Flex_Function,          // f
        Flex_Class,             // c
        Flex_Method,            // m
        Flex_Property,          // p
        Flex_GlobalVar,         // v
        Flex_Mxtag,             // x

        PHP_Class,              // c
        PHP_Interface,          // i
        PHP_Constant,           // d
        PHP_Function,           // f
        PHP_Variable,           // v
        PHP_JsFunction,         // j
        PHP_Property,           // p    // 用正则表达式扩展的变量

        ASM_Define,             // d
        ASM_Label,              // l
        ASM_Macro,              // m
        ASM_Type,               // t

        Ruby_Module,            // m
        Ruby_Class,             // c
        Ruby_Method,            // f
        Ruby_SingletonMethod,   // F

        Pascal_Function,        // f
        Pascal_Procedure,       // p

        Other,
    }

    public enum AccessType
    {
        Public,
        Private,
        Protected,
        Internal,
        Other,
    }

    public abstract class TagParser
    {
        public abstract Language Lang { get; }
        public virtual string Args { get { return "--excmd=number --fields=aksST --sort=no"; } }
        /// <summary>
        /// --list-kinds 转换成TagType枚举
        /// </summary>
        /// <param name="sTagType"></param>
        /// <returns></returns>
        protected abstract TagType StrToTagType(string sTagType);
        public abstract ITag Parse(string line);

        protected readonly char _DefaultSpliter = (char)0x01; // '☺'

        #region Helper method

        protected T _2Enum<T>(string str) { return (T)Enum.Parse(typeof(T), str, true); }

        protected string _Get(string[] fields, Regex re, int index)
        {
            for (int i = fields.Length - 1; i >= 0; --i)
            {
                var m = re.Match(fields[i]);
                if (m.Success)
                    return m.Groups[index].Value;
            }
            return "";
        }

        Regex _reBelongTo = new Regex(@"^([a-z]):([a-zA-Z0-9_\.\:\$]+)$", RegexOptions.Compiled & RegexOptions.Singleline);
        protected string _GetBelongTo(string[] fields){ return _Get(fields, _reBelongTo, 2); }

        Regex _reSignature = new Regex(@"^signature:(\([[\s\S]*\))$", RegexOptions.Compiled & RegexOptions.Singleline);
        protected string _GetSignature(string[] fields) { return _Get(fields, _reSignature, 1); }

        Regex _reReturnType = new Regex(@"^^returntype:([[\s\S]*)$", RegexOptions.Compiled & RegexOptions.Singleline);
        protected string _GetReturnType(string[] fields) { return _Get(fields, _reReturnType, 1); }

        Regex _reAccessType = new Regex(@"^^access:(public|private|protected|internal)$", RegexOptions.Compiled & RegexOptions.Singleline);
        protected AccessType _GetAccessType(string[] fields)
        {
            string s = _Get(fields, _reAccessType, 1);
            if (string.IsNullOrEmpty(s))
                return AccessType.Other;
            else
                return _2Enum<AccessType>(s);
        }

        Regex _reLineNo = new Regex(@"^^line:(\d+)$", RegexOptions.Compiled & RegexOptions.Singleline);
        protected int _GetLineNo(string[] fields)
        {
            string s = _Get(fields, _reLineNo, 1);
            if (string.IsNullOrEmpty(s))
                return -1;
            else
                return int.Parse(s);
        }

        /// <summary>
        /// General analytical method
        /// </summary>
        /// <param name="line">ctags行</param>
        /// <param name="tagName"></param>
        /// <param name="file"></param>
        /// <param name="lineNo"></param>
        /// <param name="tagType"></param>
        /// <param name="belongTo"></param>
        /// <param name="accessType"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        protected bool _Parse(string line, out string tagName, out string file, out int lineNo, out TagType tagType, out string belongTo, out AccessType accessType, out string signature)
        {
            tagName = file = belongTo = signature = "";
            lineNo = -1;
            tagType = TagType.Other;
            accessType = AccessType.Other;
            if (line.StartsWith("!"))
                return false;

            string[] fields = line.Split(_DefaultSpliter);
            tagName = fields[0];
            file = fields[1];
            string sLineNo = fields[2];
            lineNo = int.Parse(sLineNo.Split(';')[0]);
            string sTagType = fields[3];
            tagType = StrToTagType(sTagType);

            belongTo = _GetBelongTo(fields);
            accessType = _GetAccessType(fields);
            signature = _GetSignature(fields);
            string returnType = _GetReturnType(fields);
            if (!string.IsNullOrEmpty(returnType))
                signature += ":" + returnType;
            return true;
        }

        #endregion

        static Dictionary<Language, TagParser> _Parsers = new Dictionary<Language, TagParser>();
        public static Dictionary<string, Language> Ext2Lang = new Dictionary<string, Language>();
        static readonly string _CtagPath = Path.Combine(Config.Instance.NppPIALexer2Dir, "ctags.exe");
        static readonly string _TempFile;   // 临时文件

        static TagParser()
        {
            // 临时文件路径
            _TempFile = Path.GetTempFileName();
            File.Delete(_TempFile);
        }

        /// <summary>
        /// 插件加载时，先初始化解析器，再加载 后缀->语言的映射表
        /// 将不支持的语言后缀映射删除
        /// </summary>
        internal static void LoadExt2LangMapping()
        {
            Ext2Lang.Clear();
            string ext2LangMapFile = Path.Combine(Config.Instance.NppPIALexer2Dir, "LanguageMap.txt");
            if (!File.Exists(ext2LangMapFile))
            {
                string stdout = _ExecuteCtag("--list-maps");
                stdout = stdout.ToLower();
                stdout = stdout.Replace("c++", "cpp");
                stdout = stdout.Replace("c#", "csharp");
                stdout = stdout.Replace("*", "");

                foreach (string line in stdout.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] tokens = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    Language lang = (Language)Enum.Parse(typeof(Language), tokens[0], true);
                    for (int i = 1; i < tokens.Length; ++i)
                    {
                        if (tokens[i].IndexOf('[') == -1) // process []
                            Ext2Lang[tokens[i].ToLower()] = lang;
                    }
                }
                SaveExt2LanguageMap();
            }
            
            foreach (string line in File.ReadAllLines(ext2LangMapFile))
            {
                try
                {
                    string[] arr = line.Split('|');
                    string ext = arr[0];
                    Language lang = (Language)Enum.Parse(typeof(Language), arr[1], true);
                    bool supported = false;
                    foreach (Language l in _Parsers.Keys)
                        if (l == lang)
                        {
                            supported = true;
                            break;
                        }
                    if (supported)
                        Ext2Lang[ext] = lang;
                }
                catch { }
            }

            if (Ext2Lang.Count == 0)
            {
                File.Delete(ext2LangMapFile);
                Utility.Error("Loading language mapping file failed.");
            }
        }

        public static void Register(TagParser parser)
        {
            if (_Parsers.ContainsKey(parser.Lang))
                throw new Exception(string.Format("{0} parser existed.", parser.Lang.ToString()));
            _Parsers[parser.Lang] = parser;
        }

        public static void UnRegister(Language lang)
        {
            if (_Parsers.ContainsKey(lang))
                _Parsers.Remove(lang);
        }

        /// <summary>
        /// 获取当前支持解析的语言
        /// </summary>
        public static List<Language> GetSupportedLanguage
        {
            get { return new List<Language>(_Parsers.Keys); }
        }

        /// <summary>
        /// 保存 语言-文件后缀 的映射
        /// </summary>
        public static void SaveExt2LanguageMap()
        {
            List<string> lst = new List<string>();
            foreach (string key in Ext2Lang.Keys)
                lst.Add(string.Format("{0}|{1}", key, Ext2Lang[key]));
            string ext2LangMapFile = Path.Combine(Config.Instance.NppPIALexer2Dir, "LanguageMap.txt");
            File.WriteAllLines(ext2LangMapFile, lst.ToArray());
        }
        
        /// <summary>
        /// 获取源文件对应的语言
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Language GetDefaultLang(string file)
        {
            string ext = Path.GetExtension(file).ToLower();
            if (string.IsNullOrEmpty(ext))
                return Language.Other;
            if (Ext2Lang.ContainsKey(ext))
                return Ext2Lang[ext];
            else
                return Language.Other;
        }

        /// <summary>
        /// Force the use of a language to parse the source file to generate tags
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="sourceFiles"></param>
        /// <returns></returns>
        public static List<ITag> Parse(Language lang, params string[] sourceFiles)
        {
            if (sourceFiles.Length == 0)
                return new List<ITag>();
            if (!_Parsers.ContainsKey(lang))
            {
                return new List<ITag>();
            }

            if (File.Exists(_TempFile))
                File.Delete(_TempFile);
            TagParser parser = _Parsers[lang];
            string args = parser.Args;
            {
                string language = lang.ToString();
                if (lang == Language.Cpp)
                    language = "C++";
                else if (lang == Language.CSharp)
                    language = "C#";
                else
                    language = lang.ToString();

                if (args.ToLower().IndexOf("--language-force") == -1)
                    args += " --language-force=" + language;
            }
            StringBuilder sb = new StringBuilder();
            foreach (string file in sourceFiles)
                sb.AppendFormat("\"{0}\" ", file);
            args = string.Format("{0} -f \"{1}\"  {2}", args, _TempFile, sb.ToString());
            string stdout = _ExecuteCtag(args);
            if (!File.Exists(_TempFile))
                return new List<ITag>();

            List<ITag> lst = new List<ITag>();
            foreach (string line in File.ReadAllLines(_TempFile))
            {
                ITag tag = parser.Parse(line);
                if (tag != null)
                    lst.Add(tag);
            }
            File.Delete(_TempFile);
            return lst;
        }

        /// <summary>
        /// Resolve the source file according to the file extension name
        /// </summary>
        /// <param name="sourceFiles"></param>
        /// <returns></returns>
        public static List<ITag> Parse(params string[] sourceFiles)
        {
            if (sourceFiles.Length == 0)
                return new List<ITag>();

            // 同类型语言的源文件放在一起
            Dictionary<Language, List<string>> temp = new Dictionary<Language, List<string>>();
            foreach (string file in sourceFiles)
            {
                Language lang = GetDefaultLang(file);
                if (lang == Language.Other)
                    continue;

                if (!temp.ContainsKey(lang))
                    temp[lang] = new List<string>();
                temp[lang].Add(file);
            }

            List<ITag> lst = new List<ITag>();
            foreach (Language lang in temp.Keys)
                lst.AddRange(Parse(lang, temp[lang].ToArray()));
            return lst;
        }

        static string _ExecuteCtag(string args)
        {
            using (Process p = new Process())
            {
                string tagsFile = Path.GetTempFileName();
                p.StartInfo.FileName = _CtagPath;
                p.StartInfo.Arguments = args;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.StandardOutputEncoding = Encoding.Default;
                p.Start();
                p.WaitForExit();
                string stdout = p.StandardOutput.ReadToEnd();
                if (p.ExitCode != 0)
                    throw new Exception(string.Format("Excute ctags.exe failed: {0}", stdout));
                return stdout;
            }
        }

    }


    public abstract class ITag
    {
        string _srcFile;
        int _lineNo;
        string _tagName;
        TagType _tagType;
        AccessType _accessType;
        string _belongTo;
        string _signature;

        public ITag(string srcFile, int lineNo, string tagName, TagType tagType, AccessType accessType, string belongTo, string signature)
        {
            _srcFile = srcFile;
            _lineNo = lineNo;
            _tagName = tagName;
            _tagType = tagType;
            _accessType = accessType;
            _belongTo = belongTo;
            _signature = signature;
        }

        public abstract Language Lang { get; }
        /// <summary>
        /// 绑定到TreeNode时，设置TreeNode的属性。如果返回false，则不绑定到类浏览视图
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public abstract bool BindToTreeNode(TreeNode node);

        public virtual string SourceFile { get { return _srcFile; } }
        /// <summary>
        /// 所在行号（从1开始）
        /// </summary>
        public virtual int LineNo { get { return _lineNo; } }
        public virtual string TagName { get { return _tagName; } }
        public virtual TagType TagType { get { return _tagType; } }
        public virtual AccessType AccessType { get { return _accessType; } }
        public virtual string BelongTo { get { return _belongTo; } }
        public virtual string Signature { get { return _signature; } }
        public virtual string FullName { get { return string.IsNullOrEmpty(BelongTo) ? TagName : string.Format("{0}.{1}", BelongTo, TagName); } }

    }
}