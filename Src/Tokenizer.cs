using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using NppPIALexer2.Forms;

namespace NppPIALexer2 {
    public class Tokenizer {
        public class Context{
            String s_Text;
            public Context(String text) {
                s_Text=text;
            }
            int Pos=0;
            public void SetPos(int pos) {
                Pos=pos;
            }
            public int GetPos() {
                return Pos;
            }
        }
        public class NodeBuilder {
            public NodeBuilder() {
            }
            virtual public void Visit(Token token) {
                LinkedList<Token>.Enumerator x = token.GetEnumerator();
                while (x.MoveNext()) {
                    x.Current.InspectNodes(this);
                }
            }
        }
        public class Token : LinkedList<Token> {
            public Token() {
                m_Status = -2;
            }
            public Token(Boolean Empty) {
                if(Empty) m_Status = -1;
            }
           
            int m_Status;
            public Boolean IsValid() {
                return m_Status == 0;
            }
            public Boolean IsEmpty() {
                return m_Status == -1;
            }
            bool m_IsCmd;
            /// <summary>
            /// is this token the start of a new cmd
            /// </summary>
            /// <returns></returns>
            public Boolean IsCmd() {
                return m_Status == -1;
            }
            String m_Error = "";
            public void SetError(String Text) {
                m_Error = Text;
            }
            public string GetError() {
                return m_Error;
            }
            String m_Value;
            System.Type m_ThisNode;
            System.Type m_TopNode;
            public void SetValue(String value, int Start,Rule TopNode, Rule ThisNode) {
                m_Value = value ;
                m_PosStart = Start;
                m_PosEnd = Start + value.Length;
                m_ThisNode = ThisNode.GetType();
                m_TopNode = TopNode.GetType();
                m_Status = 0;
            }
            public String GetValue(Boolean FullPath) {      
                String Out = m_Value;//+"(" + m_Type.ToString() + ")";
                if(m_Error!="") {
                    Out += m_Error;
                }
                if (FullPath) {
                    LinkedList<Token>.Enumerator x = this.GetEnumerator();
                    while (x.MoveNext()) {
                        Out = Out + " " + x.Current.GetValue(FullPath);
                    }
                }
                return Out;
            }
            public System.Type GetNodeType() {
                if (m_ThisNode == null)
                    return this.GetType(); //??
                return m_ThisNode;

            }
            public System.Type GetTopNodeType() {
                if (m_TopNode == null)
                    return this.GetType();
                return m_TopNode;

            }
            public void InspectNodes(NodeBuilder Visitor) {
                Visitor.Visit(this);
            }
            int m_PosStart;
            int m_PosEnd;
            public void SetPosStart(int pos) {
                m_PosStart= pos;
            }
            public int GetPosStart() {
                return m_PosStart;
            }
            public void SetPosEnd(int pos) {
                m_PosEnd = pos;
            }
            public int GetPosEnd() {
                return m_PosEnd;
            }
            public void Combine(Token B) {
                this.AddLast(B);
            }
            
        }
        abstract public class Rule {
            protected Rule m_Parent;
            public Rule GetParent() {
                return m_Parent;
            }
            public Rule(Rule Parent) { 
                m_Parent = Parent; 
            }
            public bool IsCmd() {
                return this.Equals(m_Parent);
            }
            public int m_Score;
            public void Match() {
                if(m_Parent != null)
                    m_Parent.m_Score++;
                else
                    m_Score++;
            }
            public String m_Error="";
            public void NoMatch(String Reason) {
                if(m_Parent != null) {
                    if(m_Parent.m_Error == "") {
                        m_Parent.m_Error = Reason;
                    }
                } else {
                    if(m_Error == "") {
                        m_Error = Reason;
                    }
                }
            }
            public void ResetScore() {
                if(m_Parent != null)
                    m_Parent.m_Score=0;
                else
                    m_Score=0;
            }
            public int GetScore() {
                if(m_Parent != null)
                    return m_Parent.m_Score;
                else
                    return m_Score;
            }
            abstract public Token Evaluate(String stream, ref int pos);

        }
        static String s_ManyWhitespace = "[ \\t]*";
        #region BaseRules
        /// <summary>
        /// match all
        /// </summary>
        public class RuleSequence : Rule {
            protected LinkedList<Rule> m_Nodes = new LinkedList<Rule>();
           
            public RuleSequence(Rule Parent)
                : base(Parent) {
            }
            public void AddNode(Rule Node) {
                m_Nodes.AddLast(Node);
            }
            public override Token Evaluate(String stream, ref int pos) {
                int PosSave = pos;
                int ScoreSave = m_Score;
                Token ResultA = new Token(true);
                Token ResultB = new Token();

                LinkedList<Rule>.Enumerator Nodes = m_Nodes.GetEnumerator();
                while (Nodes.MoveNext()) {
                    ResultB = Nodes.Current.Evaluate(stream, ref pos);
                    if (ResultB.IsEmpty()) { //drop empty Token
                    } else if(ResultB.IsValid()) {
                        Match();
                        if (ResultA.IsValid()) {
                            ResultA.Combine(ResultB);
                        } else {
                            ResultA = ResultB;
                        }
                    } else {
                        pos = PosSave;
                        ResultA = new Token();
                        break;
                        
                    }
                }
                return ResultA;     //return empty Token if no match
            }
        }
        /// <summary>
        /// match exactly one
        /// </summary>
        public class RuleAlternative : RuleSequence {
            public RuleAlternative(Rule Parent): base(Parent) {
            }
            public override Token Evaluate(String stream, ref int pos) {
                int PosSave = pos;
                Token Result =new Token();
                LinkedList<Rule>.Enumerator Nodes = m_Nodes.GetEnumerator();
                while (Nodes.MoveNext()) {
                    Result = Nodes.Current.Evaluate(stream, ref pos);
                    if (Result.IsValid() ) {
                        Match();
                        break;
                    } 
                }
                return Result;
            }
        }
        /// <summary>
        /// match One,Multiple or None
        /// </summary>
        public class RuleMultiple : RuleSequence {
            int m_MinMatches=0;
            public RuleMultiple(Rule Parent, int MinMatches)
                : base(Parent) {
                    m_MinMatches = MinMatches;
            }
            public override Token Evaluate(String stream, ref int pos) {
                int PosSave = pos;
                Token ResultA = new Token(true);
                Token ResultB ;
                int Matches = 0;
                bool Run=true;    
                while(Run) {
                    ResultB = m_Nodes.First.Value.Evaluate(stream, ref pos);
                    if (ResultB.IsEmpty()) {//drop empty Token
                    } else if (ResultB.IsValid()) {
                        Matches++;
                        Match();
                        if (ResultA.IsValid()) {
                            ResultA.Combine(ResultB);
                        } else {
                            ResultA = new Token(false);
                            ResultA.SetValue("", ResultB.GetPosStart(), m_Parent, this);
                            ResultA.Combine(ResultB);
                            //ResultA = ResultB;
                        }
                    } 
                    Run = ResultB.IsValid();
                }
                if (!ResultA.IsValid()) {
                    pos = PosSave;
                    if (m_MinMatches <= Matches) {
                        ResultA = new Token(true);   //return empty Token
                    }
                }
                return ResultA;
            }
        }
        /// <summary>
        /// match one or none of the options 
        /// returns empty token if no match !
        /// </summary>
        public class RuleOption : RuleSequence {
            public RuleOption(Rule Parent)
                : base(Parent) {
            }
            public override Token Evaluate(String stream, ref int pos) {
                int PosSave = pos;
                Token Result = new Token();
                LinkedList<Rule>.Enumerator Nodes = m_Nodes.GetEnumerator();
                while (Nodes.MoveNext()) {
                    Result = Nodes.Current.Evaluate(stream, ref pos);
                    if (Result.IsValid() ) {
                        Match();
                        break;
                    }
                }
                if (!Result.IsValid()) {
                    pos = PosSave;
                    Result = new Token(true);   //return empty Token
                }
                return Result;
            }
        }
        /// <summary>
        /// Search for String-Pattern
        /// </summary>
        public class RuleRegex : Rule {
            Regex m_Regex;
            Rule m_BaseRule;
            public RuleRegex(Rule Parent, String regex, Rule BaseRule)
                : base(Parent) {
                m_Regex = new Regex(regex, RegexOptions.Singleline);
                m_BaseRule = (BaseRule!=null)? BaseRule:this;
            }
            public override Token Evaluate(String stream, ref int pos) {
                int PosSave = pos;
                Token Result=new Token() ;
                // Match the regular expression pattern against a text string.
                // https://msdn.microsoft.com/de-de/library/az24scfc(v=vs.100).aspx
                Match m = m_Regex.Match(stream, pos);
                if (m.Success && m.Index==pos) {
                    Result = new Token();
                    Result.SetValue(m.Value, m.Index, this.m_Parent, m_BaseRule);
                    pos = Result.GetPosEnd();
                    Match();
                } else {
                    NoMatch(m_Regex.ToString());
                    pos = PosSave;
                }
                return Result;
            }
        }
        #endregion
        #region Characters
        public class RuleComment : RuleSequence {
            public RuleComment(Rule Parent)   : base(Parent) {
                this.m_Parent = this;
                this.AddNode(new RuleRegex(m_Parent,
                    s_ManyWhitespace + "[;]*" + s_ManyWhitespace + "//[^\\r\\n]*", this));
                this.AddNode(new RuleEOL(Parent));
            }
        }
        public class RuleBreak : RuleRegex {
            public RuleBreak(Rule Parent)
                : base(Parent, s_ManyWhitespace + "\\bbreak\\b" + s_ManyWhitespace, null) {
            }
        }
        /// <summary>
        /// matches either EOL or //comment+EOL
        /// </summary>
        public class RuleEOLComment : RuleAlternative {
            public RuleEOLComment(Rule Parent)
                : base(Parent) {
                this.AddNode( new RuleComment(Parent));
                this.AddNode( new RuleEOL(Parent));
            }
        }
        public class RuleName : RuleRegex {
            public RuleName(Rule Parent)
                : base(Parent, "[A-Za-z_][A-Za-z0-9_]*", null) {
            }
        }
        /// <summary>
        /// like "asd 4.3 ;214fdg"
        /// </summary>
        public class RuleString : RuleRegex {
            public RuleString(Rule Parent)
                : base(Parent, "\"[^\"\r\n]*\"", null) {
            }
        }
        /// <summary>
        /// like true or false
        /// </summary>
        public class RuleBool : RuleRegex {
            public RuleBool(Rule Parent)
                : base(Parent, "(true|false)", null) {
            }
        }
        /// <summary>
        /// Separator like ',' with Spaces
        /// </summary>
        public class RuleSeparator : RuleRegex {
            public RuleSeparator(Rule Parent)
                : base(Parent, s_ManyWhitespace + "," + s_ManyWhitespace, null) {
            }
        }
        /// <summary>
        /// one or more whitespace
        /// </summary>
        public class RuleSpace : RuleRegex {
            public RuleSpace(Rule Parent)
                : base(Parent, "[ \\t]+", null) {
            }
        }
        
        /// <summary>
        /// 0 or more whitespace
        /// </summary>
       public class RuleSpaceOptional : RuleRegex {
           public RuleSpaceOptional(Rule Parent)
               : base(Parent, "[ \\t]*", null) {
            }
        }
        /// <summary>
        /// 1 or more EOL
        /// </summary>
        public class RuleEOL : RuleRegex {
            public RuleEOL(Rule Parent)
                : base(Parent,
                s_ManyWhitespace + "[;]*" + s_ManyWhitespace + "([\r]*[\n])+", null) {
                    m_Parent=this;
            }
        }
        /// <summary>
        /// 
        /// </summary>
       /* RuleEOL! public class RuleEmptyLine : RuleRegex {
            public RuleEmptyLine(Rule Parent)
                : base(Parent, "[\t \r]*[\n]") {
                m_Parent = this;
            }
        }*/
        /// <summary>
        /// only used to skip a line that cannot be parsed
        /// </summary>
        public class RuleAnyLine : RuleRegex {
            public RuleAnyLine(Rule Parent)
                : base(Parent, "[^\r]*[\r]*[\n]", null) {
                m_Parent = this;
            }
            
            public void SetError(String Text) {
                m_Error = Text;
            }
        }
        public class RuleLPar : RuleRegex {
            public RuleLPar(Rule Parent)
                : base(Parent, s_ManyWhitespace + "\\(" + s_ManyWhitespace, null) {
            }
        }
        public class RuleRPar : RuleRegex {
            public RuleRPar(Rule Parent)
                : base(Parent, s_ManyWhitespace + "\\)" + s_ManyWhitespace, null) {
            }
        }
        public class RuleRCurlPar : RuleSequence {
            public RuleRCurlPar(Rule Parent)
                : base(Parent) {
                    this.AddNode(new RuleRegex(Parent, s_ManyWhitespace + "}" + s_ManyWhitespace, this));
                this.AddNode(new RuleEOLComment(Parent));
            }
        }
        public class RuleLCurlPar : RuleSequence {
            public RuleLCurlPar(Rule Parent)
                : base(Parent) {
                    this.AddNode(new RuleRegex(Parent, s_ManyWhitespace + "{" + s_ManyWhitespace, this));
                this.AddNode(new RuleEOLComment(Parent));
            }
        }
        
        public class RuleNumber : RuleRegex {
            public RuleNumber(Rule Parent)
                : base(Parent, "[0-9]+('.'[0-9]+)?", null) {
            }
        }
        #endregion
        #region keywords
        //integrated basic types
        public class RuleBaseType : RuleRegex {
            public RuleBaseType(Rule Parent)
                : base(Parent, "(int|double|bool|string|variant)", null) {
                //Note: cannot say S*(int|bool)S* because this would cause trouble: MakeSquare(int15);
            }
        }

        #endregion
        #region structures
        /// <summary>
        /// Variable Declaration      int x   |    int x=5
        /// </summary>
        public class RuleDecl : RuleSequence {
            public RuleDecl(Rule Parent): base(Parent) {
                m_Parent = this;
                this.AddNode(new RuleSpaceOptional(m_Parent));
                this.AddNode(new RuleBaseType(m_Parent));
                this.AddNode(new RuleSpace(m_Parent));
                this.AddNode(new RuleName(m_Parent));
                RuleSequence z = new RuleSequence(m_Parent);
                z.AddNode(new RuleRegex(m_Parent, s_ManyWhitespace + "=" + s_ManyWhitespace, this));
                    RuleAlternative x = new RuleAlternative(m_Parent);
                    x.AddNode(new RuleString(m_Parent));
                    x.AddNode(new RuleBool(m_Parent));
                    x.AddNode(new RuleName(m_Parent));
                    x.AddNode(new RuleExpr(m_Parent));
                    z.AddNode(x);
                    RuleOption y = new RuleOption(m_Parent);
                y.AddNode(z);
                this.AddNode(y);
                this.AddNode(new RuleEOLComment(m_Parent));
            }
        }
        /// <summary>
        /// like x=5.12
        /// </summary>
        public class RuleAssign : RuleSequence {
            public  RuleAssign(Rule Parent)
                : base(Parent) {
                    m_Parent = this;
                    this.AddNode(new RuleSpaceOptional(m_Parent));
                    this.AddNode(new RuleName(m_Parent));
                    this.AddNode(new RuleRegex(m_Parent, s_ManyWhitespace + "=" + s_ManyWhitespace, this));
                    RuleAlternative x = new RuleAlternative(m_Parent);
                    x.AddNode(new RuleString(m_Parent));
                    x.AddNode(new RuleBool(m_Parent));
                    x.AddNode(new RuleName(m_Parent));
                    x.AddNode(new RuleExpr(m_Parent)); 
                    this.AddNode(x);
                    this.AddNode(new RuleEOLComment(m_Parent));
            }
        }
        public class RuleMultExpr : RuleSequence {   //power_expression (S ('*' / '/') S power_expression)*;
            public RuleMultExpr(Rule Parent)
                : base(Parent) {
                this.AddNode( new RulePlusExpr(Parent));
                RuleSequence x = new RuleSequence(Parent);
                x.AddNode(new RuleRegex(m_Parent, "(\\*|/)", this));
                x.AddNode( new RulePlusExpr(Parent));
                RuleMultiple y = new RuleMultiple(Parent,0);
                y.AddNode(x);
                this.AddNode(y);
            }
        }
        /* not supported by sequencer
        public class PowerExpr : RuleSequence {   //primary_expression (S '^' S primary_expression)? ;
            private static PowerExpr instance;
            public static PowerExpr Instance {
                get {
                    if (instance == null) {
                        instance = new PowerExpr();
                    }
                    return instance;
                }
            }
            private PowerExpr()
                : base() {
                this.AddNode(RulePlusExpr(Parent));
                RuleSequence x = new RuleSequence();
                x.AddNode(new RuleRegex("(^)"));
                x.AddNode(RulePlusExpr(Parent));
                RuleMultiple y = new RuleMultiple(0);
                y.AddNode(x);
                this.AddNode(y);
            }
        }*/
        // use RuleExpr instead !
        public class RulePlusExpr : RuleSequence {   //('+' / '-')? S(NAME /  number / '(' S expression S ')')
            public RulePlusExpr(Rule Parent)
                : base(Parent) {
                    this.AddNode(new RuleRegex(m_Parent, "(\\+|\\-)?", this));
                RuleAlternative x = new RuleAlternative(Parent);
                x.AddNode( new RuleName(Parent));
                x.AddNode( new RuleNumber(Parent));
                this.AddNode(x);
            }
        }
        //use RuleBoolExpr instead !
        public class RuleCompareExpr : RuleSequence {   // (NAME /  BOOL / NUMBER / STRING )S ( >=/ <= / == / !=)S (NAME /  BOOL / NUMBER / STRING )
            public RuleCompareExpr(Rule Parent)
                : base(Parent) {
                RuleAlternative x = new RuleAlternative(Parent);
                x.AddNode(new RuleName(Parent));
                x.AddNode(new RuleBool(Parent));
                x.AddNode(new RuleNumber(Parent));
                x.AddNode(new RuleString(Parent));
                this.AddNode(x);
                this.AddNode(new RuleRegex(m_Parent, "(\\=\\=|\\>\\=|\\<\\=|\\>|\\<|\\!\\=)", this));
                this.AddNode(x);
            }
        }
        //use RuleBoolExpr instead !
        public class RuleBoolVarExpr : RuleSequence {   //('!')? S(NAME /  BOOL / '(' S Comparission S ')')
            public RuleBoolVarExpr(Rule Parent)
                : base(Parent) {
                    this.AddNode(new RuleRegex(m_Parent, "(!)?", this));
                RuleAlternative x = new RuleAlternative(Parent);
                x.AddNode(new RuleCompareExpr(Parent));
                x.AddNode(new RuleName(Parent));
                x.AddNode(new RuleBool(Parent));
                this.AddNode(x);
            }
        }
        public class RuleBoolExpr : RuleSequence {   //RuleBoolVarExpr (S ('||' / '&&') S RuleBoolVarExpr)*;
            public RuleBoolExpr(Rule Parent)
                : base(Parent) {
                this.AddNode(new RuleBoolVarExpr(Parent));
                RuleSequence x = new RuleSequence(Parent);
                x.AddNode(new RuleRegex(m_Parent, "(\\|\\||\\&\\&)", this));      //todo Number <= Number bool!=bool \\=\\=|\\>\\=|\\<\\=|\\>|\\<|\\!\\=
                x.AddNode(new RuleBoolVarExpr(Parent));
                RuleMultiple y = new RuleMultiple(Parent, 0);
                y.AddNode(x);
                this.AddNode(y);
            }
        }
        // mathematical formula like (x+5)*y
        public class RuleExpr : RuleSequence {  //multiplicative_expression (S ('+' / '-') S multiplicative_expression)* 
            public RuleExpr(Rule Parent)
                : base(Parent) {
                    m_Parent = this;
                    this.AddNode(new RuleSpaceOptional(m_Parent));
                    this.AddNode(new RuleMultExpr(m_Parent));
                    RuleSequence x = new RuleSequence(m_Parent);
                    x.AddNode(new RuleRegex(m_Parent, "(\\+|\\-)", this));
                    x.AddNode(new RuleMultExpr(m_Parent));
                    RuleMultiple y = new RuleMultiple(m_Parent, 0);
                y.AddNode(x);
                this.AddNode(y);
            }
        }
        public class RuleInclude : RuleSequence {   //#include STRING
            public RuleInclude(Rule Parent)
                : base(Parent) {
                    m_Parent = this;
                    this.AddNode(new RuleRegex(m_Parent, s_ManyWhitespace + "#include" + s_ManyWhitespace, this));
                this.AddNode(new RuleString(m_Parent));
                this.AddNode(new RuleEOLComment(m_Parent));
            }
        }
        public class RuleUsing : RuleSequence {   //using STRING as NAME  or using STRING
            public RuleUsing(Rule Parent)
                : base(Parent) {
                m_Parent = this;
                this.AddNode(new RuleRegex(m_Parent, s_ManyWhitespace + "using" + s_ManyWhitespace, this));
                this.AddNode(new RuleString(m_Parent));
                
                RuleSequence x = new RuleSequence(m_Parent);
                x.AddNode(new RuleRegex(m_Parent, s_ManyWhitespace + "as" + s_ManyWhitespace, this));
                x.AddNode(new RuleName(m_Parent));
               
                RuleOption y = new RuleOption(m_Parent);
                y.AddNode(x);
                this.AddNode(y);
                this.AddNode(new RuleEOLComment(m_Parent));
            }
        }
        //curled braces following functiondecl. or while, if,... 
        public class RuleBody : RuleMultiple {   //{ ...  } EOL
            public RuleBody(Rule Parent)
                : base(Parent,0) {
                    if (Parent == null) m_Parent = this;
                    
            }
            private bool m_Initialised = false;
            public override Token Evaluate(string stream, ref int pos) {
                if (m_Initialised == false) {
                    RuleAlternative x = new RuleAlternative(m_Parent);
                    addRule(x, new RuleDecl(m_Parent));
                    addRule(x, new RuleAssign(m_Parent));
                    addRule(x, new RuleEOLComment(m_Parent));
                    addRule(x, new RuleWhile(m_Parent));
                    addRule(x, new RuleIf(m_Parent));
                    addRule(x, new RuleSwitch(m_Parent));
                    addRule(x, new RuleBreak(m_Parent));
                    //addRule(x, new RuleEmptyLine(m_Parent));
                    
                    //RuleMultiple z = new RuleMultiple(this, 0);
                    //z.AddNode(x); //there can be several instances in between {}
                    this.AddNode(x);
                    this.m_Initialised = true;
                }

                return base.Evaluate(stream, ref pos);
            }
            private Rule addRule(RuleAlternative Collection, Rule ToAdd) {
                RuleSequence y = new RuleSequence(ToAdd.GetParent());
                y.AddNode(ToAdd);
                Collection.AddNode(y);
                return Collection;
            }
        }
        public class RuleIf : RuleSequence { //'if' S* '(' EXPRESSION ')' (COMMENT | EOL) BODY ('else' (COMMENT | EOL) BODY)?
            public RuleIf(Rule Parent)   
                : base(Parent) {
                    m_Parent = this;
                    this.AddNode(new RuleRegex(m_Parent, s_ManyWhitespace + "\\bif\\b" + s_ManyWhitespace, this));
                this.AddNode(new RuleLPar(m_Parent));
                this.AddNode(new RuleBoolExpr(m_Parent));
                this.AddNode(new RuleRPar(m_Parent));
                this.AddNode(new RuleEOLComment(m_Parent));
                this.AddNode(new RuleLCurlPar(m_Parent));
                this.AddNode(new RuleBody(m_Parent));
                this.AddNode(new RuleRCurlPar(m_Parent));
                RuleOption y = new RuleOption(m_Parent);
                RuleSequence x = new RuleSequence(m_Parent);
                x.AddNode(new RuleRegex(m_Parent, s_ManyWhitespace + "\\belse\\b" + s_ManyWhitespace, this));
                x.AddNode(new RuleEOLComment(m_Parent));
                x.AddNode(new RuleLCurlPar(m_Parent));
                x.AddNode(new RuleBody(m_Parent));
                x.AddNode(new RuleRCurlPar(m_Parent));
                y.AddNode(x);
                this.AddNode(y);
            }
        }
        public class RuleWhile : RuleSequence { //'while' S* '(' EXPRESSION ')' (COMMENT | EOL) BODY 
            public RuleWhile(Rule Parent)
                : base(Parent) {
                m_Parent = this;
                this.AddNode(new RuleRegex(m_Parent, s_ManyWhitespace + "\\bwhile\\b" + s_ManyWhitespace, this));
                this.AddNode(new RuleLPar(m_Parent));
                this.AddNode(new RuleBoolExpr(m_Parent));
                this.AddNode(new RuleRPar(m_Parent));
                this.AddNode(new RuleEOLComment(m_Parent));
                this.AddNode(new RuleLCurlPar(m_Parent));
                this.AddNode(new RuleBody(m_Parent));
                this.AddNode(new RuleRCurlPar(m_Parent));
            }
        }
        public class RuleSwitch : RuleSequence { //'switch' S* '(' EXPRESSION ')' (COMMENT | EOL) '{' EOL 
                                                 //('case ' VALUE ':' EOL
                                                 //    ...
                                                 //    BREAK )?
                                                 // 'default:' EOL
                                                 //    ...
                                                 //'}' 
             public RuleSwitch(Rule Parent)
                 : base(Parent) {
                 m_Parent = this;
                 this.AddNode(new RuleRegex(m_Parent, s_ManyWhitespace + "\\bswitch\\b" + s_ManyWhitespace, this));
                 this.AddNode(new RuleLPar(m_Parent));
                 this.AddNode(new RuleName(m_Parent));
                 this.AddNode(new RuleRPar(m_Parent));
                 this.AddNode(new RuleEOLComment(m_Parent));
                 this.AddNode(new RuleLCurlPar(m_Parent));
                 RuleMultiple x = new RuleMultiple(m_Parent,0);
                 x.AddNode(new RuleSwitchCase(m_Parent));
                 this.AddNode(x);
                 RuleOption y = new RuleOption(m_Parent);
                 y.AddNode(new RuleSwitchDefault(m_Parent));
                 this.AddNode(y);
                 this.AddNode(new RuleRCurlPar(m_Parent));
             }
         }
         public class RuleSwitchCase : RuleSequence { //('case ' (NUMBER | STRING) ':' EOL
                                                 //    ...
                                                 //    BREAK )?
                                                 // 'default:' EOL
                                                 //    ...
                                                 //'}' 
             public RuleSwitchCase(Rule Parent)
                 : base(Parent) {
                 m_Parent = this;
                 this.AddNode(new RuleRegex(m_Parent, s_ManyWhitespace + "\\bcase\\b" + s_ManyWhitespace, this));
                RuleAlternative x = new RuleAlternative(m_Parent); 
                x.AddNode(new RuleNumber(m_Parent));
                x.AddNode(new RuleString(m_Parent));
                this.AddNode(x);
                this.AddNode(new RuleRegex(m_Parent, s_ManyWhitespace + ":" + s_ManyWhitespace, this));
                 this.AddNode(new RuleEOLComment(m_Parent));
                this.AddNode(new RuleBody(m_Parent));
             }
        }
         public class RuleSwitchDefault : RuleSequence { //('case ' (NUMBER | STRING) ':' EOL
             //    ...
             //    BREAK )?
             // 'default:' EOL
             //    ...
             //'}' 
             public RuleSwitchDefault(Rule Parent)
                 : base(Parent) {
                 m_Parent = this;
                 this.AddNode(new RuleRegex(m_Parent, s_ManyWhitespace + "\\bdefault\\b" + s_ManyWhitespace, this));
                 this.AddNode(new RuleRegex(m_Parent, s_ManyWhitespace + ":" + s_ManyWhitespace, this));
                 this.AddNode(new RuleEOLComment(m_Parent));
                 this.AddNode(new RuleBody(m_Parent));
             }
         }
         
        public class RuleFunctionDecl : RuleSequence {  //'function ' NAME S*'(' PARAMDECL? ')' S* RETDECL? S* '{' (COMMENT | EOL) FUNCBODY '}' EOL? 
            public RuleFunctionDecl(Rule Parent)
                : base(Parent) {
                    m_Parent = this;
                    this.AddNode(new RuleRegex(m_Parent, s_ManyWhitespace + "function[ \\t]+", this));
                this.AddNode(new RuleName(m_Parent));
                this.AddNode(new RuleLPar(m_Parent));
                RuleOption y = new RuleOption(m_Parent);
                y.AddNode(new RuleParamDecl(m_Parent));
                this.AddNode(y);
                this.AddNode(new RuleRPar(m_Parent));
                y = new RuleOption(m_Parent);
                y.AddNode(new RuleRetDecl(m_Parent));
                this.AddNode(y);
                this.AddNode(new RuleEOLComment(m_Parent));
                //y = new RuleOption(m_Parent);       //body is optional??
                this.AddNode(new RuleLCurlPar(m_Parent));
                this.AddNode(new RuleBody(m_Parent));
                this.AddNode(new RuleRCurlPar(m_Parent));
                //this.AddNode(y);
            }
        }
        /// <summary>
        /// Parameter Declaration of function, sequencecalls,...
        /// </summary>
        public class RuleParamDecl : RuleMultiple {  // BASETYPE S NAME S (, BASETYPE S NAME S )*
            public RuleParamDecl(Rule Parent)
                : base(Parent,0) {
                    
                RuleSequence z = new RuleSequence(this);
                z.AddNode(new RuleBaseType(this));
                z.AddNode(new RuleSpace(this));
                z.AddNode(new RuleName(this));
                RuleSequence x = new RuleSequence(this);
                x.AddNode(new RuleSeparator(this));
                x.AddNode(z);
                RuleMultiple y = new RuleMultiple(this, 0);
                y.AddNode(x);
                RuleSequence w = new RuleSequence(this);
                w.AddNode(z);
                w.AddNode(y);
                this.AddNode(w);
            }
        }
        /// <summary>
        /// Return Declaration of function
        /// </summary>
        public class RuleRetDecl : RuleSequence {  //-> BASETYPE [S NAME ] (, BASETYPE [S NAME])*
            public RuleRetDecl(Rule Parent)
                : base(Parent) {
                    RuleAlternative w = new RuleAlternative(this);
                    RuleSequence u = new RuleSequence(this);
                    u.AddNode(new RuleBaseType(this));
                    u.AddNode(new RuleSpace(this));
                    u.AddNode(new RuleName(this));
                    w.AddNode(u);
                    u = new RuleSequence(this);
                    u.AddNode(new RuleBaseType(this));
                    w.AddNode(u);

                    this.AddNode(new RuleRegex(this, "->" + s_ManyWhitespace, this));
                this.AddNode(w);
                RuleSequence x = new RuleSequence(this);
                x.AddNode(new RuleSeparator(this));
                x.AddNode(w);
                RuleMultiple y = new RuleMultiple(this, 0);
                y.AddNode(x);
                this.AddNode(y);
            }
        }
        #endregion

        RuleEvaluater Rules;

        public class RuleEvaluater  : RuleOption {
            public RuleEvaluater()  : base(null) {
                //add all Rules that could make up one complete Line to a list
                //this list will be evaluated again and again until every text-line is processed
                // make sure to add the more specific Rules at the beginning
                AddNode(new RuleComment(null));
                AddNode(new RuleInclude(null));
                AddNode(new RuleUsing(null));
                AddNode(new RuleFunctionDecl(null));
                AddNode(new RuleDecl(null));
                AddNode(new RuleAssign(null));
                AddNode(new RuleBody(null));
                AddNode(new RuleEOL(null));
                //AddNode(new RuleEmptyLine(null));
            }
            public override Token Evaluate(string stream, ref int pos) {
                LinkedList<Rule>.Enumerator x=this.m_Nodes.GetEnumerator();
                while(x.MoveNext()) {
                    x.Current.ResetScore();
                }
                return base.Evaluate(stream, ref pos);
            }
            public String GetError() {
                String _Ret="";
                int Score = 0;
                int TmpScore;
                LinkedList<Rule>.Enumerator x = this.m_Nodes.GetEnumerator();
                while(x.MoveNext()) {
                    TmpScore=x.Current.GetScore();
                    if(Score < TmpScore) {
                        Score = TmpScore;
                        _Ret = x.Current.m_Error;
                    }
                }
                return _Ret;
            }
        }
        public Tokenizer() {
            Rules= new RuleEvaluater();
        }
        public Token TokenizeFile(String filePath) {

            Token _Ret = new Token();
            if (!File.Exists(filePath)) 
                return _Ret;

            string ext = Path.GetExtension(filePath).ToLower();
            if (string.IsNullOrEmpty(ext) || !ext.Equals(".seq"))    //only .seq files are parsed
                return _Ret;

            /* TODO
             * if (resource.getProjectRelativePath().segment(0).equalsIgnoreCase("SOURCE")) {
                m_IsClassDef = true;  //Version 1   its in //SOURCE//...
            }
            if (resource.getProjectRelativePath().segment(0).equalsIgnoreCase("APP")) {
                m_IsClassDef = true;   //Version 2    its in //APP//PLUGINS//...
            }*/
            String _content=File.ReadAllText(filePath);
            Log.getInstance().Add("Tokenize " + filePath, Log.EnuSeverity.Info, "");
            _Ret = Tokenize(_content);
            Rule Root = new RuleName(null);
            _Ret.SetValue(filePath, _Ret.GetPosStart(), Root, Root);
            return _Ret;
        }
    
        public Token Tokenize(String stream) {
            int Pos = 0;
            Token Result;
            Token FileNode = new Token();
            Rule Root = new RuleName(null);
            FileNode.SetValue("", 0, Root, Root);
            DateTime _start=DateTime.Now;
            while(Pos<stream.Length) {
                Result=Rules.Evaluate(stream, ref Pos);
                if (Result != null && Result.IsValid()) {
                    FileNode.AddLast(Result);
                } else {
                    RuleAnyLine x = new RuleAnyLine(null);
                    Result=x.Evaluate(stream, ref Pos);
                    Result.SetError("invalid:"+Rules.GetError() + " at " + Pos.ToString());
                    FileNode.AddLast(Result);
                    Log.getInstance().Add("invalid:" + Rules.GetError() + " at " + Pos.ToString(),Log.EnuSeverity.Warn,"");
                    break;
                }
            }
            DateTime _end = DateTime.Now;
            TimeSpan dt = _start - _end;
            return FileNode;
        }
    };
}
