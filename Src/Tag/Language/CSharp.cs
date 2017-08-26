using System.Collections.Generic;
using System;
using System.Windows.Forms;

namespace NppPIALexer2.Tag
{
    public class CSharpTagParser : TagParser
    {
        public override Language Lang { get { return Language.CSharp; } }

        string _args = "--excmd=number --fields=aksST --sort=no --language-force=C#";
        public override string Args { get { return _args; } }

        public override ITag Parse(string line)
        {
            string file, tagName, belongTo, signature;
            int lineNo;
            AccessType accessType;
            TagType tagType;
            if (_Parse(line, out tagName, out file, out lineNo, out tagType, out belongTo, out accessType, out signature))
                return new CSharpTag(file, lineNo, tagName, tagType, accessType, belongTo, signature);
            else
                return null;
        }

        protected override TagType StrToTagType(string sTagType)
        {
            switch (sTagType)
            {
                case "c":
                    return TagType.CSharp_Class;
                case "d":
                    return TagType.CSharp_Macro;
                case "e":
                    return TagType.CSharp_Enum;
                case "E":
                    return TagType.CSharp_Event;
                case "f":
                    return TagType.CSharp_Field;
                case "g":
                    return TagType.CSharp_EnumerationName;
                case "i":
                    return TagType.CSharp_Interface;
                case "l":
                    return TagType.CSharp_LocalVar;
                case "m":
                    return TagType.CSharp_Method;
                case "n":
                    return TagType.CSharp_Namespace;
                case "p":
                    return TagType.CSharp_Property;
                case "s":
                    return TagType.CSharp_Struct;
                case "t":
                    return TagType.CSharp_Typedef;
                default:
                    throw new Exception(string.Format("Unknown C# tag type '{0}'", sTagType));
            }
        }
    }

    public class CSharpTag : ITag
    {
        public CSharpTag(string srcFile, int lineNo, string tagName, TagType tagType, AccessType accessType, string belongTo, string signature)
            : base(srcFile, lineNo, tagName, tagType, accessType, belongTo, signature)
        { }

        public override Language Lang { get { return Language.CSharp; } }

        public override bool BindToTreeNode(TreeNode node)
        {
            node.Text = TagName;
            node.ToolTipText = Signature;
            switch (TagType)
            {
                case TagType.CSharp_Class:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_CSharp_Class;
                    break;
                case TagType.CSharp_Enum:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_CSharp_Enum;
                    break;
                case TagType.CSharp_EnumerationName:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_CSharp_EnumerationName;
                    break;
                case TagType.CSharp_Event:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_CSharp_Event;
                    break;
                case TagType.CSharp_Field:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_CSharp_Field;
                    break;
                case TagType.CSharp_Interface:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_CSharp_Interface;
                    break;
                case TagType.CSharp_LocalVar:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_CSharp_LocalVar;
                    break;
                case TagType.CSharp_Macro:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_CSharp_Macro;
                    break;
                case TagType.CSharp_Method:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_CSharp_Method;
                    break;
                case TagType.CSharp_Namespace:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_CSharp_Namespace;
                    break;
                case TagType.CSharp_Property:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_CSharp_Property;
                    break;
                case TagType.CSharp_Struct:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_CSharp_Struct;
                    break;
                case TagType.CSharp_Typedef:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_CSharp_Typedef;
                    break;
            }
            return true;
        }
    }
}