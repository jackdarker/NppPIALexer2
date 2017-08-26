using System.Collections.Generic;
using System;
using System.Windows.Forms;

namespace NppPIALexer2.Tag
{
    public class PythonTag : ITag
    {
        public PythonTag(string srcFile, int lineNo, string tagName, TagType tagType, AccessType accessType, string belongTo)
            : base(srcFile, lineNo, tagName, tagType, accessType, belongTo, "")
        { }

        public override Language Lang { get { return Language.Python; } }

        public override bool BindToTreeNode(TreeNode node)
        {
            node.Text = TagName;
            node.ToolTipText = SourceFile;
            switch (TagType)
            {
                case TagType.Python_Class:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Python_Class;
                    break;
                case TagType.Python_Function:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Python_Function;
                    break;
                case TagType.Python_Import:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Python_Import;
                    break;
                case TagType.Python_Method:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Python_Method;
                    break;
                case TagType.Python_Variable:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Python_Variable;
                    break;
                case TagType.Python_Field:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Python_Field;
                    return false;
            }
            return true;
        }
    }

    public class PythonTagParser : TagParser
    {
        public override Language Lang { get { return Language.Python; } }

        string _args = @"--excmd=number --fields=aksST --sort=no --regex-python=""/^[ \t]*self\.([a-zA-Z0-9_]+)[ \t]*=/\1/F,Field,Fileds/"" --language-force=python";
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
            
            AccessType accessType = AccessType.Other;
            string belongTo = "";
            if (fields.Length == 5)
            {
                string[] arr = fields[4].Split(':');
                if (arr[0].ToLower() == "access")
                    accessType = _2Enum<AccessType>(arr[1]);
            }
            else if (fields.Length == 6)
            {
                string[] arr0 = fields[4].Split(':');
                string[] arr1 = fields[5].Split(':');
                belongTo = arr0[1];
                accessType = _2Enum<AccessType>(arr1[1]);
            }

            return new PythonTag(file, lineNo, tagName, tagType, accessType, belongTo);
        }

        protected override TagType StrToTagType(string sTagType)
        {
            switch (sTagType)
            {
                case "c":
                    return TagType.Python_Class;
                case "i":
                    return TagType.Python_Import;
                case "m":
                    return TagType.Python_Method;
                case "f":
                    return TagType.Python_Function;
                case "v":
                    return TagType.Python_Variable;
                case "F":
                    return TagType.Python_Field;
                default:
                    throw new Exception(string.Format("Unknown Python tag type '{0}'", sTagType));
            }
        }
    }
}