using System;
using System.Collections.Generic;
using System.Threading;

namespace NppPIALexer2.Tag
{
    class Pair<FirstClass, SecondClass>
    {
        FirstClass _first;
        SecondClass _second;

        public Pair(FirstClass first, SecondClass second)
        {
            _first = first;
            _second = second;
        }

        public FirstClass First { get { return _first; } }
        public SecondClass Second { get { return _second; } }
    }

    public class TagUpdater
    {
        static Dictionary<string, Pair<Language, int>> _ChangedFiles = new Dictionary<string, Pair<Language, int>>();
        static List<Pair<string[], int>> _DeletedFiles = new List<Pair<string[], int>>();

        public static void Work()
        {
            Thread t = new Thread(_Work);
            t.Start();
        }
        static void _Work() {

            foreach (Project project in ProjectManager.Projects) {
                var files = project.Root.SubFiles2;

             //??
            }

            while (true) {
                DateTime current = DateTime.Now;

                try {
                    Dictionary<string, Pair<Language, int>> _FilesToProcess = null;
                    List<Pair<string[], int>> _FilesToRemove = null;
                    /*TODO use lock object
                     * private readonly static Object s_lock = new Object();
                     * or
                     * private readonly Object _lock = new Object();
                       lock (_lock){  ...... }
                     * */
                    lock (typeof(TagUpdater)) {
                        if (_ChangedFiles.Count > 0) {
                            _FilesToProcess = _ChangedFiles;
                            _ChangedFiles = new Dictionary<string, Pair<Language, int>>();
                        }
                        if (_DeletedFiles.Count > 0) {
                            _FilesToRemove = _DeletedFiles;
                            _DeletedFiles = new List<Pair<string[], int>>();
                        }
                    }

                    if (_FilesToRemove != null) {
                        foreach (var obj in _FilesToRemove) {
                            string[] files = obj.First;
                            //if (files != null)
                               // TagCache.Remove(obj.Second, files);
                        }
                    }

                    if (_FilesToProcess != null) {
                        foreach (string file in _FilesToProcess.Keys) {
              //?? Todo incemental update              _Parser.AnalyseFile(file);
                        }
                    }
                } catch (Exception ex) {
                    Utility.Debug("TagUpdater Work Thread Error: {0}\n{1}", ex.Message, ex.StackTrace);
                }

                var d = DateTime.Now - current;
                if (d.TotalSeconds < 1)
                    Thread.Sleep((int)(1 - d.TotalSeconds) * 1000);
            }
        }

        static void _Work_old()
        {
            // 项目中所有的文件
            int i = 0;
            foreach (Project project in ProjectManager.Projects)
            {
                var files = project.Root.SubFiles2;

                var tags = TagParser.Parse(files.ToArray());

                //var tags = new List<ITag>();
                //foreach (string file in files)
                //{
                //    var t = TagParser.Parse(new string[] { file, });
                //    if (t != null && t.Count > 0)
                //        tags.AddRange(t);
                //    Thread.Sleep(5);
                //}

                if (tags != null)
                    TagCache.Add(i, tags.ToArray());
                ++i;
            }

            while (true)
            {
                DateTime current = DateTime.Now;

                try
                {
                    Dictionary<string,Pair<Language,int>> t = null;
                    List<Pair<string[], int>> f = null;
                    lock (typeof(TagUpdater))
                    {
                        if (_ChangedFiles.Count > 0)
                        {
                            t = _ChangedFiles;
                            _ChangedFiles = new Dictionary<string,Pair<Language,int>>();
                        }
                        if (_DeletedFiles.Count > 0)
                        {
                            f = _DeletedFiles;
                            _DeletedFiles = new List<Pair<string[],int>>();
                        }
                    }

                    if (f != null)
                    {
                        foreach (var obj in f)
                        {
                            string[] files = obj.First;
                            if (files != null)
                                TagCache.Remove(obj.Second, files);
                        }
                    }

                    if (t != null)
                    {
                        foreach (string file in t.Keys)
                        {
                            List<ITag> lst;
                            if (t[file].First == Language.Other)
                                lst = TagParser.Parse(new string[] { file });
                            else
                                lst = TagParser.Parse(t[file].First, file);
                            TagCache.Update(t[file].Second, file, lst);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utility.Debug("TagUpdater Work Thread Error: {0}\n{1}", ex.Message, ex.StackTrace);
                }

                var d = DateTime.Now - current;
                if (d.TotalSeconds < 1)
                    Thread.Sleep((int)(1 - d.TotalSeconds) * 1000);
            }
        }

        /// <summary>
        /// When the file is saved, it needs to update TagCache
        /// </summary>
        /// <param name="file"></param>
        /// <param name="lang"></param>
        public static void Update(int projIndex, string file, Language lang)
        {
            lock (typeof(TagUpdater))
            {
                _ChangedFiles[file] = new Pair<Language,int>(lang, projIndex);
            }
        }

        /// <summary>
        /// Update / add a label for a file
        /// </summary>
        /// <param name="file"></param>
        public static void Update(int projIndex, string file)
        {
            Update(projIndex, file, Language.Other);
        }

        public static void Remove(int projIndex, params string[] files)
        {
            lock (typeof(TagUpdater))
            {
                _DeletedFiles.Add(new Pair<string[],int>(files, projIndex));
            }
        }
    }
}