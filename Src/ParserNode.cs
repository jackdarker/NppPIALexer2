using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NppPIALexer2 {
    interface IParserNode {
        bool HasChildNode();
        bool HasParent();
        IParserNode GetParent();
        void AppendChildNode(IParserNode Child);
        String GetName();
        String GetFullString();
    }

    public class ParserNode : IParserNode {

        private LinkedList<IParserNode> m_Childs = new LinkedList<IParserNode>();
        private IParserNode m_Parent = null;
        public ParserNode() {
            m_Parent = null;
        }
        public ParserNode(IParserNode Parent) {
            m_Parent = Parent;
        }
        public bool IParserNode.HasParent() {
            return m_Parent != null;
        }
        public bool IParserNode.HasChildNode() {
            return m_Childs.Count != 0;
        }
        public IParserNode IParserNode.GetParent() {
            return m_Parent;
        }
        public void IParserNode.AppendChildNode(IParserNode Child) {
            if (Child == null) return;
            m_Childs.AddLast(Child);
        }
        public String IParserNode.GetName() {
            return this.ToString();
        }
        public String IParserNode.GetFullString() {
            return this.ToString();
        }
    }

    public class VariableNode : ParserNode {
        private String m_Name;
        private String m_Type;
        public VariableNode(IParserNode Parent, String Name, String Type):base(Parent) {
            m_Name = Name;
            m_Type = Type;
        }
        public String IParserNode.GetName() {
            return m_Name;
        }
        public String IParserNode.GetFullString() {
            return m_Type + " " + m_Name;
        }
    }
    public class FunctionNode : ParserNode {
        private String m_Name;

        public FunctionNode(IParserNode Parent, String Name)
            : base(Parent) {
            m_Name = Name;
        }
        public String IParserNode.GetName() {
            return m_Name;
        }
        public String IParserNode.GetFullString() {
            return m_Name;
        }
    }
}
