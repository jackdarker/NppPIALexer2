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
            Object m_Type;
            public void SetValue(String value, int Start, object Type) {
                m_Value = value ;
                m_PosStart = Start;
                m_PosEnd = Start + value.Length;
                m_Type = Type;
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
                if (m_Type == null)
                    return "";
                return m_Type.ToString();

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
        static bool _debug = true;
        abstract public class Rule {
            abstract public Token Evaluate(String stream, ref int pos);
        }
        static string s_ManyWhitespace = "[\\s\\t]*";
        #region BaseRules
        /// <summary>
        /// match all
        /// </summary>
        public class RuleSequence : Rule {
            protected LinkedList<Rule> m_Nodes = new LinkedList<Rule>();
            public RuleSequence()
                : base() {
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
            public RuleAlternative(): base() {
            }
            public override Token Evaluate(String stream, ref int pos) {
                int PosSave = pos;
                Token Result = null;
                LinkedList<Rule>.Enumerator Nodes = m_Nodes.GetEnumerator();
                while (Nodes.MoveNext()) {
                    Result = Nodes.Current.Evaluate(stream, ref pos);
                    if (Result != null ) {
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
            public RuleMultiple(int MinMatches)
                : base() {
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
            public RuleOption()
                : base() {
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
            public RuleRegex(String regex) : base() {
                    m_Regex = new Regex(regex, RegexOptions.Singleline);
            }
            protected Rule m_NodeReplacement;
            public RuleRegex(String regex, Rule NodeReplace)  : base() {
                m_Regex = new Regex(regex, RegexOptions.Singleline);
                m_NodeReplacement = NodeReplace;
            }
            public override Token Evaluate(String stream, ref int pos) {
                int PosSave = pos;
                Token Result=null ;
                // Match the regular expression pattern against a text string.
                // https://msdn.microsoft.com/de-de/library/az24scfc(v=vs.100).aspx
                Match m = m_Regex.Match(stream, pos);
                if (m.Success && m.Index==pos) {
                    Result = new Token();
                    Result.SetValue(m.Value, m.Index,
                        (this.m_NodeReplacement == null) ? this.GetType() : this.m_NodeReplacement.GetType());//m.Groups[0].Value);
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
            private static RuleComment instance;
            public static RuleComment Instance {
                get {
                    if (instance == null) {
                        instance = new RuleComment();
                    }
                    return instance;
                }
            }
            private RuleComment()   : base() {
                this.AddNode(new RuleRegex("//[^\\r\\n]*", this));
                this.AddNode(RuleEOL.Instance);
            }
        }
        public class RuleName : RuleRegex {
            private static RuleName instance;
            public static RuleName Instance {
                get {
                    if (instance == null) {
                        instance = new RuleName();
                    }
                    return instance;
                }
            }
            private RuleName()
                : base("[A-Za-z_][A-Za-z0-9_]*") {
            }
        }
        /// <summary>
        /// like "asd43214fdg"
        /// </summary>
        public class RuleString : RuleRegex {
            private static RuleString instance;
            public static RuleString Instance {
                get {
                    if (instance == null) {
                        instance = new RuleString();
                    }
                    return instance;
                }
            }
            private RuleString()
                : base("\"[^\"]\"") {
            }
        }
        /// <summary>
        /// like true or false
        /// </summary>
        public class RuleBool : RuleRegex {
            private static RuleBool instance;
            public static RuleBool Instance {
                get {
                    if (instance == null) {
                        instance = new RuleBool();
                    }
                    return instance;
                }
            }
            private RuleBool()
                : base("(true|false)") {
            }
        }
        /// <summary>
        /// one or more whitespace
        /// </summary>
        public class RuleSpace : RuleRegex {
            private static RuleSpace instance;
            public static RuleSpace Instance {
                get {
                    if (instance == null) {
                        instance = new RuleSpace();
                    }
                    return instance;
                }
            }
            private RuleSpace()
                : base("[\\s\\t]+") {
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
            private static RuleEOL instance;
            public static RuleEOL Instance {
                get {
                    if (instance == null) {
                        instance = new RuleEOL();
                    }
                    return instance;
                }
            }
            private RuleEOL()
                : base("([\r]*[\n])+") {
            }
        }
        public class RuleLPar : RuleRegex {
            private static RuleLPar instance;
            public static RuleLPar Instance {
                get {
                    if (instance == null) {
                        instance = new RuleLPar();
                    }
                    return instance;
                }
            }
            private RuleLPar()
                : base(s_ManyWhitespace+"\\(") {
            }
        }
        public class RuleRPar : RuleRegex {
            private static RuleRPar instance;
            public static RuleRPar Instance {
                get {
                    if (instance == null) {
                        instance = new RuleRPar();
                    }
                    return instance;
                }
            }
            private RuleRPar()
                : base(s_ManyWhitespace+"\\)") {
            }
        }
        public class RuleNumber : RuleRegex {
            private static RuleNumber instance;
            public static RuleNumber Instance {
                get {
                    if (instance == null) {
                        instance = new RuleNumber();
                    }
                    return instance;
                }
            }
            private RuleNumber()
                : base("[0-9]+('.'[0-9]+)?") {
            }
        }
        #endregion
        #region keywords
        //integrated basic types
        public class RuleBaseType : RuleRegex {
            private static RuleBaseType instance;
            public static RuleBaseType Instance {
                get {
                    if (instance == null) {
                        instance = new RuleBaseType();
                    }
                    return instance;
                }
            }
            private RuleBaseType()
                : base("int|double|bool|string|variant",instance) {
            }
        }

        #endregion
        #region structures
        //Variable Declaration      int x   |    int x=5
        public class RuleDecl : RuleSequence {
            private static RuleDecl instance;
            public static RuleDecl Instance {
                get {
                    if (instance == null) {
                        instance = new RuleDecl();
                    }
                    return instance;
                }
            }
            private RuleDecl(): base() {
                this.AddNode(RuleBaseType.Instance);
                this.AddNode(RuleSpace.Instance);
                this.AddNode(RuleName.Instance);
                this.AddNode(RuleEOL.Instance);
            }
        }
        /// <summary>
        /// like x=5.12
        /// </summary>
        public class RuleAssign : RuleSequence {
            private static RuleAssign instance;
            public static RuleAssign Instance {
                get {
                    if (instance == null) {
                        instance = new RuleAssign();
                    }
                    return instance;
                }
            }
            private RuleAssign()
                : base() {
                    this.AddNode(RuleName.Instance);
                    this.AddNode(new RuleRegex(s_ManyWhitespace + "=" + s_ManyWhitespace,this));
                    RuleAlternative x = new RuleAlternative();
                    x.AddNode(RuleName.Instance);
                    x.AddNode(RulePlusExpr.Instance);
                    x.AddNode(RuleString.Instance);
                    x.AddNode(RuleBool.Instance);
                    this.AddNode(x);
                    this.AddNode(RuleEOL.Instance);
            }
        }
        public class RuleMultExpr : RuleSequence {   //power_expression (S ('*' / '/') S power_expression)*;
            private static RuleMultExpr instance;
            public static RuleMultExpr Instance {
                get {
                    if (instance == null) {
                        instance = new RuleMultExpr();
                    }
                    return instance;
                }
            }
            private RuleMultExpr() : base() {
                this.AddNode(RulePlusExpr.Instance);
                RuleSequence x = new RuleSequence();
                x.AddNode(new RuleRegex("(\\*|/)",this));
                x.AddNode(RulePlusExpr.Instance);
                RuleMultiple y = new RuleMultiple(0);
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
                this.AddNode(RulePlusExpr.Instance);
                RuleSequence x = new RuleSequence();
                x.AddNode(new RuleRegex("(^)"));
                x.AddNode(RulePlusExpr.Instance);
                RuleMultiple y = new RuleMultiple(0);
                y.AddNode(x);
                this.AddNode(y);
            }
        }*/
        public class RulePlusExpr : RuleSequence {   //('+' / '-')? S(NAME /  number / '(' S expression S ')')
            private static RulePlusExpr instance;
            public static RulePlusExpr Instance {
                get {
                    if (instance == null) {
                        instance = new RulePlusExpr();
                    }
                    return instance;
                }
            }
            private RulePlusExpr()
                : base() {
                this.AddNode(new RuleRegex("(\\+|\\-)?",this));
                RuleAlternative x = new RuleAlternative();
                x.AddNode(RuleName.Instance);
                x.AddNode(RuleNumber.Instance);
                this.AddNode(x);
            }
        }
        public class RuleExpr : RuleSequence {  //multiplicative_expression (S ('+' / '-') S multiplicative_expression)* 
            private static RuleExpr instance;
            public static RuleExpr Instance {
                get {
                    if (instance == null) {
                        instance = new RuleExpr();
                    }
                    return instance;
                }
            }
            private RuleExpr()  : base() {
                this.AddNode(RuleMultExpr.Instance);
                RuleSequence x = new RuleSequence();
                x.AddNode(new RuleRegex("(\\+|\\-)",this));
                x.AddNode(RuleMultExpr.Instance);
                RuleMultiple y = new RuleMultiple(0);
                y.AddNode(x);
                this.AddNode(y);
            }
        }
        public class RuleFunctionDecl : RuleSequence {  //'function ' NAME S*'(' PARAMDECL? ')' S* RETDECL? S* '{' (COMMENT | EOL) FUNCBODY '}' EOL? 
            private static RuleFunctionDecl instance;
            public static RuleFunctionDecl Instance {
                get {
                    if (instance == null) {
                        instance = new RuleFunctionDecl();
                    }
                    return instance;
                }
            }
            private RuleFunctionDecl()
                : base() {
                    this.AddNode(new RuleRegex("function[\\s\\t]+",this));
                this.AddNode(RuleName.Instance);
                this.AddNode(RuleLPar.Instance);
                RuleOption y = new RuleOption();
                y.AddNode(RuleParamDecl.Instance);
                this.AddNode(y);
                this.AddNode(RuleRPar.Instance);
                y = new RuleOption();
                y.AddNode(RuleRetDecl.Instance);
                this.AddNode(y);
                this.AddNode(RuleEOL.Instance);
            }
        }
        public class RuleParamDecl : RuleSequence {  // BASETYPE S NAME S (, BASETYPE S NAME S )*
            private static RuleParamDecl instance;
            public static RuleParamDecl Instance {
                get {
                    if (instance == null) {
                        instance = new RuleParamDecl();
                    }
                    return instance;
                }
            }
            private RuleParamDecl()
                : base() {
                this.AddNode(RuleBaseType.Instance);
                this.AddNode(RuleSpace.Instance);
                this.AddNode(RuleName.Instance);
                RuleSequence x = new RuleSequence();
                x.AddNode(new RuleRegex(s_ManyWhitespace + 
                    "," + s_ManyWhitespace, this)); 
                x.AddNode(RuleBaseType.Instance);
                x.AddNode(RuleSpace.Instance);
                x.AddNode(RuleName.Instance);
                RuleMultiple y = new RuleMultiple(0);
                y.AddNode(x);
                this.AddNode(y);
            }
        }
        public class RuleRetDecl : RuleSequence {  //-> BASETYPE S (, BASETYPE S )*
            private static RuleRetDecl instance;
            public static RuleRetDecl Instance {
                get {
                    if (instance == null) {
                        instance = new RuleRetDecl();
                    }
                    return instance;
                }
            }
            private RuleRetDecl()
                : base() {
                    this.AddNode(new RuleRegex("->" + s_ManyWhitespace, this));
                this.AddNode(RuleBaseType.Instance);
                RuleSequence x = new RuleSequence();
                x.AddNode(new RuleRegex(s_ManyWhitespace +
                    "," + s_ManyWhitespace, this));
                x.AddNode(RuleBaseType.Instance);
                RuleMultiple y = new RuleMultiple(0);
                y.AddNode(x);
                this.AddNode(y);
            }
        }
        #endregion

        RuleOption Rules;
        public Tokenizer() {
            Rules = new RuleOption();
            Rules.AddNode(RuleComment.Instance);
            Rules.AddNode(RuleFunctionDecl.Instance);
            Rules.AddNode(RuleDecl.Instance);
            Rules.AddNode(RuleAssign.Instance);
            Rules.AddNode(RuleExpr.Instance);
            Rules.AddNode(RuleEOL.Instance);
        }
        public LinkedList<Token> Tokenize(String stream) {
            int Pos = 0;
            Token Result;
            LinkedList<Token> Tree = new LinkedList<Token>();
            DateTime _start=DateTime.Now;
            while(Pos<stream.Length) {
                Result=Rules.Evaluate(stream, ref Pos);
                if (Result != null && Result.IsValid()) {
                    Tree.AddLast(Result);
                } else {
                    break;
                }
            }
            DateTime _end = DateTime.Now;
            TimeSpan x = _start - _end;
            return Tree;
        }
    };
}
