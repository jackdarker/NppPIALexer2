using System;
using System.Collections.Generic;

namespace NppPIALexer2
{
    public class Jump
    {
        private Jump(string info, string file, int lineNo, int pos)
        {
            Info = info;
            File = file;
            LineNo = lineNo;
            Pos = pos;
        }

        private Jump(string info, string file, int lineNo)
            : this(info, file, lineNo, -1)
        { }

        public string Info { get; set; }
        public string File { get; set; }
        /// <summary>
        /// 行号，从0开始
        /// </summary>
        public int LineNo { get; set; }
        public int Pos { get; set; }

        public override string ToString()
        {
            return string.Format("{0}    [{1}]{2}", Info, LineNo + 1, File);
        }

        public void Go()
        {
            if (Pos != -1)
                NPP.GoToDefinition(File, Pos);
            else
                NPP.GoToDefinition(File, LineNo, Info);
        }

        static List<Jump> _JumpList = new List<Jump>();
        static int _Cursor = -1;

        public static void Add(string info, string file, int lineNo)
        {
            if (!System.IO.File.Exists(file))
                return;
            
            Jump oldPos = new Jump(NPP.GetCurrentWord2(), NPP.GetCurrentFile(), NPP.GetCurrentLine(), NPP.GetCurrentPosition());
            if (oldPos.Info == "")
                oldPos.Info = string.Format("line-{0}", oldPos.LineNo + 1);

            Jump newPos = new Jump(info, file, lineNo);
            while (_JumpList.Count > _Cursor + 1 ||
                    _JumpList.Count > 0 && _JumpList[_JumpList.Count - 1].File == file && _JumpList[_JumpList.Count - 1].LineNo == lineNo)
                _JumpList.RemoveAt(_JumpList.Count - 1);

            if (_JumpList.Count == 0 || _JumpList.Count > 0 && (_JumpList[_JumpList.Count - 1].LineNo != oldPos.LineNo || _JumpList[_JumpList.Count - 1].File != oldPos.File))
                _JumpList.Add(oldPos);
            _JumpList.Add(newPos);

            while (_JumpList.Count > 20)    // 最多保留20项
                _JumpList.RemoveAt(0);
            _Cursor = _JumpList.Count - 1;
        }

        public static Jump Cursor
        {
            get { return _Cursor == -1 ? null : _JumpList[_Cursor]; }
            set 
            {
                for (int i = 0; i < _JumpList.Count; ++i)
                    if (_JumpList[i] == value)
                    {
                        _Cursor = i;
                        break;
                    }
            }
        }

        public static Jump Back
        {
            get { return _Cursor <= 0 ? null : _JumpList[--_Cursor]; }
        }

        public static Jump Forward
        {
            get { return _Cursor >= _JumpList.Count - 1 ? null : _JumpList[++_Cursor]; }
        }

        public static Jump[] JumpList
        {
            get { _JumpList.Reverse(); var ret = _JumpList.ToArray(); _JumpList.Reverse(); return ret; }
        }
        
        public static void ClearList()
        {
            _JumpList.Clear(); 
            _Cursor = -1;
        }

    }
}