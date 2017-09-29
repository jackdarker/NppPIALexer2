using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
            public String m_Error="";
            abstract public Boolean TryParse(Parser2.Context Context, Tokenizer.Token Token);
            abstract public String AsText();
            abstract public CmdBase Copy();
            public CmdBase() { }
            public CmdBase(CmdBase CopyThis) {
                m_StartPos = CopyThis.m_StartPos;
                m_Error = CopyThis.m_Error;
            }
            public bool HasError() {
                return !m_Error.Equals("");
            }
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
                    LinkedList<Tokenizer.Token>.Enumerator m_Subs = Token.GetEnumerator();
                    while (m_Subs.MoveNext()) {
                        if (m_Subs.Current.GetNodeType().Equals(typeof(Tokenizer.RuleString))) {
                            this.m_Path = m_Subs.Current.GetValue(false);
                            break;
                        }
                    }
                    if (this.m_Path.Equals("\"\"")) {
                        Context.AddLog(1, " missing path", this);
                    }
                    this.m_StartPos = Token.GetPosStart();
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
        public class CmdUsing : CmdBase {
            public String m_Path = "";
            public String m_Name = "";
            public CmdUsing()
                : base() {
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
                    while (m_Subs.MoveNext()) {
                        if (m_Subs.Current.GetNodeType().Equals(typeof(Tokenizer.RuleString))) {
                            this.m_Path = m_Subs.Current.GetValue(false);
                            this.m_Name = this.m_Path;
                        }
                        if (m_Subs.Current.GetNodeType().Equals(typeof(Tokenizer.RuleName))) {
                            this.m_Name = m_Subs.Current.GetValue(false);
                        }
                    }
                    if (this.m_Path.Equals("")) {
                        this.m_Error += " missing path";
                    }
                    this.m_StartPos = Token.GetPosStart();
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
                    Context.m_ActualCmd.Equals(typeof(Tokenizer.RuleDecl)) ||
                    Token.GetTopNodeType().Equals(typeof(Tokenizer.RuleRetDecl))) {
                    this.m_Type = Token.GetValue(false);
                    if (this.m_Type.Equals("int") || this.m_Type.Equals("bool") ||
                        this.m_Type.Equals("double") || this.m_Type.Equals("string") ||
                        this.m_Type.Equals("variant")) {
                    } else {
                        this.m_Error += Token.GetValue(false) + " is not a valid Type";
                    }
                    
                    LinkedList<Tokenizer.Token>.Enumerator m_Subs = Token.GetEnumerator();
                    while (m_Subs.MoveNext()) {
                        if (m_Subs.Current.GetNodeType().Equals(typeof(Tokenizer.RuleName))) {
                            this.m_Name = m_Subs.Current.GetValue(false);
                            break;
                        }
                    }
                    if (this.m_Name.Equals("") && !Token.GetTopNodeType().Equals(typeof(Tokenizer.RuleRetDecl))) {
                        this.m_Error += " missing name";
                    }
                    //Todo missing name
                    this.m_StartPos = Token.GetPosStart();
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
            public override CmdBase Copy() {
                return new CmdFunctionDecl(this);
            }
            public CmdFunctionDecl():base() {}
            public CmdFunctionDecl(CmdFunctionDecl CopyThis)
                : base(CopyThis) {
                m_Name = CopyThis.m_Name;
                m_Params = new LinkedList<CmdDecl>(CopyThis.m_Params);
                m_Returns = new LinkedList<CmdDecl>(CopyThis.m_Returns);
            }
            override public Boolean TryParse(Context Context, Tokenizer.Token Token) {
                m_Error = "";
                Boolean _Ret = true;
                if (Context.m_ActualCmd.Equals(typeof(Tokenizer.RuleFunctionDecl))) {
                    LinkedList<Tokenizer.Token>.Enumerator m_Subs = Token.GetEnumerator();               
                    while (m_Subs.MoveNext()) {
                        if (m_Subs.Current.GetNodeType().Equals(typeof(Tokenizer.RuleName))) {
                            this.m_Name = m_Subs.Current.GetValue(false);
                        }
                        if (m_Subs.Current.GetTopNodeType().Equals(typeof(Tokenizer.RuleParamDecl))) {
                            this.m_Params = ParseParams(Context,m_Subs.Current,null);  //this is the first node in the params fe. string from (string sText,int Wolf))
                        }
                        if (m_Subs.Current.GetTopNodeType().Equals(typeof(Tokenizer.RuleRetDecl))) {
                            this.m_Returns = ParseReturns(Context, m_Subs.Current, null);  //this is the first node in the params fe. string from (string sText,int Wolf))
                        }
                    }
                    //Todo missing name
                    this.m_StartPos = Token.GetPosStart();
                    return _Ret;
                } else {
                    return false;
                }
            }
            /// <summary>
            /// Parses a Return-List by Recursion
            /// </summary>
            /// <param name="m_Params">Start-Node of the Return-List</param>
            /// <param name="ListIn">Null</param>
            /// <returns></returns>
            static LinkedList<CmdDecl> ParseReturns(Context Context, Tokenizer.Token m_Params, LinkedList<CmdDecl> ListIn) {
                if (ListIn == null) {
                    ListIn = new LinkedList<CmdDecl>();
                }
                //this tree starts with ->
                CmdDecl _Decl = new CmdDecl();
                LinkedList<Tokenizer.Token>.Enumerator Params = m_Params.GetEnumerator();
                while (Params.MoveNext()) {
                    if (Params.Current.GetNodeType().Equals(typeof(Tokenizer.RuleSeparator))) {
                        //more Params ? - go deeper into the tree
                            ListIn = ParseReturns(Context, Params.Current, ListIn);
                         // TODO else Param is missing
                    } else if (Params.Current.GetNodeType().Equals(typeof(Tokenizer.RuleBaseType))) {
                        _Decl.TryParse(Context, Params.Current);
                        ListIn.AddLast(_Decl);
                        ListIn = ParseReturns(Context, Params.Current, ListIn);
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
            static LinkedList<CmdDecl> ParseParams(Context Context,Tokenizer.Token m_Params,LinkedList<CmdDecl> ListIn) {
                if (ListIn == null) {
                    ListIn = new LinkedList<CmdDecl>();
                }
                //this tree starts with the first BaseType
                CmdDecl _Decl = new CmdDecl();
                _Decl.TryParse(Context, m_Params);
                ListIn.AddLast(_Decl);
                LinkedList<Tokenizer.Token>.Enumerator Params = m_Params.GetEnumerator();
                while (Params.MoveNext()) {
                    if (Params.Current.GetNodeType().Equals(typeof(Tokenizer.RuleSeparator))) {
                        //more Params - go deeper into the tree
                        LinkedList<Tokenizer.Token>.Enumerator ParamsNext=Params.Current.GetEnumerator();
                        if (ParamsNext.MoveNext()) 
                        {
                            ListIn = ParseParams(Context, ParamsNext.Current, ListIn);
                        } // TODO else Param is missing
                    }
                    //Console.WriteLine(Params.Current.GetValue(false));
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
                return "Declaration of function " + m_Name + " with Params " + ParamsAsText(m_Params) + " with Returns " + ParamsAsText(m_Returns);
            }
        }
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
