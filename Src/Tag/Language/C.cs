using System.Collections.Generic;
using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace NppPIALexer2.Tag
{
    public class CTagParser : TagParser
    {
        public override Language Lang { get { return Language.C; } }

        string _args = "--excmd=number --fields=aksST --sort=no --language-force=C";
        public override string Args { get { return _args; } }

        readonly char _Spliter = (char)0x01; // '☺'
        public override ITag Parse(string line)
        {
            if (line.StartsWith("!"))
                return null;

            string[] fields = line.Split(_Spliter);
            string tagName = fields[0];
            string file = fields[1];
            string sLineNo = fields[2];
            int lineNo = int.Parse(sLineNo.Split(';')[0]);
            string sTagType = fields[3];
            TagType tagType = StrToTagType(sTagType);

            string belongTo = _GetBelongTo(fields);
            if (belongTo.IndexOf("__anon") != -1)  // 匿名枚举，结构等
            {
                //if (belongTo.IndexOf('.') == -1)
                //    belongTo = "";
                //else
                //    belongTo = belongTo.Substring(belongTo.IndexOf('.') + 1);
                return null;
            }
            AccessType accessType = _GetAccessType(fields);
            string signature = _GetSignature(fields);
            string returnType = _GetReturnType(fields);
            if (!string.IsNullOrEmpty(returnType))
                signature += ":" + returnType;

            return new CTag(file, lineNo, tagName, tagType, belongTo, signature);
        }

        protected override TagType StrToTagType(string sTagType)
        {
            switch (sTagType)
            {
                case "c":
                    return TagType.C_Class;
                case "d":
                    return TagType.C_Macro;
                case "e":
                    return TagType.C_Enumerator;
                case "f":
                    return TagType.C_Function;
                case "g":
                    return TagType.C_Enumeration;
                case "l":
                    return TagType.C_LocalVar;
                case "m":
                    return TagType.C_ClassMember;
                case "n":
                    return TagType.C_Namespace;
                case "p":
                    return TagType.C_FunctionPrototype;
                case "s":
                    return TagType.C_Struct;
                case "t":
                    return TagType.C_Typedef;
                case "u":
                    return TagType.C_Union;
                case "v":
                    return TagType.C_Variable;
                case "x":
                    return TagType.C_Declaration;
                default:
                    throw new Exception(string.Format("Unknown C tag type '{0}'", sTagType));
            }
        }
    }

    public class CTag : ITag
    {
        public CTag(string srcFile, int lineNo, string tagName, TagType tagType, string belongTo, string signature)
            : base(srcFile, lineNo, tagName, tagType, AccessType.Other, belongTo, signature)
        { }

        public override Language Lang { get { return Language.C; } }

        public override bool BindToTreeNode(TreeNode node)
        {
            node.Text = TagName;
            node.ToolTipText = Signature;
            switch (TagType)
            {
                case TagType.C_Class:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_C_Class;
                    break;
                case TagType.C_Macro:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_C_Macro;
                    break;
                case TagType.C_Enumerator:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_C_Enumerator;
                    break;
                case TagType.C_Enumeration:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_C_Enumeration;
                    break;
                case TagType.C_Function:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_C_Function;
                    break;
                case TagType.C_LocalVar:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_C_LocalVar;
                    break;
                case TagType.C_ClassMember:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_C_ClassMember;
                    break;
                case TagType.C_Namespace:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_C_Namespace;
                    break;
                case TagType.C_FunctionPrototype:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_C_FunctionPrototype;
                    break;
                case TagType.C_Struct:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_C_Struct;
                    break;
                case TagType.C_Typedef:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_C_Typedef;
                    break;
                case TagType.C_Union:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_C_Union;
                    break;
                case TagType.C_Variable:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_C_Variable;
                    break;
                case TagType.C_Declaration:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_C_Declaration;
                    break;
            }
            return true;
        }
    }
}