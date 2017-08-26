using System.Collections.Generic;
using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace NppPIALexer2.Tag
{
    public class CppTagParser : TagParser
    {
        public override Language Lang { get { return Language.Cpp; } }

        string _args = "--excmd=number --fields=aksST --sort=no --language-force=C++";
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

            return new CppTag(file, lineNo, tagName, tagType, accessType, belongTo, signature);
        }

        protected override TagType StrToTagType(string sTagType)
        {
            switch (sTagType)
            {
                case "c":
                    return TagType.Cpp_Class;
                case "d":
                    return TagType.Cpp_Macro;
                case "e":
                    return TagType.Cpp_Enumerator;
                case "f":
                    return TagType.Cpp_Function;
                case "g":
                    return TagType.Cpp_Enumeration;
                case "l":
                    return TagType.Cpp_LocalVar;
                case "m":
                    return TagType.Cpp_ClassMember;
                case "n":
                    return TagType.Cpp_Namespace;
                case "p":
                    return TagType.Cpp_FunctionPrototype;
                case "s":
                    return TagType.Cpp_Struct;
                case "t":
                    return TagType.Cpp_Typedef;
                case "u":
                    return TagType.Cpp_Union;
                case "v":
                    return TagType.Cpp_Variable;
                case "x":
                    return TagType.Cpp_Declaration;
                default:
                    throw new Exception(string.Format("Unknown C++ tag type '{0}'", sTagType));
            }
        }
    }

    public class CppTag : ITag
    {
        public CppTag(string srcFile, int lineNo, string tagName, TagType tagType, AccessType accessType, string belongTo, string signature)
            : base(srcFile, lineNo, tagName, tagType, accessType, belongTo, signature)
        { }

        public override Language Lang { get { return Language.Cpp; } }

        public override bool BindToTreeNode(TreeNode node)
        {
            node.Text = TagName;
            node.ToolTipText = Signature;
            switch (TagType)
            {
                case TagType.Cpp_Class:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Cpp_Class;
                    break;
                case TagType.Cpp_ClassMember:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Cpp_ClassMember;
                    break;
                case TagType.Cpp_Declaration:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Cpp_Declaration;
                    break;
                case TagType.Cpp_Enumeration:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Cpp_EnumerationName;
                    break;
                case TagType.Cpp_Enumerator:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Cpp_Enum;
                    break;
                case TagType.Cpp_Function:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Cpp_Function;
                    break;
                case TagType.Cpp_FunctionPrototype:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Cpp_FunctionPrototype;
                    break;
                case TagType.Cpp_LocalVar:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Cpp_LocalVar;
                    break;
                case TagType.Cpp_Macro:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Cpp_Macro;
                    break;
                case TagType.Cpp_Namespace:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Cpp_Namespace;
                    break;
                case TagType.Cpp_Struct:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Cpp_Struct;
                    break;
                case TagType.Cpp_Typedef:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Cpp_Typedef;
                    break;
                case TagType.Cpp_Union:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Cpp_Union;
                    break;
                case TagType.Cpp_Variable:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Cpp_Variable;
                    break;
            }
            return true;
        }
    }
}