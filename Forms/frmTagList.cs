using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using NppPIALexer2.Tag;
using NppPIALexer2;


namespace NppPIALexer2.Forms
{
    public partial class frmTagList : Form
    {
        /// <summary>
        /// 为ClassView中所有结点创建索引
        /// </summary>
        class ClassViewIndex
        {
            public ClassViewIndex(Dictionary<string, TreeNode> tagFullName, Dictionary<string, List<TreeNode>> fileName)
            {
                TagFullName2TreeNode_Index = tagFullName;
                FileName2TreeNode_Index = fileName;
            }

            // key: tag.FullName, value: node
            public Dictionary<string, TreeNode> TagFullName2TreeNode_Index
            {
                get;
                set;
            }

            // key: filename, value: treenode list
            public Dictionary<string, List<TreeNode>> FileName2TreeNode_Index
            {
                get;
                set;
            }

            TreeNode _pyImportNode = null;
            TreeNode _PyVarNode = null;
            public TreeNode PyImportNode
            {
                get { return _pyImportNode; }
                set { _pyImportNode = value; }
            }

            public TreeNode PyVarNode
            {
                get { return _PyVarNode; }
                set { _PyVarNode = value; }
            }

            TreeNode _cppMacroNode = null;
            public TreeNode CppMacroNode
            {
                get { return _cppMacroNode; }
                set { _cppMacroNode = value; }
            }

            TreeNode _cppGlobalFuncNode = null;
            public TreeNode CppGloalFuncNode
            {
                get { return _cppGlobalFuncNode; }
                set { _cppGlobalFuncNode = value; }
            }

            TreeNode _cppTypedefNode = null;
            public TreeNode CppTypedefNode
            {
                get { return _cppTypedefNode; }
                set { _cppTypedefNode = value; }
            }

            TreeNode _cppGlobalVar = null;
            public TreeNode CppGlobalVar
            {
                get { return _cppGlobalVar; }
                set { _cppGlobalVar = value; }
            }

            //TreeNode _cMacroNode = null;
            //public TreeNode CMacroNode
            //{
            //    get { return _cMacroNode; }
            //    set { _cMacroNode = value; }
            //}

            //TreeNode _cGlobalVarNode = null;
            //public TreeNode CGlobalVarNode
            //{
            //    get { return _cGlobalVarNode; }
            //    set { _cGlobalVarNode = value; }
            //}
            
            //TreeNode _cTypedefNode = null;
            //public TreeNode CTypedefNode
            //{
            //    get { return _cTypedefNode; }
            //    set { _cTypedefNode = value; }
            //}

            TreeNode _js = null;
            public TreeNode JS
            {
                get { return _js; }
                set { _js = value; }
            }

        }

        class SearchResultItem
        {
            public SearchResultItem(int rootIndex, string fullName)
            {
                RootIndex = rootIndex;
                TagFullName = fullName;
            }
            public int RootIndex { get; set; }
            public string TagFullName { get; set; }
        }

        delegate void UpdateClassViewDelegate(CacheUpdatedArgs e);
        UpdateClassViewDelegate _updateClassView;
        delegate void UpdateViewDelegate(String File);
        UpdateViewDelegate _DGupdateView;
        //called by Event
        void OnLogChanged(String File) {
            while(!this.IsHandleCreated)
                Thread.Sleep(1);
            this.Invoke(_DGupdateView, new Object[] { File });
        }

        private String m_CurrFile = "";
        void _UpdateView(String File) {
            m_CurrFile = File;
            TreeNode root = tvClassView.Nodes[0]; //TODO
            _InsertTags(root, TagCache.GetTags(File));
        }

        TreeNode m_RootNode;
        public frmTagList()
        {
            InitializeComponent();
            
            tvClassView.ImageList = Resource.ClassViewImgList;
            _updateClassView = new UpdateClassViewDelegate(_TagCache_CacheUpdated);
            TagCache.CacheUpdated += new CacheUpdated(TagCache_CacheUpdated);
            _DGupdateView = new UpdateViewDelegate(_UpdateView);
            Main.getInstance().EvtFileBufferChanged += new Main.FileBufferChanged(OnLogChanged);
            // When the window is initialized, all tags are loaded from the content.
            //Changes to the label after the event callback to modify
            _LoadClassView();
        }

        void TagCache_CacheUpdated(CacheUpdatedArgs e)
        {
            while (!this.IsHandleCreated)
                Thread.Sleep(1);

            this.Invoke(_updateClassView, new object[] { e });
        }

        /// <summary>
        ///  Update interface when label changes
        /// </summary>
        /// <param name="file"></param>
        /// <param name="op"></param>
        void _TagCache_CacheUpdated(CacheUpdatedArgs e)
        {
            // 删除结点
            TreeNode root = tvClassView.Nodes[e.ProjectIndex];
            /*
            ClassViewIndex index = root.Tag as ClassViewIndex;
            if (index.FileName2TreeNode_Index.ContainsKey(e.File))
            {
                List<TreeNode> nodeList = index.FileName2TreeNode_Index[e.File];
                nodeList.Reverse();
                foreach (TreeNode node in nodeList)
                {
                    ITag tag = node.Tag as ITag;
                    index.TagFullName2TreeNode_Index.Remove(tag.FullName);
                }
                index.FileName2TreeNode_Index.Remove(e.File);
                foreach (TreeNode node in nodeList)
                {
                    if (node.Nodes.Count == 0)  // 没有子结点的情况下才删除。像命名空间往往有很多子结点
                        node.Remove();  // throw new exception?
                }
            }*/
            // 如果是Update操作，添加结点
            if (e.Operator == Operator.Update)
            {
                _InsertTags(root, TagCache.GetTags(e.File));
            }
        }
        /// <summary>
        /// Update tree for current active document
        /// </summary>
        /// <param name="root"></param>
        /// <param name="tags"></param>
        void _InsertTags(TreeNode root, List<ITag> tags) {
            string _Scope = NPP.GetCurrentFile();
            Project proj=ProjectManager.GetProjectByItsFile(_Scope);
            if (proj == null)
                return;
            ModelDocument _Model = proj.Model;
            _Scope = _Model.GetRelativePath(_Scope);
            List<Obj>.Enumerator _Objs =_Model.GetObjects(_Scope).GetEnumerator();
            TreeNode parent = new TreeNode(_Scope);
            parent.BeginEdit();
            while (_Objs.MoveNext()) {
                TreeNode node = new TreeNode();
                node.Text = _Objs.Current.ClassID()+" "+_Objs.Current.Name();
                node.Tag = _Objs.Current;
                node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Cpp_Variable;
                node.ToolTipText = _Objs.Current.Description();
                parent.Nodes.Add(node);
            }
            List<ObjDecl>.Enumerator _ObjsDecl = _Model.GetFunctions(_Scope).GetEnumerator();
            while(_ObjsDecl.MoveNext()) {
                TreeNode node = new TreeNode();
                node.Text = _ObjsDecl.Current.Function();
                node.Tag = _ObjsDecl.Current;
                node.ImageIndex = node.SelectedImageIndex = Resource.ClassViewIcon_Cpp_Function;
                node.ToolTipText = _ObjsDecl.Current.Description();
                parent.Nodes.Add(node);
            }
            parent.EndEdit(false);
            tvClassView.Nodes.Clear();
            tvClassView.Nodes.Add(parent);
            tvClassView.ExpandAll();
        }
        /// <summary>
        /// 将标签添加到浏览树
        /// </summary>
        /// <param name="root"></param>
        /// <param name="tags"></param>
        void _InsertTags2(TreeNode root, List<ITag> tags)
        {
            if (tags.Count == 0)
                return;
            //tags.Sort(new Comparison<ITag>(_CompareTag));

            ClassViewIndex index = root.Tag as ClassViewIndex;
            foreach (ITag tag in tags)
            {
                // 同名结点的情况, 视图中将不显示
                if (index.TagFullName2TreeNode_Index.ContainsKey(tag.FullName))
                    continue;

                TreeNode n = new TreeNode();
                if (!tag.BindToTreeNode(n))
                    continue;
                n.Tag = tag;

                index.TagFullName2TreeNode_Index[tag.FullName] = n;
                if (!index.FileName2TreeNode_Index.ContainsKey(tag.SourceFile))
                    index.FileName2TreeNode_Index[tag.SourceFile] = new List<TreeNode>();
                index.FileName2TreeNode_Index[tag.SourceFile].Add(n);

                // For the Python language, all import is placed under the Import folder and all variable are placed under the variable folder
                //For C + +, all macros are placed under the Macro folder, TypeDef, function, global var in a separate folder
                //For C, macro, TypeDef, global var into a separate folder
                TreeNode parent = null;
                //if (tag.FullName.IndexOf('.') == -1 || tag.TagType == TagType.CSharp_Namespace)
                if(tag.FullName == tag.TagName)    // Top-level objects
                {

                    if (tag.Lang == Language.Cpp && tag.TagType == TagType.Cpp_Macro || tag.Lang == Language.C && tag.TagType == TagType.C_Macro)
                    {
                        if (index.CppMacroNode == null)
                        {
                            index.CppMacroNode = new TreeNode("Macro", Resource.ClassViewIcon_Cpp_Macro, Resource.ClassViewIcon_Cpp_Macro);
                            index.CppMacroNode.Tag = "CppMacroNode";
                            root.Nodes.Insert(0, index.CppMacroNode);
                        }
                        parent = index.CppMacroNode;
                    }
                    else if ((tag.Lang == Language.Cpp && tag.TagType == TagType.Cpp_Function || tag.Lang == Language.C && tag.TagType == TagType.C_Function) && string.IsNullOrEmpty(tag.BelongTo))
                    {
                        if (index.CppGloalFuncNode == null)
                        {
                            index.CppGloalFuncNode = new TreeNode("Global Function", Resource.ClassViewIcon_Cpp_Function, Resource.ClassViewIcon_Cpp_Function);
                            index.CppGloalFuncNode.Tag = "CppGlobalFuncNode";
                            root.Nodes.Insert(0, index.CppGloalFuncNode);
                        }
                        parent = index.CppGloalFuncNode;
                    }
                    else if ((tag.Lang == Language.Cpp && tag.TagType == TagType.Cpp_Typedef || tag.Lang == Language.C && tag.TagType == TagType.C_Typedef) && string.IsNullOrEmpty(tag.BelongTo))
                    {
                        if (index.CppTypedefNode == null)
                        {
                            index.CppTypedefNode = new TreeNode("Global Typedef", Resource.ClassViewIcon_Cpp_Typedef, Resource.ClassViewIcon_Cpp_Typedef);
                            index.CppTypedefNode.Tag = "CppTypedefNode";
                            root.Nodes.Insert(0, index.CppTypedefNode);
                        }
                        parent = index.CppTypedefNode;
                    }
                    else if ((tag.Lang == Language.Cpp && tag.TagType == TagType.Cpp_Variable || tag.Lang == Language.C && tag.TagType == TagType.C_Variable) && string.IsNullOrEmpty(tag.BelongTo))
                    {
                        if (index.CppGlobalVar == null)
                        {
                            index.CppGlobalVar = new TreeNode("Global Variable", Resource.ClassViewIcon_Cpp_Variable, Resource.ClassViewIcon_Cpp_Variable);
                            index.CppGlobalVar.Tag = "CppGlobalVarNode";
                            root.Nodes.Insert(0, index.CppGlobalVar);
                        }
                        parent = index.CppGlobalVar;
                    }
                    else
                        parent = root;
                }
                else
                {
                    Utility.Assert(tag.FullName.EndsWith(tag.TagName));
                    string key = tag.FullName.Substring(0, tag.FullName.Length - tag.TagName.Length);
                    while (key.Length > 0 && key.EndsWith(".") || key.EndsWith(":"))  // Remove the last member accessor, generally ".", C + + is "::"
                        key = key.Substring(0, key.Length - 1);
                    if (string.IsNullOrEmpty(key))
                        continue;
                    if (!index.TagFullName2TreeNode_Index.ContainsKey(key))
                    {
                        continue;
                    }
                    parent = index.TagFullName2TreeNode_Index[key];
                }
                _InsertNode(parent, n);
            }
        }

        /// <summary>
        /// 加载标签视图
        /// </summary>
        void _LoadClassView()
        {
            foreach (Project project in ProjectManager.Projects)
            {
                List<ITag> tags = TagCache.GetTags(project.Root.SubFiles2.ToArray());
                TreeNode node = new TreeNode(project.Root.Name, Resource.ClassViewIcon_Project, Resource.ClassViewIcon_Project);
                tvClassView.Nodes.Add(node);
                _Bind(node, tags);
            }
        }

        /// <summary>
        /// 比较标签
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        int _CompareTag(ITag a, ITag b)
        {
            if (a.Lang != b.Lang)
                return (int)a.Lang - (int)b.Lang;
            if (a.TagType != b.TagType)
                return (int)a.TagType - (int)b.TagType;

            string[] arrA = a.FullName.Split('.');
            string[] arrB = b.FullName.Split('.');
            if (arrA.Length != arrB.Length)
                return arrA.Length - arrB.Length;
            for (int i = 0; i < arrA.Length; ++i)
            {
                int c = string.Compare(arrA[i], arrB[i]);
                if (c != 0)
                    return c;
            }
            return 0;
        }

        /// <summary>
        /// 添加子结点，子结点按顺序排序
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        void _InsertNode(TreeNode parent, TreeNode child)
        {
            if (parent.Nodes.Count == 0)
                parent.Nodes.Add(child);
            else
            {
                var nodeList = parent.Nodes;

                int beg = 0; // 部分节点（如 宏 结点，是一个包含结点）
                while (beg < nodeList.Count && nodeList[beg].Tag as ITag == null)
                    ++beg;
                if (beg >= nodeList.Count)
                {
                    nodeList.Add(child);
                    return;
                }
                int index = _GetInsertIndex(nodeList, beg, nodeList.Count - 1, child.Tag as ITag);
                if (index < nodeList.Count)
                    nodeList.Insert(index, child);
                else
                    parent.Nodes.Add(child);
            }
        }

        int _GetInsertIndex(TreeNodeCollection list, int beg, int end, ITag tag)
        {
            while (beg <= end)
            {
                int mid = (beg + end) / 2;
                var midTag = list[mid].Tag as ITag;
                int cmp = _CompareTag(tag, midTag);
                if (cmp < 0)
                    end = mid - 1;
                else if (cmp > 0)
                    beg = mid + 1;
                else
                    return mid;
            }
            return beg;
        }


        /// <summary>
        /// 将标签绑定到TreeView
        /// </summary>
        /// <param name="node">根结点</param>
        /// <param name="tags">标签列表</param>
        void _Bind(TreeNode root, List<ITag> tags)
        {
            // todo: 考虑到同一项目中有不同语言的情况
            // Dictionary<Language, Dictionary<string, List<ITag>>> t
            _ResetSearch();
            //tags.Sort(new Comparison<ITag>(_CompareTag));
          
            Dictionary<string, TreeNode> t = new Dictionary<string, TreeNode>();
            Dictionary<string, List<TreeNode>> f = new Dictionary<string, List<TreeNode>>();
            root.Tag = new ClassViewIndex(t, f);

            _InsertTags(root, tags);
        }

        /// <summary>
        /// 进入指定函数
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvClassView_DoubleClick(object sender, EventArgs e)
        {
            if (tvClassView.SelectedNode == null)
                return;
            ObjDecl tag = tvClassView.SelectedNode.Tag as ObjDecl;
            if (tag == null)
                return;
            String _path = Path.Combine(ProjectManager.GetProjectByItsFile(m_CurrFile).BaseDir, tag.ClassID());
            if (File.Exists(_path)) {
                Jump.Add(tag.Function(), _path, tag.StartPos());
                Jump.Cursor.Go();
                //NPP.GoToDefinition(tag.SourceFile, tag.LineNo - 1, tag.TagName);
            }
            /*if (File.Exists(tag.SourceFile))
            {
                Jump.Add(tag.TagName, tag.SourceFile, tag.LineNo - 1);
                Jump.Cursor.Go();
                //NPP.GoToDefinition(tag.SourceFile, tag.LineNo - 1, tag.TagName);
            }*/
        }

        void _ResetSearch()
        {
            if (_searchResult.Count > 0)
            {
                foreach (SearchResultItem item in _searchResult)
                {
                    var index = tvClassView.Nodes[item.RootIndex].Tag as ClassViewIndex;
                    index.TagFullName2TreeNode_Index[item.TagFullName].BackColor = Color.White;
                }
                _searchResult.Clear();
            }
            _current = -1;
            tvClassView.SelectedNode = null;
        }

        List<SearchResultItem> _searchResult = new List<SearchResultItem>();
        int _current = -1;
        private void tbtnSearch_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(ttxtSearchText.Text))
            {
                string kw = ttxtSearchText.Text.Trim();
                //int find = -1;
                //// tcboxSearchText的下拉列表中保存有最近的搜索关键字
                //for (int i = 0; i < tcboxSearchText.Items.Count; ++i)
                //{
                //    if ((string)tcboxSearchText.Items[i] == kw)
                //    {
                //        find = i;
                //        break;
                //    }
                //}
                //if (find != -1)
                //    tcboxSearchText.Items.RemoveAt(find);
                //tcboxSearchText.Items.Insert(0, kw);

                _ResetSearch();
                foreach (TreeNode root in tvClassView.Nodes)
                {
                    var index = root.Tag as ClassViewIndex;
                    foreach (string key in index.TagFullName2TreeNode_Index.Keys)
                    {
                        TreeNode node = index.TagFullName2TreeNode_Index[key];
                        ITag tag = node.Tag as ITag;
                        if (tag == null)
                            continue;
                        if (tag.TagName.StartsWith(kw))
                        {
                            _searchResult.Add(new SearchResultItem(root.Index, tag.FullName));
                            node.BackColor = Color.LightSkyBlue;    // 匹配项高亮显示
                        }
                    }
                }
                _searchResult.Sort(new Comparison<SearchResultItem>(delegate(SearchResultItem a, SearchResultItem b)
                    {
                        int ret = a.RootIndex - b.RootIndex;
                        if (ret != 0)
                            return ret;
                        return string.Compare(a.TagFullName, b.TagFullName);
                            
                    }));
                _SelectSearchResult(0);
            }
        }

        /// <summary>
        /// 按回车自动搜索
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tcboxSearchText_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)13)
            {
                tbtnSearch_Click(tbtnSearch, EventArgs.Empty);
            }
        }

        /// <summary>
        /// 选中搜索结果
        /// </summary>
        /// <param name="index"></param>
        void _SelectSearchResult(int index)
        {
            if (_searchResult.Count == 0)
                return;
            if (index < 0)
                index = _searchResult.Count - 1;
            else
                index = index % _searchResult.Count;
            SearchResultItem item = _searchResult[index];
            var i = tvClassView.Nodes[item.RootIndex].Tag as ClassViewIndex;
            TreeNode node = i.TagFullName2TreeNode_Index[item.TagFullName];
            tvClassView.Select();
            tvClassView.SelectedNode = node;
            _current = index;
        }

        private void tbtnPrev_Click(object sender, EventArgs e)
        {
            _SelectSearchResult(--_current);
        }

        private void tbtnNext_Click(object sender, EventArgs e)
        {
            _SelectSearchResult(++_current);
        }

        /// <summary>
        /// 刷新
        /// </summary>
        public void RefreshClassView()
        {
            _ResetSearch();
            tvClassView.Nodes.Clear();
            _LoadClassView();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            RefreshClassView();
        }

        /// <summary>
        /// 回车，定位到选中的定义处
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tvClassView_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && tvClassView.SelectedNode != null)
            {
                ITag tag = tvClassView.SelectedNode.Tag as ITag;
                if (tag == null)
                    return;
                if (File.Exists(tag.SourceFile))
                {
                    Jump.Add(tag.TagName, tag.SourceFile, tag.LineNo - 1);
                    //NPP.GoToDefinition(tag.SourceFile, tag.LineNo - 1, tag.TagName);
                    Jump.Cursor.Go();
                }
            }
        }

        private void frmTagList_Load(object sender, EventArgs e) {

        }

    }
}
