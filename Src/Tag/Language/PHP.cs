using System.Collections.Generic;
using System;
using System.Windows.Forms;

namespace NppPIALexer2.Tag
{
    public class PHPTag : ITag
    {
        public PHPTag(string srcFile, int lineNo, string tagName, TagType tagType, string belongTo)
            : base(srcFile, lineNo, tagName, tagType, AccessType.Other, belongTo, "")
        { }

        public override Language Lang { get { return Language.PHP; } }

        public override bool BindToTreeNode(TreeNode node)
        {
            node.Text = TagName;
            node.ToolTipText = SourceFile;
            switch (TagType)
            {
                case TagType.PHP_Class:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_PHP_Class;
                    break;
                case TagType.PHP_Interface:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_PHP_Interface;
                    break;
                case TagType.PHP_Function:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_PHP_Function;
                    break;
                case TagType.PHP_Constant:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_PHP_Constant;
                    break;
                case TagType.PHP_Variable:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_PHP_Variable;
                    break;
                case TagType.PHP_JsFunction:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_PHP_JsFunction;
                    break;
                case TagType.PHP_Property:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_PHP_Variable;
                    break;
            }
            return true;
        }
    }

    public class PHPTagParser : TagParser
    {
        public override Language Lang { get { return Language.PHP; } }

        string _args = @"--excmd=number --fields=aksST --sort=no --language-force=PHP --regex-php=""/^[ \t]*(private|public|static)[ \t]+function[ \t]+([A-Za-z0-9_]+)[ \t]*\(/\1/f, function, functions/"" --regex-php=""/^[ \t]*[(private|public|static)]+[ \t]+\$([A-Za-z0-9_]+)[ \t]*/\1/p, property, properties/"" --regex-php=""/^[ \t]*(const)[ \t]+([A-Za-z0-9_]+)[ \t]*/\2/d, const, constants/""";
        public override string Args { get { return _args; } }

        public override ITag Parse(string line)
        {
            string file, tagName, belongTo, signature;
            int lineNo;
            AccessType accessType;
            TagType tagType;
            if (_Parse(line, out tagName, out file, out lineNo, out tagType, out belongTo, out accessType, out signature))
            {
                if (tagType == TagType.PHP_Variable)
                    return null;
                return new PHPTag(file, lineNo, tagName, tagType, belongTo);
            }
            else
                return null;
        }

        protected override TagType StrToTagType(string sTagType)
        {
            switch (sTagType)
            {
                case "c":
                    return TagType.PHP_Class;
                case "i":
                    return TagType.PHP_Interface;
                case "d":
                    return TagType.PHP_Constant;
                case "f":
                    return TagType.PHP_Function;
                case "v":
                    return TagType.PHP_Variable;
                case "j":
                    return TagType.PHP_JsFunction;
                case "p":   // 用正则表达式扩展
                    return TagType.PHP_Property;
                default:
                    throw new Exception(string.Format("Unknown PHP tag type '{0}'", sTagType));
            }
        }
    }
}