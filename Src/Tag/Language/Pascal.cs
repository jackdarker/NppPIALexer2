using System.Collections.Generic;
using System;
using System.Windows.Forms;

namespace NppPIALexer2.Tag
{
    public class PascalTag : ITag
    {
        public PascalTag(string srcFile, int lineNo, string tagName, TagType tagType)
            : base(srcFile, lineNo, tagName, tagType, AccessType.Other, "", "")
        { }

        public override Language Lang { get { return Language.Pascal; } }

        public override bool BindToTreeNode(TreeNode node)
        {
            node.Text = TagName;
            node.ToolTipText = SourceFile;
            switch (TagType)
            {
                case TagType.Pascal_Function:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Pascal_Function;
                    break;
                case TagType.Pascal_Procedure:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Pascal_Procedure;
                    break;
            }
            return true;
        }
    }

    public class PascalTagParser : TagParser
    {
        public override Language Lang { get { return Language.Pascal; } }

        string _args = @"--excmd=number --fields=aksST --sort=no --language-force=Pascal";
        public override string Args { get { return _args; } }

        readonly char _Spliter = (char)0x01; // '☺'
        public override ITag Parse(string line)
        {
            if (line.StartsWith("!"))
                return null;
            string[] fields = line.Split(_Spliter);     // 前4项内容是固定的，分别是：标称名，所在文件，位置，及类型
            string tagName = fields[0];
            string file = fields[1];
            string sLineNo = fields[2];
            int lineNo = int.Parse(sLineNo.Split(';')[0]);
            string sTagType = fields[3];
            TagType tagType = StrToTagType(sTagType);
            return new PascalTag(file, lineNo, tagName, tagType);
        }

        protected override TagType StrToTagType(string sTagType)
        {
            switch (sTagType)
            {
                case "f":
                    return TagType.Pascal_Function;
                case "p":
                    return TagType.Pascal_Procedure;
                default:
                    throw new Exception(string.Format("Unknown Pascal tag type '{0}'", sTagType));
            }
        }
    }
}