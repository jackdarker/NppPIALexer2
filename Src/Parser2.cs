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
            abstract public CmdBase TryParse(Builder.Context Context, Tokenizer.Token Token);
            abstract public String AsText();
            public bool HasError() {
                return !m_Error.Equals("");
            }
        }
        public class CmdInvalid : CmdBase {
            override public CmdBase TryParse(Builder.Context Context, Tokenizer.Token Token) {
                CmdInvalid Cmd = new CmdInvalid();
                Cmd.m_Error = "unknown Cmd " + Token.GetValue(true);
                return Cmd;
            }
            override public String AsText() {
                return m_Error;
            }
        }
        public class CmdDecl: CmdBase {
            public String m_Type;
            public String m_Name;
            override public CmdBase TryParse(Builder.Context Context, Tokenizer.Token Token) {
                if (Context.m_ActualCmd.Equals(typeof(Tokenizer.RuleDecl).ToString())) {
                    CmdDecl Cmd = new CmdDecl();
                    Cmd.m_Type = Token.GetValue(false);
                    if (Cmd.m_Type.Equals("int") || Cmd.m_Type.Equals("bool")) {
                    } else {
                        Cmd.m_Error += Token.GetValue(false) + " is not a valid Type";
                    }
                    
                    LinkedList<Tokenizer.Token>.Enumerator m_Subs = Token.GetEnumerator();
                    while (m_Subs.MoveNext()) {
                        if (m_Subs.Current.GetNodeType().Equals(typeof(Tokenizer.RuleName).ToString())) {
                            Cmd.m_Name = m_Subs.Current.GetValue(false);
                            break;
                        }
                    }
                    if (Cmd.m_Name.Equals("")) {
                        Cmd.m_Error += " missing name";
                    }
                    //Todo missing name
                    Cmd.m_StartPos = Token.GetPosStart();
                    return Cmd;
                } else {
                    return null;
                }
            }
            override public String AsText() {
                return "Declaration of "+m_Name+" as "+m_Type+" at Pos="+m_StartPos.ToString();
            }
        }
        
        public class CmdFunctionDecl : CmdBase {
            String m_Name="";
            protected LinkedList<CmdDecl> m_Params = new LinkedList<CmdDecl>();
            protected LinkedList<CmdDecl> m_Returns = new LinkedList<CmdDecl>();
            override public CmdBase TryParse(Builder.Context Context, Tokenizer.Token Token) {
                if (Context.m_ActualCmd.Equals(typeof(Tokenizer.RuleFunctionDecl).ToString())) {
                    CmdFunctionDecl Cmd = new CmdFunctionDecl();
                    LinkedList<Tokenizer.Token>.Enumerator m_Subs = Token.GetEnumerator();               
                    while (m_Subs.MoveNext()) {
                        if (m_Subs.Current.GetNodeType().Equals(typeof(Tokenizer.RuleName).ToString())) {
                            Cmd.m_Name = m_Subs.Current.GetValue(false);
                        }
                        if (m_Subs.Current.GetTopNodeType().Equals(typeof(Tokenizer.RuleParamDecl).ToString())) {
                            Cmd.m_Params = ParseParams(m_Subs.Current);
                        }
                    }
                    //Todo missing name
                    Cmd.m_StartPos = Token.GetPosStart();
                    return Cmd;
                } else {
                    return null;
                }
            }
            static LinkedList<CmdDecl> ParseParams(Tokenizer.Token m_Params) {
                LinkedList<Tokenizer.Token>.Enumerator m_Params = m_Subs.Current.GetEnumerator();
                while (m_Params.MoveNext()) {
                    Console.WriteLine(m_Params.Current.GetValue(false));
                }
                return null;
            }
            override public String AsText(){
                return "Declaration of function " + m_Name ;
            }
        }

        public class Builder : Tokenizer.NodeBuilder {
            public class Context {
                protected LinkedList<CmdBase> m_Cmds = new LinkedList<CmdBase>();
                public Context() {
                }
                public String m_ActualCmd;
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
                    CmdBase CmdOut=null;
                    LinkedList<CmdBase>.Enumerator y = m_Evaluators.GetEnumerator();
                    while (y.MoveNext()) {
                        CmdOut = y.Current.TryParse(m_Context, Token);
                        if (CmdOut != null) {
                            m_Context.AddCmd(CmdOut);
                            break;
                        } else { 
                            
                        }
                    }
                    if (CmdOut==null) {//no cmd for this Token
                        m_Context.AddCmd(new CmdInvalid().TryParse(m_Context,Token));
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
