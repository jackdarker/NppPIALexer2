using System.Collections.Generic;
using System;
using System.Windows.Forms;

namespace NppPIALexer2.Tag
{
    public class FlexTag : ITag
    {
        public FlexTag(string srcFile, int lineNo, string tagName, TagType tagType, string belongTo)
            : base(srcFile, lineNo, tagName, tagType, AccessType.Other, belongTo, "")
        { }

        public override Language Lang { get { return Language.Flex; } }

        public override bool BindToTreeNode(TreeNode node)
        {
            node.Text = TagName;
            node.ToolTipText = SourceFile;
            switch (TagType)
            {
                case TagType.Flex_Class:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Flex_Class;
                    break;
                case TagType.Flex_Function:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Flex_Function;
                    break;
                case TagType.Flex_GlobalVar:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Flex_GlobalVar;
                    break;
                case TagType.Flex_Method:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Flex_Method;
                    break;
                case TagType.Flex_Property:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Flex_Property;
                    break;
                case TagType.Flex_Mxtag:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Flex_Mxtag;
                    break;
            }
            return true;
        }
    }

    public class FlexTagParser : TagParser
    {
        public override Language Lang { get { return Language.Flex; } }

        string _args = "--excmd=number --fields=aksST --sort=no --language-force=Flex";
        public override string Args { get { return _args; } }

        public override ITag Parse(string line)
        {
            string file, tagName, belongTo, signature;
            int lineNo;
            AccessType accessType;
            TagType tagType;
            if (_Parse(line, out tagName, out file, out lineNo, out tagType, out belongTo, out accessType, out signature))
                return new FlexTag(file, lineNo, tagName, tagType, belongTo);
            else
                return null;
        }

        protected override TagType StrToTagType(string sTagType)
        {
            switch (sTagType)
            {
                case "c":
                    return TagType.Flex_Class;
                case "f":
                    return TagType.Flex_Function;
                case "m":
                    return TagType.Flex_Method;
                case "p":
                    return TagType.Flex_Property;
                case "v":
                    return TagType.Flex_GlobalVar;
                case "x":
                    return TagType.Flex_Mxtag;
                default:
                    throw new Exception(string.Format("Unknown Flex tag type '{0}'", sTagType));
            }
        }
    }
}