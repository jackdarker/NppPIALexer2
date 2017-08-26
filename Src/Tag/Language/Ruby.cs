using System.Collections.Generic;
using System;
using System.Windows.Forms;

namespace NppPIALexer2.Tag
{
    public class RubyTag : ITag
    {
        public RubyTag(string srcFile, int lineNo, string tagName, TagType tagType, AccessType accessType, string belongTo)
            : base(srcFile, lineNo, tagName, tagType, accessType, belongTo, "")
        { }

        public override Language Lang { get { return Language.Ruby; } }

        public override bool BindToTreeNode(TreeNode node)
        {
            node.Text = TagName;
            node.ToolTipText = SourceFile;
            switch (TagType)
            {
                case TagType.Ruby_Module:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Ruby_Module;
                    break;
                case TagType.Ruby_Class:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Ruby_Class;
                    break;
                case TagType.Ruby_Method:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Ruby_Method;
                    break;
                case TagType.Ruby_SingletonMethod:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Ruby_SingletonMethod;
                    break;
            }
            return true;
        }
    }

    public class RubyTagParser : TagParser
    {
        public override Language Lang { get { return Language.Ruby; } }

        string _args = @"--excmd=number --fields=aksST --sort=no --language-force=Ruby";
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
            string belongTo = _GetBelongTo(fields);
            AccessType accessType = _GetAccessType(fields);
            return new RubyTag(file, lineNo, tagName, tagType, accessType, belongTo);
        }

        protected override TagType StrToTagType(string sTagType)
        {
            switch (sTagType)
            {
                case "c":
                    return TagType.Ruby_Class;
                case "f":
                    return TagType.Ruby_Method;
                case "m":
                    return TagType.Ruby_Module;
                case "F":
                    return TagType.Ruby_SingletonMethod;
                default:
                    throw new Exception(string.Format("Unknown Ruby tag type '{0}'", sTagType));
            }
        }
    }
}