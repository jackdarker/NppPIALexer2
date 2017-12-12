using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace NppPIALexer2 {
    /// <summary>
    /// the parser implements 
    /// a visitor inspects every Token in a Tree and converts them to a list of Cmds
    /// it also checks for all kind of errors like assigning a bool to an int, colliding functionnames,...
    /// </summary>
    public class Parser2 : Tokenizer.NodeBuilder {
        protected String m_Project="";
        protected ModelDocument m_Model;
        protected Context m_Context;
        private String m_Scope;
        private bool m_IsClassDef;
        bool m_IsRoot = false;

        public Parser2(ModelDocument Model, String Project):base() {
            m_Project = Project;
            m_Model = Model;
            m_Context = new Context();
            m_Evaluators.AddLast(new CmdDecl());
            m_Evaluators.AddLast(new CmdFunctionDecl());
            m_Evaluators.AddLast(new CmdUsing());
            m_Evaluators.AddLast(new CmdInclude());
            m_Evaluators.AddLast(new CmdComment());
            m_Evaluators.AddLast(new CmdInvalid());
        }
        //////////////////////////////////////////////////
#region Rulechecks
        /// <summary>
        /// 
        /// </summary>
        public class RuleWrongVariableType { 
        }
#endregion
        /// ///////////////////////////////////////////////
#region Cmds
        abstract public class CmdBase {
            protected int m_StartPos;
            protected int m_Length;
            public String m_Descr="";
            public String m_Error="";
            abstract public Boolean TryParse(Parser2.Context Context, Tokenizer.Token Token);
            abstract public String AsText();
            abstract public CmdBase Copy();
            public CmdBase() { }
            public CmdBase(CmdBase CopyThis) {
                m_StartPos = CopyThis.m_StartPos;
                m_Length = CopyThis.m_Length;
                m_Descr = CopyThis.m_Descr;
                m_Error = CopyThis.m_Error;
            }
            public bool HasError() {
                return !m_Error.Equals("");
            }
            public int StartPos() { return m_StartPos; }
            public int Length() { return m_Length; }
            public String Description() { return m_Descr; }
        }
        public class CmdInvalid : CmdBase {
            public CmdInvalid(): base() {
            }
            public CmdInvalid(CmdInvalid CopyThis)
                : base(CopyThis) {
            }
            public override CmdBase Copy() {
                return new CmdInvalid(this);
            }
            /// <summary>
            /// returns true if the token was successfully converted to Cmd
            /// even if there are errors in the cmd
            /// </summary>
            /// <param name="Context"></param>
            /// <param name="Token"></param>
            /// <returns></returns>
            override public Boolean TryParse(Context Context, Tokenizer.Token Token) {
                this.m_Error = "unknown Cmd " + Token.GetValue(true);
                return true;
            }
            override public String AsText() {
                return m_Error;
            }
        }
        /// <summary>
        /// #include declaration
        /// </summary>
        public class CmdInclude : CmdBase {
            public String m_Path = "";
            public CmdInclude()
                : base() {
            }
            public CmdInclude(CmdInclude CopyThis)
                : base(CopyThis) {
                    m_Path = CopyThis.m_Path;
            }
            public override CmdBase Copy() {
                return new CmdInclude(this);
            }
            override public Boolean TryParse(Context Context, Tokenizer.Token Token) {
                m_Error = "";
                Boolean _Ret = true;
                if (Token.GetTopNodeType().Equals(typeof(Tokenizer.RuleInclude))) {
                    this.m_StartPos = Token.GetPosStart();
                    this.m_Length = Token.GetPosEnd() - Token.GetPosStart();
                    LinkedList<Tokenizer.Token>.Enumerator m_Subs = Token.GetEnumerator();
                    while (m_Subs.MoveNext()) {
                        if (m_Subs.Current.GetNodeType().Equals(typeof(Tokenizer.RuleString))) {
                            this.m_Path = m_Subs.Current.GetValue(false);
                            m_Path=m_Path.Replace("\"", "");
                            break;
                        }
                        this.m_Length = m_Subs.Current.GetPosEnd() - Token.GetPosStart();
                    }
                    if (this.m_Path.Equals("\"\"")) {
                        Context.AddLog(1, " missing path", this);
                    }
                    
                    return _Ret;
                } else {
                    return false;
                }
            }
            override public String AsText() {
                return "Include of " + m_Path;
            }
        }
        /// <summary>
        /// using declaration
        /// </summary>
        public class CmdComment : CmdBase {
            public String m_Comment = "";
            public CmdComment()
                : base() {
            }
            public CmdComment(CmdComment CopyThis)
                : base(CopyThis) {
                    m_Comment = CopyThis.m_Comment;
            }
            public override CmdBase Copy() {
                return new CmdComment(this);
            }
            override public Boolean TryParse(Context Context, Tokenizer.Token Token) {
                m_Error = "";
                Boolean _Ret = true;
                if (Token.GetTopNodeType().Equals(typeof(Tokenizer.RuleComment))) {
                    this.m_StartPos = Token.GetPosStart();
                    this.m_Length = Token.GetPosEnd() - Token.GetPosStart();
                    m_Comment = Token.GetValue(false);
                    return _Ret;
                } else {
                    return false;
                }
            }
            override public String AsText() {
                return m_Comment;
            }
        }
        /// <summary>
        /// using declaration
        /// </summary>
        public class CmdUsing : CmdBase {
            public String m_Path = "";
            public String m_Name = "";
            Regex m_RegEx;
            public CmdUsing()
                : base() {
                    m_RegEx = new Regex("(\"[^\r]*:)([^\r]*)(.[^\r]*\")", RegexOptions.Singleline);
            }
            public CmdUsing(CmdUsing CopyThis)
                : base(CopyThis) {
                m_Path = CopyThis.m_Path;
                m_Name = CopyThis.m_Name;
            }
            public override CmdBase Copy() {
                return new CmdUsing(this);
            }
            override public Boolean TryParse(Context Context, Tokenizer.Token Token) {
                m_Error = "";
                Boolean _Ret = true;
                if (Token.GetTopNodeType().Equals(typeof(Tokenizer.RuleUsing))) {
                    LinkedList<Tokenizer.Token>.Enumerator m_Subs = Token.GetEnumerator();
                    this.m_StartPos = Token.GetPosStart();
                    this.m_Length = Token.GetPosEnd() - Token.GetPosStart();
                    while (m_Subs.MoveNext()) {
                        if (m_Subs.Current.GetNodeType().Equals(typeof(Tokenizer.RuleString))) {
                            this.m_Path = m_Subs.Current.GetValue(false);
                            m_Path = m_Path.Replace("\"", "");
                            //"UserManager.lvlibp:UserManager.lvclass"   -> Name=UserManager
                            Match m = m_RegEx.Match(this.m_Path, 0);
                            if(m.Success && m.Groups.Count == 4) {
                                this.m_Name = m.Groups[2].Value;
                            } else {
                                this.m_Error += " invalid Path";
                            }
                        }
                        if (m_Subs.Current.GetNodeType().Equals(typeof(Tokenizer.RuleRegex))) {  //optional Name is hidden behind "as"
                            this.m_Name = m_Subs.Current.First.Value.GetValue(false);
                        }
                        this.m_Length = m_Subs.Current.GetPosEnd() - Token.GetPosStart();
                    }
                    
                    if (this.m_Path.Equals("")) {
                        this.m_Error += " missing path";
                    }
                    
                    return _Ret;
                } else {
                    return false;
                }
            }
            override public String AsText() {
                return "using of " +m_Path+ " as "+m_Name;
            }
        }
        /// <summary>
        /// a declaration of a variable
        /// </summary>
        public class CmdDecl: CmdBase {
            public String m_Type="";
            public String m_Name="";
            public CmdDecl()
                : base() {
            }
            public CmdDecl(CmdDecl CopyThis):base(CopyThis) {
                m_Name = CopyThis.m_Name;
                m_Type = CopyThis.m_Type;
            }
            public override CmdBase Copy() {
                return new CmdDecl(this);
            }
            override public Boolean TryParse(Context Context, Tokenizer.Token Token) {
                m_Error = "";
                Boolean _Ret = true;
                if (Token.GetTopNodeType().Equals(typeof(Tokenizer.RuleParamDecl)) ||
                    Token.GetTopNodeType().Equals(typeof(Tokenizer.RuleDecl))||
                    Context.m_ActualCmd.Equals(typeof(Tokenizer.RuleDecl)) ||
                    Token.GetTopNodeType().Equals(typeof(Tokenizer.RuleRetDecl))) {

                    if (Token.GetNodeType().Equals(typeof(Tokenizer.RuleBaseType))) {
                        this.m_Type = Token.GetValue(false);
                        this.m_StartPos = Token.GetPosStart();
                        this.m_Length = Token.GetPosEnd() - Token.GetPosStart();
                    }
                    LinkedList<Tokenizer.Token>.Enumerator m_Subs = Token.GetEnumerator();
                    while (m_Subs.MoveNext()) {
                        if (m_Subs.Current.GetNodeType().Equals(typeof(Tokenizer.RuleBaseType))) {
                            this.m_Type = m_Subs.Current.GetValue(false);
                            this.m_StartPos = m_Subs.Current.GetPosStart();
                            this.m_Length = m_Subs.Current.GetPosEnd() - m_Subs.Current.GetPosStart();
                        }
                        if (m_Subs.Current.GetNodeType().Equals(typeof(Tokenizer.RuleName))) {
                            this.m_Name = m_Subs.Current.GetValue(false);
                            break;
                        }
                        this.m_Length = m_Subs.Current.GetPosEnd() - Token.GetPosStart();
                    }
                    if (this.m_Name.Equals("") && !Token.GetTopNodeType().Equals(typeof(Tokenizer.RuleRetDecl))) {
                        this.m_Error += " missing name";
                    }
                    //Todo missing name
                    
                    return _Ret;
                } else {
                    return false;
                }
            }
            override public String AsText() {
                return "Declaration of "+m_Name+" as "+m_Type+" at Pos="+m_StartPos.ToString();
            }
        }
        /// <summary>
        /// a complete Functiondeclaration with Parameter- and Return-declaration
        /// </summary>
        public class CmdFunctionDecl : CmdBase {
            public String m_Name="";
            public LinkedList<CmdDecl> m_Params = new LinkedList<CmdDecl>();
            public LinkedList<CmdDecl> m_Returns = new LinkedList<CmdDecl>();
            public LinkedList<CmdDecl> m_Vars = new LinkedList<CmdDecl>();
            public override CmdBase Copy() {
                return new CmdFunctionDecl(this);
            }
            public CmdFunctionDecl():base() {}
            public CmdFunctionDecl(CmdFunctionDecl CopyThis)
                : base(CopyThis) {
                m_Name = CopyThis.m_Name;
                m_Params = new LinkedList<CmdDecl>(CopyThis.m_Params);
                m_Returns = new LinkedList<CmdDecl>(CopyThis.m_Returns);
                m_Vars = new LinkedList<CmdDecl>(CopyThis.m_Vars);
            }
            override public Boolean TryParse(Context Context, Tokenizer.Token Token) {
                m_Error = "";
                Boolean _Ret = true;
                if (Context.m_ActualCmd.Equals(typeof(Tokenizer.RuleFunctionDecl))) {
                    LinkedList<Tokenizer.Token>.Enumerator m_Subs = Token.GetEnumerator();
                    this.m_StartPos = Token.GetPosStart();
                    this.m_Length = Token.GetPosEnd() - Token.GetPosStart();
                    bool _ParseBody = false;
                    while (m_Subs.MoveNext()) {
                        if (m_Subs.Current.GetNodeType().Equals(typeof(Tokenizer.RuleName))) {
                            this.m_Name = m_Subs.Current.GetValue(false);
                        }
                        if (m_Subs.Current.GetNodeType().Equals(typeof(Tokenizer.RuleParamDecl))) {
                            this.m_Params = ParseParams(Context,m_Subs.Current.GetEnumerator(),null);  //this is the first node in the params fe. string from (string sText,int Wolf))
                        }
                        if (m_Subs.Current.GetNodeType().Equals(typeof(Tokenizer.RuleRetDecl))) {
                            this.m_Returns = ParseReturns(Context, m_Subs.Current.GetEnumerator(), null);  //this is the first node in the params fe. string from (string sText,int Wolf))
                        }
                        this.m_Length = m_Subs.Current.GetPosEnd() - Token.GetPosStart();   //length without following body
                        if (_ParseBody==true) {
                            if (!m_Subs.Current.GetNodeType().Equals(typeof(Tokenizer.RuleRCurlPar))) {
                                this.m_Vars = ParseLocals(Context, m_Subs.Current,null);
                            }
                        }
                        if (m_Subs.Current.GetNodeType().Equals(typeof(Tokenizer.RuleLCurlPar))) {
                            _ParseBody = true; //The next token is either a body-line or }
                        }
                    }
                    //Todo missing name
                  
                    return _Ret;
                } else {
                    return false;
                }
            }
            static LinkedList<CmdDecl> ParseLocals(Context Context, Tokenizer.Token m_Params, LinkedList<CmdDecl> ListIn) {
                if (ListIn == null) {
                    ListIn = new LinkedList<CmdDecl>();
                }

                LinkedList<Tokenizer.Token>.Enumerator Params = m_Params.GetEnumerator();
                while (Params.MoveNext()) {
                    if (Params.Current.GetTopNodeType().Equals(typeof(Tokenizer.RuleDecl))) {
                        CmdDecl x = new CmdDecl();
                        if (x.TryParse(Context, Params.Current)) {
                            ListIn.AddLast(x);
                        }
                        //more Params - go deeper into the tree
                        //Todo
                        /* LinkedList<Tokenizer.Token>.Enumerator ParamsNext = Params.Current.GetEnumerator();
                         if (ParamsNext.MoveNext()) {    //Todo ??
                             ListIn = ParseLocals(Context, ParamsNext.Current, ListIn);
                         }*/
                        // TODO else Param is missing
                    }
                    //Console.WriteLine(Params.Current.GetValue(false));
                }
                return ListIn;
            }
            /// <summary>
            /// Parses a Return-List by Recursion
            /// </summary>
            /// <param name="m_Params">Start-Node of the Return-List</param>
            /// <param name="ListIn">Null</param>
            /// <returns></returns>
            static LinkedList<CmdDecl> ParseReturns(Context Context, LinkedList<Tokenizer.Token>.Enumerator m_Params, LinkedList<CmdDecl> ListIn) {
                if (ListIn == null) {
                    ListIn = new LinkedList<CmdDecl>();
                }
                //this tree starts with ->
                //LinkedList<Tokenizer.Token>.Enumerator Params = m_Params.GetEnumerator();
                while (m_Params.MoveNext()) {
                    if (m_Params.Current.GetNodeType().Equals(typeof(Tokenizer.RuleBaseType))) {
                        CmdDecl _Decl = new CmdDecl();
                        _Decl.TryParse(Context, m_Params.Current);
                        ListIn.AddLast(_Decl);
                        ListIn = ParseReturns(Context, m_Params.Current.GetEnumerator(), ListIn);
                    } else if (m_Params.Current.GetNodeType().Equals(typeof(Tokenizer.RuleMultiple)) ||
                         m_Params.Current.GetNodeType().Equals(typeof(Tokenizer.RuleSeparator))) {
                        //more Params - go deeper into the tree
                        ListIn = ParseParams(Context, m_Params.Current.GetEnumerator(), ListIn);
                        // TODO else Param is missing
                    }
                }
                return ListIn;
            }
            /// <summary>
            /// Parses a Param-List by Recursion
            /// </summary>
            /// <param name="m_Params">Start-Node of the Params-List</param>
            /// <param name="ListIn">Null</param>
            /// <returns></returns>
            static LinkedList<CmdDecl> ParseParams(Context Context, LinkedList<Tokenizer.Token>.Enumerator m_Params, LinkedList<CmdDecl> ListIn) {
                if (ListIn == null) {
                    ListIn = new LinkedList<CmdDecl>();
                }
                //this tree starts with the first BaseType
                while (m_Params.MoveNext()) {
                    if (m_Params.Current.GetNodeType().Equals(typeof(Tokenizer.RuleBaseType))) {
                        CmdDecl _Decl = new CmdDecl();
                        _Decl.TryParse(Context, m_Params.Current);
                        ListIn.AddLast(_Decl);
                        ParseParams(Context, m_Params.Current.GetEnumerator(), ListIn);
                    } else if (m_Params.Current.GetNodeType().Equals(typeof(Tokenizer.RuleMultiple)) ||
                        m_Params.Current.GetNodeType().Equals(typeof(Tokenizer.RuleSeparator))) {
                        //more Params - go deeper into the tree
                        ListIn = ParseParams(Context, m_Params.Current.GetEnumerator(), ListIn);
                        // TODO else Param is missing
                    }
                }
                return ListIn;
            }
            static String ParamsAsText(LinkedList<CmdDecl> Params) {
                LinkedList<CmdDecl>.Enumerator _Iter = Params.GetEnumerator();
                String Text="";
                while (_Iter.MoveNext()) {
                    Text += _Iter.Current.AsText()+",";
                }
                return Text;
            }
            override public String AsText(){
                return "Declaration of function " + m_Name + " with Params " + ParamsAsText(m_Params) + 
                    " with Returns " + ParamsAsText(m_Returns) + " with Locals " +ParamsAsText(m_Vars);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /* unused public class CmdFuncBody : CmdBase {
            public CmdFuncBody()
                : base() {
            }
            public CmdFuncBody(CmdFuncBody CopyThis)
                : base(CopyThis) {
            }
            public override CmdBase Copy() {
                return new CmdFuncBody(this);
            }
            LinkedList<CmdDecl> m_Locals = new LinkedList<CmdDecl>();
            override public Boolean TryParse(Context Context, Tokenizer.Token Token) {
                m_Error = "";
                Boolean _Ret = true;
                m_Locals = ParseLocals(Context, Token, null);
                return _Ret;
            }
            override public String AsText() {
                LinkedList<CmdDecl>.Enumerator x= m_Locals.GetEnumerator();
                string text = "local Vars: ";
                while(x.MoveNext()) {
                    text += x.Current.AsText() + ", ";
                }
                return text;
            }
            static LinkedList<CmdDecl> ParseLocals(Context Context, Tokenizer.Token m_Params, LinkedList<CmdDecl> ListIn) {
                if (ListIn == null) {
                    ListIn = new LinkedList<CmdDecl>();
                }

                LinkedList<Tokenizer.Token>.Enumerator Params = m_Params.GetEnumerator();
                while (Params.MoveNext()) {
                    if (Params.Current.GetTopNodeType().Equals(typeof(Tokenizer.RuleDecl))) {
                        CmdDecl x = new CmdDecl();
                        if (x.TryParse(Context, Params.Current)) {
                            ListIn.AddLast(x);
                        }
                        //more Params - go deeper into the tree
                        //Todo
                        // TODO else Param is missing
                    }
                    //Console.WriteLine(Params.Current.GetValue(false));
                }
                return ListIn;
            }
        }*/
#endregion
        

            public class Context {
                protected Dictionary<String, LinkedList<CmdBase>> m_Cmds = new Dictionary<String, LinkedList<CmdBase>>();
                public Context() {
                }
                public System.Type m_ActualCmd;
                public void AddCmd(String Scope,CmdBase Cmd) {
                    if(m_Cmds.ContainsKey(Scope)) {
                    } else {
                        m_Cmds.Add(Scope, new LinkedList<CmdBase>());
                    }
                    m_Cmds[Scope].AddLast(Cmd);
                }
                public LinkedList<CmdBase> GetCmds(String Scope) {
                    if(m_Cmds.ContainsKey(Scope)) {
                        return m_Cmds[Scope];
                    } else {
                        return null;
                    }
                }
                public void ResetCmds(String Scope) {
                    if(m_Cmds.ContainsKey(Scope)) {
                        m_Cmds.Remove(Scope);
                    }
                }
                public List<String> GetScopes(){
                    return m_Cmds.Keys.ToList();
                }
                public struct Log {
                    public CmdBase m_Cmd;
                    public int m_Error;
                    public String m_Text;
                }
                protected LinkedList<Log> m_Logs = new LinkedList<Log>();
                public void AddLog(int Error, String Text, CmdBase Cmd) {
                    Log x = new Log();
                    x.m_Error = Error;
                    x.m_Text = Text;
                    x.m_Cmd = Cmd;
                    m_Logs.AddLast(x);
                }
                public LinkedList<Log> GetLogs() {
                    return m_Logs;
                }
            }
            protected LinkedList<CmdBase> m_Evaluators = new LinkedList<CmdBase>();

            /// <summary>
            /// this needs to be called before processing next file token
            /// </summary>
            /// <param name="Scope"></param>
            public void ResetState(String filePath) {
                m_IsRoot = true;
                m_Scope = filePath;
                if (!filePath.Equals(""))
                    m_Scope = m_Model.GetRelativePath(filePath); // the sequence relative to project-dir
                m_Context.ResetCmds(m_Scope);
            }
            override public void Visit(Tokenizer.Token Token) {
                if (m_IsRoot) {  //processing Root Node
                    m_IsRoot = false;          
                    LinkedList<Tokenizer.Token>.Enumerator x = Token.GetEnumerator();
                    while (x.MoveNext()) {
                        x.Current.InspectNodes(this);
                    }
                } else { //inspecting a cmd token
                    m_Context.m_ActualCmd = Token.GetTopNodeType();
                    LinkedList<CmdBase>.Enumerator y = m_Evaluators.GetEnumerator();
                    while (y.MoveNext()) {
                        if (y.Current.TryParse(this.m_Context, Token)) {
                            CmdBase _x = y.Current.Copy();
                            m_Context.AddCmd(m_Scope,_x);
                            break;
                        } else { //no cmd for this Token
                        }
                    }

                }
            }
        public void ParseTokens(LinkedList<Tokenizer.Token> tokens) {
            
            LinkedList<Tokenizer.Token>.Enumerator y= tokens.GetEnumerator();
            while(y.MoveNext()) {
                this.ResetState(y.Current.GetValue(false));
                y.Current.InspectNodes(this);
            }
            Verify();
        }
        void Verify() {
            //Todo
        }
        public LinkedList<CmdBase> GetCmds(String Scope) {
            return m_Context.GetCmds(Scope);
        }
        public List<String> GetScopes() {
            return m_Context.GetScopes();
        }
        public LinkedList<Parser2.Context.Log> GetLogs() {
            return m_Context.GetLogs();
        }
    }
}
