using System.Collections.Generic;
using System;
using System.Windows.Forms;

namespace NppPIALexer2.Tag
{
    public class AsmTag : ITag
    {
        public AsmTag(string srcFile, int lineNo, string tagName, TagType tagType)
            : base(srcFile, lineNo, tagName, tagType, AccessType.Other, "", "")
        { }

        public override Language Lang { get { return Language.Asm; } }

        public override bool BindToTreeNode(TreeNode node)
        {
            node.Text = TagName;
            node.ToolTipText = SourceFile;
            switch (TagType)
            {
                case TagType.ASM_Define:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_ASM_Define;
                    break;
                case TagType.ASM_Label:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_ASM_Label;
                    break;
                case TagType.ASM_Macro:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_ASM_Macro;
                    break;
                case TagType.ASM_Type:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_ASM_Type;
                    break;
            }
            return true;
        }
    }

    public class AsmTagParser : TagParser
    {
        public override Language Lang { get { return Language.Asm; } }

        string _args = @"--excmd=number --fields=aksST --sort=no --language-force=Asm";
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
            return new AsmTag(file, lineNo, tagName, tagType);
        }

        protected override TagType StrToTagType(string sTagType)
        {
            switch (sTagType)
            {
                case "d":
                    return TagType.ASM_Define;
                case "l":
                    return TagType.ASM_Label;
                case "m":
                    return TagType.ASM_Macro;
                case "t":
                    return TagType.ASM_Type;
                default:
                    throw new Exception(string.Format("Unknown Asm tag type '{0}'", sTagType));
            }
        }
    }
}