using System;
using System.Collections.Generic;
using System.IO;

namespace NppPIALexer2.Tag
{
    public enum Operator
    {
        //Insert,
        Update,
        Delete,
    }

    public class CacheUpdatedArgs
    {
        int _projIndex;
        string _file;
        Operator _op;

        public int ProjectIndex { get { return _projIndex; } }
        public string File { get { return _file; } }
        public Operator Operator { get { return _op; } }

        public CacheUpdatedArgs(int projIndex, string file, Operator op)
        {
            _projIndex = projIndex;
            _file = file;
            _op = op;
        }
    }

    public delegate void CacheUpdated(CacheUpdatedArgs e);

    /// <summary>
    /// Tag the tag in file units
    /// Also create an index for all tags based on TagName
    /// </summary>
    public class TagCache
    {
        public static event CacheUpdated CacheUpdated;

        // key: file, value: tags list
        static Dictionary<string, List<ITag>> _Cache = new Dictionary<string, List<ITag>>();

        // The first two letters of TagName are the keys
        static Dictionary<string, List<ITag>> _CacheIndex = new Dictionary<string, List<ITag>>();

        /// <summary>
        ///Gets the list of tags within the file
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public static List<ITag> GetTags(params string[] files)
        {
            List<ITag> lst = new List<ITag>();
            if (files.Length == 0)
                return lst;

            lock (typeof(TagCache))
            {
                foreach (string file in files)
                    if (_Cache.ContainsKey(file))
                        lst.AddRange(_Cache[file]);
            }
            return lst;
        }


        /// <summary>
        /// TagName前两个字母为key
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        static string _GetKey(ITag tag) { return tag.TagName.Length >= 2 ? tag.TagName.Substring(0, 2) : tag.TagName; }

        static void _AddToIndex(ITag tag)
        {
            string key = _GetKey(tag);
            if (!_CacheIndex.ContainsKey(key))
                _CacheIndex[key] = new List<ITag>();
            _CacheIndex[key].Add(tag);
        }

        static void _AddToIndex(List<ITag> tags)
        {
            foreach (ITag tag in tags)
                _AddToIndex(tag);
        }

        static void _RemoveFromIndex(ITag tag)
        {
            string key = _GetKey(tag);
            _CacheIndex[key].Remove(tag);
        }

        static void _RemoveFromIndex(List<ITag> tags)
        {
            foreach (ITag tag in tags)
                _RemoveFromIndex(tag);
        }

        public static void Add(int projIndex, params ITag[] tags)
        {
            if (tags == null)
                return;

            List<string> addedFiles = new List<string>();   // 记录添加的文件，用于触发事件
            lock (typeof(TagCache))
            {
                foreach (ITag tag in tags)
                {
                    if (!_Cache.ContainsKey(tag.SourceFile))
                    {
                        _Cache[tag.SourceFile] = new List<ITag>();
                        addedFiles.Add(tag.SourceFile);
                    }
                    _Cache[tag.SourceFile].Add(tag);
                    _AddToIndex(tag);
                }
            }

            if (CacheUpdated != null && addedFiles.Count > 0)
                foreach (string file in addedFiles)
                    CacheUpdated(new CacheUpdatedArgs(projIndex, file, Operator.Update));
        }
        
        public static void Update(int projIndex, string file, List<ITag> fileTags)
        {
            lock (typeof(TagCache))
            {
                if (_Cache.ContainsKey(file))
                    _RemoveFromIndex(_Cache[file]);
                _Cache[file] = fileTags;
                _AddToIndex(fileTags);
            }
            if (CacheUpdated != null)
            {
                CacheUpdated(new CacheUpdatedArgs(projIndex, file, Operator.Update));
            }
        }

        /// <summary>
        /// 删除文件的标签。一般在卸载项目，或者删除/移除项目文件的时候调用
        /// </summary>
        /// <param name="files"></param>
        public static void Remove(int projIndex, params string[] files)
        {
            List<string> removedFile = new List<string>();
            lock (typeof(TagCache))
            {
                foreach (string file in files)
                    if (_Cache.ContainsKey(file))
                    {
                        _RemoveFromIndex(_Cache[file]);
                        _Cache.Remove(file);
                        removedFile.Add(file);
                    }
            }
            if (removedFile.Count > 0 && CacheUpdated != null)
                foreach (string file in removedFile)
                    CacheUpdated(new CacheUpdatedArgs(projIndex, file, Operator.Delete));
        }

        static Dictionary<Language, List<string>> _BaseLibraryCache = new Dictionary<Language, List<string>>();

        /// <summary>
        /// 过滤内置类库的标签
        /// </summary>
        /// <param name="lang"></param>
        /// <param name="tagNameHead"></param>
        /// <returns></returns>
        static List<string> _FilterLibraryTags(Language lang, string tagNameHead)
        {
            List<string> lst = new List<string>();
            int beg, end;
            if (_BaseLibraryCache.ContainsKey(lang) && _GetHeadAndEndIndex(_BaseLibraryCache[lang], tagNameHead, out beg, out end))
                lst.AddRange(_BaseLibraryCache[lang].GetRange(beg, end - beg));
            return lst;
        }

        static bool _GetHeadAndEndIndex(List<string> list, string tagNameHead, out int beg, out int end)
        {
            beg = 0;
            end = list.Count - 1;
            while (beg <= end)
            {
                int mid = (beg + end) / 2;
                var midTag = list[mid];
                midTag = midTag.Length > tagNameHead.Length ? midTag.Substring(0, tagNameHead.Length) : midTag;
                int cmp = _CompareString(tagNameHead, midTag);
                if (cmp < 0)
                    end = mid - 1;
                else if (cmp > 0)
                    beg = mid + 1;
                else
                {
                    beg = mid;
                    while (beg - 1 >= 0 && list[beg - 1].StartsWith(tagNameHead))
                        --beg;
                    end = mid;
                    while (end < list.Count && list[end].StartsWith(tagNameHead))
                        ++end;
                    return true;
                }
            }
            return false;
        }

        static int _CompareString(string a, string b)
        {
            int len = a.Length < b.Length ? a.Length : b.Length;
            int i = 0;
            while (i < len && a[i] - b[i] == 0)
                ++i;
            return i < len ? a[i] - b[i] : a.Length - b.Length;
        }

        /// <summary>
        /// 过滤标签(智能提示)
        /// </summary>
        /// <param name="tagNameHead"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        public static List<string> FilterTagName(string tagNameHead, Language lang)
        {
            if (tagNameHead.Length < 2)
                return new List<string>();
            Dictionary<string, int> ret = new Dictionary<string, int>();
            string key = tagNameHead.Substring(0, 2);
            lock (typeof(TagCache))
            {
                if (_CacheIndex.ContainsKey(key))
                {
                    foreach (ITag tag in _CacheIndex[key])
                        if (tag.TagName.StartsWith(tagNameHead))
                        {
                            if (lang == Language.Other)
                                ret[tag.TagName] = 1;
                            else if (tag.Lang == lang)  // 同语言
                                ret[tag.TagName] = 1;
                        }
                }
            }

            // 加载语言默认内置库
            if (lang != Language.Other && !_BaseLibraryCache.ContainsKey(lang))
            {
                _BaseLibraryCache[lang] = new List<string>();
                string tagFile = Path.Combine(Config.Instance.TagDir, lang.ToString());
                if (File.Exists(tagFile))
                {
                    _BaseLibraryCache[lang].AddRange(File.ReadAllLines(tagFile));
                    _BaseLibraryCache[lang].Sort(_CompareString);
                }
            }
            if (lang != Language.Other)
                foreach (string tag in _FilterLibraryTags(lang, tagNameHead))
                    ret[tag] = 1;

            var lst = new List<string>(ret.Keys);
            lst.Sort();
            return lst;
        }

        public static List<string> FilterTagName(string tagNameHead)
        {
            return FilterTagName(tagNameHead, Language.Other);  
        }

        public static List<ITag> SearchTag(string tagName, Language lang)
        {
            string key = tagName;
            if (key.Length > 2)
                key = key.Substring(0, 2);
            List<ITag> lst = new List<ITag>();
            lock (typeof(TagCache))
            {
                if (_CacheIndex.ContainsKey(key))
                {
                    foreach (ITag tag in _CacheIndex[key])
                    {
                        if (tag.TagName == tagName && (tag.Lang == lang || lang == Language.Other) )
                            lst.Add(tag);
                    }
                }
            }
            return lst;
        }

        public static List<ITag> SearchTag(string tagName)
        {
            return SearchTag(tagName, Language.Other);
        }

    }
}