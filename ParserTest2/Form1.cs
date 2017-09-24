using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using NppPIALexer2;

namespace ParserTest2 {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
            StringBuilder stringToRead = new StringBuilder();
            stringToRead.AppendLine("#include \"StandardObj.seq\"");
            stringToRead.AppendLine("int Test");
            stringToRead.AppendLine("using \"CAN.lvlibp:CAN.lvclass\" as CAN");
            stringToRead.AppendLine("bool Test2=true");
            stringToRead.AppendLine("double Te4=true");
            stringToRead.AppendLine("//a comment");
            stringToRead.AppendLine("int Test2 //another comment");
            stringToRead.AppendLine("Test=3+2*5");
            stringToRead.AppendLine("function boom()->int count");
            stringToRead.AppendLine("function kaboom(bool count,int size) -> string test, double");
            stringToRead.AppendLine("Test=X\nTest2=Y");
            stringToRead.AppendLine("{ xcv }");

            this.textBox2.Text = stringToRead.ToString();
        }
        public class TreeNodeBuilder : Tokenizer.NodeBuilder {
            TreeNodeCollection m_Tree;
            LinkedList<int> m_Index = new LinkedList<int>();
            
            public TreeNodeBuilder(TreeNodeCollection Tree):base() {
                m_Tree = Tree;
            }
            override public void Visit(Tokenizer.Token token) {
                TreeNode Node = new TreeNode(token.GetValue(m_Index.Count <= 2 && 1 <= m_Index.Count));
                Node.Tag = token;
                LinkedList<int>.Enumerator y = m_Index.GetEnumerator();
                TreeNodeCollection trc = m_Tree;
                while (y.MoveNext()) {
                    trc = trc[y.Current].Nodes;
                }
                int Index = trc.Add(Node);
                m_Index.AddLast(Index);
                LinkedList<Tokenizer.Token>.Enumerator x = token.GetEnumerator();
                while (x.MoveNext()) {
                    x.Current.InspectNodes(this);
                }
                m_Index.RemoveLast();
            }
        }
        void TokensToTree(TreeNodeCollection Tree, Tokenizer.Token Tokens) {
            Tree.Clear();
            TreeNodeBuilder visitor = new TreeNodeBuilder(Tree);
            Tokens.InspectNodes(visitor);
            /*
            LinkedList<Tokenizer.Token>.Enumerator x =Tokens.GetEnumerator();
            while (x.MoveNext()) {
                    x.Current.InspectNodes(visitor);
            }*/

        }
        Tokenizer.Token m_Tokens;
        void UpdateTree() { 
            
            Tokenizer test = new Tokenizer( );
            //LinkedList<Tokenizer.Token> Tokens = 
            m_Tokens=test.Tokenize(this.textBox2.Text);
            /*LinkedList<Tokenizer.Token>.Enumerator x=m_Tokens.GetEnumerator();
            while (x.MoveNext()) {
                    Console.WriteLine(x.Current.GetValue(true));
            }*/
            treeView1.BeginUpdate();
            TokensToTree(treeView1.Nodes,m_Tokens);
            treeView1.EndUpdate();
            treeView1.Nodes[0].Expand();

            
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e) {
            Tokenizer.Token x = (Tokenizer.Token)e.Node.Tag;
            this.textBox1.Text = x.GetNodeType().ToString() + "\r\nin\r\n" + x.GetTopNodeType().ToString();
        }

        private void button1_Click(object sender, EventArgs e) {
            UpdateTree();
        }

        private void button2_Click(object sender, EventArgs e) {
            textBox3.Clear();
            textBox4.Clear();
            if (m_Tokens == null) return;
            Parser2 parser = new Parser2();
            LinkedList<Tokenizer.Token> _Tokens = new LinkedList<Tokenizer.Token>();
            _Tokens.AddLast(m_Tokens);
            parser.ParseTokens(_Tokens);
            LinkedList<Parser2.Context.Log>.Enumerator x=parser.GetLogs().GetEnumerator();
            while(x.MoveNext()) {
                if(x.Current.m_Error > 0) {
                    textBox4.Text += x.Current.m_Text +" at "+x.Current.m_Cmd.AsText()+ "\r\n";
                }

            }
            LinkedList<Parser2.CmdBase>.Enumerator Result=parser.GetCmds("").GetEnumerator();
            while(Result.MoveNext()) {
                textBox3.Text += ( Result.Current.AsText() + "\r\n" );

            }
        }
    }
}
