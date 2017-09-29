using System.Collections.Generic;
using System;
using System.Windows.Forms;

namespace NppPIALexer2.Tag
{
    public class SmartLVTag : ITag
    {
        public SmartLVTag(string srcFile, int lineNo, string tagName, TagType tagType, AccessType accessType, string belongTo)
            : base(srcFile, lineNo, tagName, tagType, accessType, belongTo, "")
        { }

        public override Language Lang { get { return Language.SmartLV; } }

        public override bool BindToTreeNode(TreeNode node)
        {
            node.Text = TagName;
            node.ToolTipText = SourceFile;
            node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Python_Class;
            /*switch (TagType)
            {
                case TagType.SmartLVTag_Class:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Python_Class;
                    break;
                case TagType.SmartLVTag_Function:
                    node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Python_Function;
                    break;
                case TagType.SmartLVTag_Import:
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
            }*/
            return true;
        }
    }

}