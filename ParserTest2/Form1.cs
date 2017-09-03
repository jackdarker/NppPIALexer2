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
            UpdateTree();
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
                TreeNode Node = new TreeNode(token.GetValue(m_Index.Count<=1));
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
        void TokensToTree(TreeNodeCollection Tree, LinkedList<Tokenizer.Token> Tokens) {
            Tree.Clear();
            TreeNodeBuilder visitor = new TreeNodeBuilder(Tree);
            LinkedList<Tokenizer.Token>.Enumerator x =Tokens.GetEnumerator();
            while (x.MoveNext()) {
                    x.Current.InspectNodes(visitor);
            }

        }
        
        void UpdateTree() { 
            StringBuilder stringToRead = new StringBuilder();
            stringToRead.AppendLine("int Test");
            stringToRead.AppendLine("//a comment");
            //stringToRead.AppendLine("int Test2 //another comment");
            stringToRead.AppendLine("3+2*5");
            //stringToRead.AppendLine("");
            stringToRead.AppendLine("3*2+5");
            stringToRead.AppendLine("Test=X\nTest2=Y");

            Tokenizer test = new Tokenizer( );
            LinkedList<Tokenizer.Token> Tokens = 
                test.Tokenize(stringToRead.ToString());
            LinkedList<Tokenizer.Token>.Enumerator x=Tokens.GetEnumerator();
            while (x.MoveNext()) {
                    Console.WriteLine(x.Current.GetValue(true));
            }
            treeView1.BeginUpdate();
            TokensToTree(treeView1.Nodes,Tokens);
            treeView1.EndUpdate();

            
        }
    }
}
