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
            stringToRead.AppendLine("int Test");
            stringToRead.AppendLine("bool Test2=true");
            stringToRead.AppendLine("//a comment");
            stringToRead.AppendLine("int Test2 //another comment");
            stringToRead.AppendLine("Test=3+2*5");
            stringToRead.AppendLine("function boom()->int");
            stringToRead.AppendLine("function kaboom(int count,int size) -> string , double");
            stringToRead.AppendLine("Test=X\nTest2=Y");
            this.textBox2.Text = stringToRead.ToString();
        }
        public class TreeNodeBuilder : Tokenizer.NodeBuilder {
            TreeNodeCollection m_Tree;
            LinkedList<int> m_Index = new LinkedList<int>();
            public class Context {
                TreeNode m_Parent;
                public Context(TreeNode Parent) {
                    m_Parent = Parent;
                }
            }
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
        
        void UpdateTree() { 
            
            Tokenizer test = new Tokenizer( );
            //LinkedList<Tokenizer.Token> Tokens = 
            Tokenizer.Token Tokens=test.Tokenize(this.textBox2.Text);
            LinkedList<Tokenizer.Token>.Enumerator x=Tokens.GetEnumerator();
            while (x.MoveNext()) {
                    Console.WriteLine(x.Current.GetValue(true));
            }
            treeView1.BeginUpdate();
            TokensToTree(treeView1.Nodes,Tokens);
            treeView1.EndUpdate();
            treeView1.Nodes[0].Expand();

            
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e) {
            Tokenizer.Token x = (Tokenizer.Token)e.Node.Tag;
            this.textBox1.Text = x.GetNodeType();
        }

        private void button1_Click(object sender, EventArgs e) {
            UpdateTree();
        }
    }
}
