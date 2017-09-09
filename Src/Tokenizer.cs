using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

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
           /* public Token(Token copy) {
                m_PosEnd = copy.GetPosEnd();
                m_PosStart = copy.GetPosStart();
                m_Value = copy.m_Value;
                m_Type = copy.m_Type;
            }*/
            int m_Status;
            public Boolean IsValid() {
                return m_Status == 0;
            }
            public Boolean IsEmpty() {
                return m_Status == -1;
            }
            String m_Value;
            Object m_ThisNode;
            Object m_TopNode;
            public void SetValue(String value, int Start,object TopNode, object ThisNode) {
                m_Value = value ;
                m_PosStart = Start;
                m_PosEnd = Start + value.Length;
                m_ThisNode = ThisNode;
                m_TopNode = TopNode;
                m_Status = 0;
            }
            public String GetValue(Boolean FullPath) {      
                string Out = m_Value;//+"(" + m_Type.ToString() + ")";
                if (FullPath) {
                    LinkedList<Token>.Enumerator x = this.GetEnumerator();
                    while (x.MoveNext()) {
                        Out = Out + " " + x.Current.GetValue(FullPath);
                    }
                }
                return Out;
            }
            public String GetNodeType() {
                if (m_ThisNode == null)
                    return "";
                return m_ThisNode.ToString();

            }
            public String GetTopNodeType() {
                if (m_TopNode == null)
                    return "";
                return m_TopNode.ToString();

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
            public Rule(Rule Parent) { 
                m_Parent = Parent; 
            }
            public bool IsCmd() {
                return this.Equals(m_Parent);
            }
            abstract public Token Evaluate(String stream, ref int pos);

        }
        static string s_ManyWhitespace = "[ \\t]*";
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
                Token ResultA = null;
                Token ResultB = null;

                LinkedList<Rule>.Enumerator Nodes = m_Nodes.GetEnumerator();
                while (Nodes.MoveNext()) {
                    ResultB = Nodes.Current.Evaluate(stream, ref pos);
                    if (ResultB != null ) {
                        if (ResultB.IsValid()) { //drop empty Token
                            if (ResultA != null) {
                                ResultA.Combine(ResultB);
                            } else {
                                ResultA = ResultB;
                            }
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
                Token Result = null;
                LinkedList<Rule>.Enumerator Nodes = m_Nodes.GetEnumerator();
                while (Nodes.MoveNext()) {
                    Result = Nodes.Current.Evaluate(stream, ref pos);
                    if (Result != null && Result.IsValid() ) {
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
                Token ResultA = null;
                Token ResultB = null;
                int Matches = 0;
                bool Run=true;    
                while(Run) {
                    ResultB = m_Nodes.First.Value.Evaluate(stream, ref pos);
                    if (ResultB != null) {
                        Matches++;
                        if (ResultB.IsValid()) { //drop empty Token
                            if (ResultA != null) {
                                ResultA.Combine(ResultB);
                            } else {
                                ResultA = ResultB;
                            }
                        }
                    }
                    Run = (ResultB != null) && ResultB.IsValid();
                }
                if (ResultA == null || !ResultA.IsValid()) {
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
        /// </summary>
        public class RuleOption : RuleSequence {
            public RuleOption(Rule Parent)
                : base(Parent) {
            }
            public override Token Evaluate(String stream, ref int pos) {
                int PosSave = pos;
                Token Result = null;
                LinkedList<Rule>.Enumerator Nodes = m_Nodes.GetEnumerator();
                while (Nodes.MoveNext()) {
                    Result = Nodes.Current.Evaluate(stream, ref pos);
                    if (Result != null && Result.IsValid() ) {
                        break;
                    }
                }
                if (Result == null || !Result.IsValid()) {
                    pos = PosSave;
                    Result = new Token(true);   //return empty Token
                }
                return Result;
            }
        }
        public class RuleRegex : Rule {
            Regex m_Regex;
            public RuleRegex(Rule Parent,String regex) : base(Parent) {
                m_Regex = new Regex(regex, RegexOptions.Singleline);
            }
            public override Token Evaluate(String stream, ref int pos) {
                int PosSave = pos;
                Token Result=null ;
                // Match the regular expression pattern against a text string.
                // https://msdn.microsoft.com/de-de/library/az24scfc(v=vs.100).aspx
                Match m = m_Regex.Match(stream, pos);
                if (m.Success && m.Index==pos) {
                    Result = new Token();
                    Result.SetValue(m.Value, m.Index, this.m_Parent,this);//m.Groups[0].Value);
                    pos = Result.GetPosEnd();
                } else {
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
                this.AddNode(new RuleRegex(m_Parent,s_ManyWhitespace + "//[^\\r\\n]*"));
                this.AddNode(new RuleEOL(Parent));
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
                : base(Parent,"[A-Za-z_][A-Za-z0-9_]*") {
            }
        }
        /// <summary>
        /// like "asd43214fdg"
        /// </summary>
        public class RuleString : RuleRegex {
            public RuleString(Rule Parent)
                : base(Parent,"\"[^\"]\"") {
            }
        }
        /// <summary>
        /// like true or false
        /// </summary>
        public class RuleBool : RuleRegex {
            public RuleBool(Rule Parent)
                : base(Parent,"(true|false)") {
            }
        }
        /// <summary>
        /// one or more whitespace
        /// </summary>
        public class RuleSpace : RuleRegex {
            public RuleSpace(Rule Parent)
                : base(Parent,"[ \\t]+") {
            }
        }
        /// <summary>
        /// 0 or more whitespace
        /// </summary>
       /* public class RuleSpaceOptional : RuleRegex {
            private static RuleSpaceOptional instance;
            public static RuleSpaceOptional Instance {
                get {
                    if (instance == null) {
                        instance = new RuleSpaceOptional();
                    }
                    return instance;
                }
            }
            private RuleSpaceOptional()
                : base("[\\s\\t]*[^\\s\\t]") {      thats not working as expected
            }
        }*/
        /// <summary>
        /// 1 or more EOL
        /// </summary>
        public class RuleEOL : RuleRegex {
            public RuleEOL(Rule Parent)
                : base(Parent,"([\r]*[\n])+") {
                    m_Parent=this;
            }
        }
        public class RuleLPar : RuleRegex {
            public RuleLPar(Rule Parent)
                : base(Parent, s_ManyWhitespace + "\\(" + s_ManyWhitespace) {
            }
        }
        public class RuleRPar : RuleRegex {
            public RuleRPar(Rule Parent)
                : base(Parent, s_ManyWhitespace + "\\)" + s_ManyWhitespace) {
            }
        }
        public class RuleNumber : RuleRegex {
            public RuleNumber(Rule Parent)
                : base(Parent, "[0-9]+('.'[0-9]+)?") {
            }
        }
        #endregion
        #region keywords
        //integrated basic types
        public class RuleBaseType : RuleRegex {
            public RuleBaseType(Rule Parent)
                : base(Parent, "(int|double|bool|string|variant)") {
                //Note: cannot say S*(int|bool)S* because this would cause trouble: MakeSquare(int15);
            }
        }

        #endregion
        #region structures
        //Variable Declaration      int x   |    int x=5
        public class RuleDecl : RuleSequence {
            public RuleDecl(Rule Parent): base(Parent) {
                m_Parent = this;
                this.AddNode(new RuleBaseType(m_Parent));
                this.AddNode(new RuleSpace(m_Parent));
                this.AddNode(new RuleName(m_Parent));
                RuleSequence z = new RuleSequence(m_Parent);
                z.AddNode(new RuleRegex(m_Parent,s_ManyWhitespace + "=" + s_ManyWhitespace));
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
                    this.AddNode(new RuleName(m_Parent));
                    this.AddNode(new RuleRegex(m_Parent,s_ManyWhitespace + "=" + s_ManyWhitespace));
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
                x.AddNode(new RuleRegex(m_Parent,"(\\*|/)"));
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
        public class RulePlusExpr : RuleSequence {   //('+' / '-')? S(NAME /  number / '(' S expression S ')')
            public RulePlusExpr(Rule Parent)
                : base(Parent) {
                this.AddNode(new RuleRegex(m_Parent,"(\\+|\\-)?"));
                RuleAlternative x = new RuleAlternative(Parent);
                x.AddNode( new RuleName(Parent));
                x.AddNode( new RuleNumber(Parent));
                this.AddNode(x);
            }
        }
        public class RuleExpr : RuleSequence {  //multiplicative_expression (S ('+' / '-') S multiplicative_expression)* 
            public RuleExpr(Rule Parent)
                : base(Parent) {
                    m_Parent = this;
                    this.AddNode(new RuleMultExpr(m_Parent));
                    RuleSequence x = new RuleSequence(m_Parent);
                    x.AddNode(new RuleRegex(m_Parent,"(\\+|\\-)"));
                    x.AddNode(new RuleMultExpr(m_Parent));
                    RuleMultiple y = new RuleMultiple(m_Parent, 0);
                y.AddNode(x);
                this.AddNode(y);
            }
        }
        public class RuleFunctionDecl : RuleSequence {  //'function ' NAME S*'(' PARAMDECL? ')' S* RETDECL? S* '{' (COMMENT | EOL) FUNCBODY '}' EOL? 
            public RuleFunctionDecl(Rule Parent)
                : base(Parent) {
                    m_Parent = this;
                    this.AddNode(new RuleRegex(m_Parent,"function[ \\t]+"));
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
            }
        }
        public class RuleParamDecl : RuleSequence {  // BASETYPE S NAME S (, BASETYPE S NAME S )*
            public RuleParamDecl(Rule Parent)
                : base(Parent) {
                    m_Parent = this;
                    this.AddNode(new RuleBaseType(m_Parent));
                    this.AddNode(new RuleSpace(m_Parent));
                    this.AddNode(new RuleName(m_Parent));
                    RuleSequence x = new RuleSequence(m_Parent);
                    x.AddNode(new RuleRegex(m_Parent,s_ManyWhitespace +
                    "," + s_ManyWhitespace));
                x.AddNode(new RuleBaseType(m_Parent));
                x.AddNode(new RuleSpace(m_Parent));
                x.AddNode(new RuleName(m_Parent));
                RuleMultiple y = new RuleMultiple(m_Parent, 0);
                y.AddNode(x);
                this.AddNode(y);
            }
        }
        public class RuleRetDecl : RuleSequence {  //-> BASETYPE S (, BASETYPE S )*
            public RuleRetDecl(Rule Parent)
                : base(Parent) {
                    this.AddNode(new RuleRegex(m_Parent,"->" + s_ManyWhitespace));
                this.AddNode( new RuleBaseType(Parent));
                RuleSequence x = new RuleSequence(Parent);
                x.AddNode(new RuleRegex(m_Parent,s_ManyWhitespace +
                    "," + s_ManyWhitespace));
                x.AddNode( new RuleBaseType(Parent));
                RuleMultiple y = new RuleMultiple(Parent,0);
                y.AddNode(x);
                this.AddNode(y);
            }
        }
        #endregion

        RuleOption Rules;
        public Tokenizer() {
            //add all Rules that could make up one complete Line to a list
            //this list will be evaluated again and again until every text-line is processed
            // make sure to add the more specific Rules at the beginning
            Rules = new RuleOption(null);
            Rules.AddNode(new RuleComment(null));
            Rules.AddNode( new RuleFunctionDecl(null));
            Rules.AddNode( new RuleDecl(null));
            Rules.AddNode( new RuleAssign(null));
            Rules.AddNode( new RuleExpr(null));
            Rules.AddNode( new RuleEOL(null));
        }
        public Token Tokenize(String stream) {
            int Pos = 0;
            Token Result;
            Token FileNode = new Token();
            FileNode.SetValue("MyFile", 0, stream,stream);
            DateTime _start=DateTime.Now;
            while(Pos<stream.Length) {
                Result=Rules.Evaluate(stream, ref Pos);
                if (Result != null && Result.IsValid()) {
                    FileNode.AddLast(Result);
                } else {
                    break;
                }
            }
            DateTime _end = DateTime.Now;
            TimeSpan x = _start - _end;
            return FileNode;
        }
    };
}
