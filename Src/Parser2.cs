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
        public class TreeNodeBuilder : Tokenizer.NodeBuilder {
            public TreeNodeBuilder()
                : base() {
            }
            override public void Visit(Tokenizer.Token token) {

            }
        }
        public Parser2() { 
        }
        public void ParseTokens(Tokenizer.Token x) {
            TreeNodeBuilder _bld = new TreeNodeBuilder();
            x.InspectNodes(_bld);
        }
    }
}
