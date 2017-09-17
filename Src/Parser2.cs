using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NppPIALexer2 {
    /// <summary>
    /// the parser uses the Token-tree to build commands
    /// it also checks for all kind of errors like assigning a bool to an int, colliding functionnames,...
    /// </summary>
    public class Parser2 {
        public class RuleWrongVariableType { 
        }
        /// ///////////////////////////////////////////////
        abstract public class CmdBase {
            protected int m_StartPos;
            public String m_Error="";
            abstract public Boolean TryParse(Builder.Context Context, Tokenizer.Token Token);
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
            override public Boolean TryParse(Builder.Context Context, Tokenizer.Token Token) {
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
            override public Boolean TryParse(Builder.Context Context, Tokenizer.Token Token) {
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
                    if (this.m_Path.Equals("")) {
                        this.m_Error += " missing path";
                        _Ret = false;
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
            override public Boolean TryParse(Builder.Context Context, Tokenizer.Token Token) {
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
                        _Ret = false;
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
            override public Boolean TryParse(Builder.Context Context, Tokenizer.Token Token) {
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
                        _Ret = false;
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
                        _Ret = false;
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
            String m_Name="";
            protected LinkedList<CmdDecl> m_Params = new LinkedList<CmdDecl>();
            protected LinkedList<CmdDecl> m_Returns = new LinkedList<CmdDecl>();
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
            override public Boolean TryParse(Builder.Context Context, Tokenizer.Token Token) {
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
            static LinkedList<CmdDecl> ParseReturns(Builder.Context Context, Tokenizer.Token m_Params, LinkedList<CmdDecl> ListIn) {
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
            static LinkedList<CmdDecl> ParseParams(Builder.Context Context,Tokenizer.Token m_Params,LinkedList<CmdDecl> ListIn) {
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

        public class Builder : Tokenizer.NodeBuilder {
            public class Context {
                protected LinkedList<CmdBase> m_Cmds = new LinkedList<CmdBase>();
                public Context() {
                }
                public System.Type m_ActualCmd;
                public void AddCmd(CmdBase Cmd) {
                    m_Cmds.AddLast(Cmd);
                }
                public LinkedList<CmdBase> GetCmds() {
                    return m_Cmds;
                }
            }
            Context m_Context;

            protected LinkedList<CmdBase> m_Evaluators = new LinkedList<CmdBase>();
            public Builder()
                : base() {
                    m_Context = new Context();
                    m_Evaluators.AddLast(new CmdDecl());
                    m_Evaluators.AddLast(new CmdFunctionDecl());
                    m_Evaluators.AddLast(new CmdUsing());
                    m_Evaluators.AddLast(new CmdInclude());    
                    m_Evaluators.AddLast(new CmdInvalid());
                    m_IsRoot = true;
            }
            bool m_IsRoot = false;
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
                        if (y.Current.TryParse(m_Context, Token)) {
                            CmdBase _x = y.Current.Copy();
                            m_Context.AddCmd(_x);
                            break;
                        } else { //no cmd for this Token
                        }
                    }

                }
            }
            public Context GetResult() {
                return m_Context;
            }
        }
        public Parser2() { 
        }
        Builder.Context m_Result;
        public void ParseTokens(Tokenizer.Token x) {
            Builder _bld = new Builder();
            x.InspectNodes(_bld);
            m_Result = _bld.GetResult();
            Verify();
        }
        void Verify() {
            m_Result.GetCmds();
        }
        public Builder.Context GetResult() {
            return m_Result;
        }
    }
}
