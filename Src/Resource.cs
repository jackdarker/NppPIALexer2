using System.Windows.Forms;
using System.Drawing;
namespace NppPIALexer2
{
    public class Resource
    {
        static ImageList _ImgList = null;
        static ImageList _ClassViewImgList = null;

        static Resource()
        {
            _ImgList = new ImageList();
            _ImgList.ColorDepth = ColorDepth.Depth32Bit;
            _ClassViewImgList = new ImageList();
            _ClassViewImgList.ColorDepth = ColorDepth.Depth32Bit;

            #region Projecttreeicon
            _ImgList.Images.Add(Properties.Resources.AddProj);  // 0
            _ImgList.Images.Add(Properties.Resources.OpenProj); // 1
            _ImgList.Images.Add(Properties.Resources.Proj);     // 2
            _ImgList.Images.Add(Properties.Resources.FloderOpen);   // 3
            _ImgList.Images.Add(Properties.Resources.FloderClose);  // 4
            _ImgList.Images.Add(Properties.Resources.File); // 5
            #endregion

            #region Classbrowsetreeicon

            _2(Properties.Resources.Proj);
            _2(Properties.Resources.asm_macro);     // 1
            _2(Properties.Resources.asm_proc);      // 2
            _2(Properties.Resources.asm_section);   // 3
            _2(Properties.Resources.asm_struct);    // 4

            _2(Properties.Resources.c_class);       // 5
            _2(Properties.Resources.c_enum);        // 6
            _2(Properties.Resources.c_enumitem);    // 7
            _2(Properties.Resources.c_event);       // 8
            _2(Properties.Resources.c_field);       // 9
            _2(Properties.Resources.c_interface);   // 10
            _2(Properties.Resources.c_macro);       // 11
            _2(Properties.Resources.c_method);      // 12
            _2(Properties.Resources.c_namespace);   // 13
            _2(Properties.Resources.c_property);    // 14
            _2(Properties.Resources.c_struct);      // 15
            _2(Properties.Resources.c_typedef);     // 16
            _2(Properties.Resources.c_union);       // 17

            _2(Properties.Resources.python_class);  // 18
            _2(Properties.Resources.python_function);//19
            _2(Properties.Resources.python_method); // 20
            _2(Properties.Resources.python_variable);//21
            _2(Properties.Resources.python_import); // 22

            _2(Properties.Resources.xml_tag);        // 23

            _2(Properties.Resources.pascal_function);   // 24
            _2(Properties.Resources.pascal_procedure);   // 25

            _2(Properties.Resources.JS);            // 26
            
            #endregion
        }

        static void _2(Bitmap bmp) { _ClassViewImgList.Images.Add(bmp); }

        #region The project tree icon corresponds to the index in the ImageList
        public const int Icon_AddProj = 0;
        public const int Icon_OpenProj = 1;
        public const int Icon_Project = 2;
        public const int Icon_FolderOpen = 3;
        public const int Icon_FolderClose = 4;
        public const int Icon_File = 5;
        #endregion

        public const int ClassViewIcon_Project = 0;

        public const int ClassViewIcon_C_Class = 5;
        public const int ClassViewIcon_C_Macro = 11;
        public const int ClassViewIcon_C_Enumerator = 7;
        public const int ClassViewIcon_C_Enumeration = 6;
        public const int ClassViewIcon_C_Function = 12;
        public const int ClassViewIcon_C_LocalVar = 9;
        public const int ClassViewIcon_C_ClassMember = 12;
        public const int ClassViewIcon_C_Namespace = 13;
        public const int ClassViewIcon_C_FunctionPrototype = 14;
        public const int ClassViewIcon_C_Struct = 15;
        public const int ClassViewIcon_C_Typedef = 16;
        public const int ClassViewIcon_C_Union = 17;
        public const int ClassViewIcon_C_Variable = 9;
        public const int ClassViewIcon_C_Declaration = 11;

        public const int ClassViewIcon_Cpp_Class = 5;
        public const int ClassViewIcon_Cpp_Macro = 11;
        public const int ClassViewIcon_Cpp_EnumerationName = 7; // 
        public const int ClassViewIcon_Cpp_Function = 12;
        public const int ClassViewIcon_Cpp_Enum = 6;            // 
        public const int ClassViewIcon_Cpp_LocalVar = 9;        // 
        public const int ClassViewIcon_Cpp_ClassMember = 9;
        public const int ClassViewIcon_Cpp_Namespace = 13;
        public const int ClassViewIcon_Cpp_FunctionPrototype = 14;  // 
        public const int ClassViewIcon_Cpp_Struct = 15;
        public const int ClassViewIcon_Cpp_Typedef = 16;
        public const int ClassViewIcon_Cpp_Union = 17;
        public const int ClassViewIcon_Cpp_Variable = 9;
        public const int ClassViewIcon_Cpp_Declaration = 11;    // 

        public const int ClassViewIcon_CSharp_Class = 5;
        public const int ClassViewIcon_CSharp_Macro = 11;
        public const int ClassViewIcon_CSharp_Enum = 6;
        public const int ClassViewIcon_CSharp_Event = 8;
        public const int ClassViewIcon_CSharp_Field = 9;
        public const int ClassViewIcon_CSharp_EnumerationName = 7;
        public const int ClassViewIcon_CSharp_Interface = 10;
        public const int ClassViewIcon_CSharp_LocalVar = 9;
        public const int ClassViewIcon_CSharp_Method = 12;
        public const int ClassViewIcon_CSharp_Namespace = 13;
        public const int ClassViewIcon_CSharp_Property = 14;
        public const int ClassViewIcon_CSharp_Struct = 15;
        public const int ClassViewIcon_CSharp_Typedef = 16;

        public const int ClassViewIcon_Java_Class = 5;
        public const int ClassViewIcon_Java_EnumConstant = 7;
        public const int ClassViewIcon_Java_Field = 9;
        public const int ClassViewIcon_Java_Enum = 6;
        public const int ClassViewIcon_Java_Interface = 10;
        public const int ClassViewIcon_Java_LocalVar = 9;   // 
        public const int ClassViewIcon_Java_Method = 12;
        public const int ClassViewIcon_Java_Package = 13;

        public const int ClassViewIcon_Python_Class = 18;
        public const int ClassViewIcon_Python_Function = 19;
        public const int ClassViewIcon_Python_Method = 20;
        public const int ClassViewIcon_Python_Variable = 21;
        public const int ClassViewIcon_Python_Import = 22;
        public const int ClassViewIcon_Python_Field = 9;    // 

        public const int ClassViewIcon_JavaScript_Function = 12;
        public const int ClassViewIcon_JavaScript_Class = 5;
        public const int ClassViewIcon_JavaScript_Method = 12;
        public const int ClassViewIcon_JavaScript_Property = 14;
        public const int ClassViewIcon_JavaScript_GlobalVar = 9;

        public const int ClassViewIcon_Flex_Function = 12;
        public const int ClassViewIcon_Flex_Class = 5;
        public const int ClassViewIcon_Flex_Method = 12;
        public const int ClassViewIcon_Flex_Property = 14;
        public const int ClassViewIcon_Flex_GlobalVar = 9;
        public const int ClassViewIcon_Flex_Mxtag = 23;

        public const int ClassViewIcon_PHP_Class = 5;
        public const int ClassViewIcon_PHP_Interface = 10;
        public const int ClassViewIcon_PHP_Constant = 11;
        public const int ClassViewIcon_PHP_Function = 12;
        public const int ClassViewIcon_PHP_Variable = 9;
        public const int ClassViewIcon_PHP_JsFunction = 12;

        public const int ClassViewIcon_ASM_Define = 2;
        public const int ClassViewIcon_ASM_Label = 3;
        public const int ClassViewIcon_ASM_Macro = 1;
        public const int ClassViewIcon_ASM_Type = 4;

        public const int ClassViewIcon_Ruby_Module = 3;
        public const int ClassViewIcon_Ruby_Method = 12;
        public const int ClassViewIcon_Ruby_Class = 5;
        public const int ClassViewIcon_Ruby_SingletonMethod = 12;

        public const int ClassViewIcon_Pascal_Function = 24;
        public const int ClassViewIcon_Pascal_Procedure = 25;

        public const int ClassViewIcon_JavaScript = 26;


        public static ImageList ImageList
        {
            get { return _ImgList; }
        }

        public static ImageList ClassViewImgList
        {
            get { return _ClassViewImgList; }
        }


        /// <summary>
        /// A reference to a tree control in the interface (initialized in frmMain's constructor)
        /// </summary>
        public static TreeView ProjectTreeView;  //todo oh no please

        public static readonly string[] LangTypes = new string[] {
            "TEXT", "PHP ", "C", "CPP", "CS", "OBJC", "JAVA", "RC",
        "HTML", "XML", "MAKEFILE", "PASCAL", "BATCH", "INI", "ASCII", "USER",
        "ASP", "SQL", "VB", "JS", "CSS", "PERL", "PYTHON", "LUA",
        "TEX", "FORTRAN", "BASH", "FLASH", "NSIS", "TCL", "LISP", "SCHEME",
        "ASM", "DIFF", "PROPS", "PS", "RUBY", "SMALLTALK", "VHDL", "KIX", "AU3",
        "CAML", "ADA", "VERILOG", "MATLAB", "HASKELL", "INNO", "SEARCHRESULT",
        "CMAKE", "YAML", "COBOL", "GUI4CLI", "D", "POWERSHELL", "R", "JSP",
        // The end of enumated language type", so it should be always at the end
        "EXTERNAL",
        };
    }
}