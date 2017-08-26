using System.Collections.Generic;
using System;
using System.Windows.Forms;

namespace NppPIALexer2.Tag
{
    public class JavaTagParser : TagParser
    {
        public override Language Lang { get { return Language.Java; } }

        string _args = "--excmd=number --fields=aksST --sort=no --language-force=Java";
        public override string Args { get { return _args; } }

        public override ITag Parse(string line)
        {
            string file, tagName, belongTo, signature;
            int lineNo;
            AccessType accessType;
            TagType tagType;
            if (_Parse(line, out tagName, out file, out lineNo, out tagType, out belongTo, out accessType, out signature))
                //return new JavaTag(file, lineNo, tagName, tagType, accessType, belongTo, signature);
                return new JavaTag(file, lineNo, tagName, tagType, accessType, belongTo, signature);
            else
                return null;
        }

        protected override TagType StrToTagType(string sTagType)
        {
            switch (sTagType)
            {
                case "c":
                    return TagType.Java_Class;
                case "e":
                    return TagType.Java_EnumConstant;
                case "f":
                    return TagType.Java_Field;
                case "g":
                    return TagType.Java_Enum;
                case "i":
                    return TagType.Java_Interface;
                case "l":
                    return TagType.Java_LocalVar;
                case "m":
                    return TagType.Java_Method;
                case "p":
                    return TagType.Java_Package;
                default:
                    throw new Exception(string.Format("Unknown Java tag type '{0}'", sTagType));
            }
        }
    }

    public class JavaTag : ITag
    {
        public JavaTag(string srcFile, int lineNo, string tagName, TagType tagType, AccessType accessType, string belongTo, string signature)
            : base(srcFile, lineNo, tagName, tagType, accessType, belongTo, signature)
        { }

        public override Language Lang { get { return Language.Java; } }

        public override bool BindToTreeNode(TreeNode node)
        {
            node.Text = TagName;
            node.ToolTipText = Signature;
            switch (TagType)
            {
                case TagType.Java_Class:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Java_Class;
                    break;
                case TagType.Java_EnumConstant:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Java_EnumConstant;
                    break;
                case TagType.Java_Field:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Java_Field;
                    break;
                case TagType.Java_Enum:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Java_Enum;
                    break;
                case TagType.Java_Interface:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Java_Interface;
                    break;
                case TagType.Java_LocalVar:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Java_LocalVar;
                    break;
                case TagType.Java_Method:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Java_Method;
                    break;
                case TagType.Java_Package:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Java_Package;
                    break;
            }
            return true;
        }
    }
}