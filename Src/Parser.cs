using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace NppPIALexer2 {

    public class Parser {
        private String m_Scope;
        private bool m_IsClassDef;
        private ModelDocument m_Model;
        private String m_Project;
        private String m_Comment;
        private bool m_SlashStarComment;

        public Parser(ModelDocument Model, String Project) {
            m_Project = Project;
            m_Model = Model;
        }
        //parse doxygen comment
        private String parseComment(String Line) {
            //we expect something like this before functions
            // /**
            //  * Checks if the file was changed
            //  * @param file
            //  * @return
            //  */
            // private boolean hasChanged(IFile file)

            //...or this before vars
            // //a variable that holds a double
            // double fTest
            bool isComment = Line.StartsWith("//");
            m_SlashStarComment = m_SlashStarComment || Line.StartsWith("/*");

            if (isComment) {
                m_Comment += Line + "\n";
                Line = "";
            } else if (m_SlashStarComment) {
                int _found = Line.IndexOf("*/");
                if (_found < 0) {
                    m_Comment = Line + "\n";
                    Line = "";
                } else {
                    m_SlashStarComment = false;
                    m_Comment = Line.Substring(0, _found);
                    Line = Line.Substring(_found + 2);
                }
            }

            return Line;
        }
    /* removes unnecessary whitespaces and tabs in this text line except in between ""
	*   function  foo ( bool X , "this that & his" ,  string _A ) -> double Y , string test ;
	* function foo(bool X,"this that & his",string _A)->double Y, string test;
	* 
	*/
	private static String s_SpecialChars= "()-><+\\/,;.!=?:*%&|";
	public static String NormalizeWhitespace(String text) {
		String _out ="";
		bool _text = false, _text2=false; //found starting/end of ""
		int _white = 0; // is/was whitespace
		int _special=0; // is/was special char
		bool _Start=false;
		String _Curr;
		for (int i= 0; i < text.Length; i++) {
			_Curr = text.Substring(i,1);
			if(_Curr=="\"") {
				if(_text) { 
					_text=false; _text2=true;
				} else {
					_text=true; _text2=false;
				}
			}
			if(_text || _text2) {  // in between "
				_Start=true;
				if ((_white & 0x2)>0) _out=_out+" "; //add space if there was at least one before
				_out=_out+_Curr;
			} else {
				if (((_Curr==" ") || (_Curr=="\t")) && (_special & 0x2)==0) { //set flag if there is space but no special before
					_white=_white+1;
				} else if (s_SpecialChars.IndexOf(_Curr)>=0) { //special char; scrap space before
					_special=_special +1;
					_white = 0;	
				} else if (((_Curr==" ") || (_Curr=="\t")) && (_special & 0x2)>0){ //scrap space after special
					_white = 2;	
				} else if ((_special & 0x2)>0){ //reset flag if normal char
					_special = 0;	
				}

				if ((_white & 0x2)>0 && (_white & 0x1 )==0 && 
					(_special & 0x2)==0 && (_special & 0x1 )==0) { //normal character, but spaces before; insert one
					_out=_out+" ";
					_out=_out+_Curr;
				} else if((_white & 0x2)==0 && (_white & 0x1)==0) { //normal character
					_Start=true;
					_out=_out+_Curr;
				}
			}
			if(_Start) {
				_white=(_white*2)& 0x3; 
			} else { 
				_white=0; // shift out the flag
			}
			_special=(_special*2 | (_special & 0x2))& 0x3; //keep the flag; required if multiple spaces after special flag
			_text2=false;
		}
		return _out;
	}

    private static Char[] s_SingleSeparators = {' ','(',')','-','>','<','+','\\','/',',',';','.','!','=','?',':','*','%','&','|'};
    private static Char[] s_DualSeparators = { '-', '>', '<', '/', '!', '=', '&', '|' };    //  -> // >= <= == != && ||
    private void parseLine(String Line, int LineNo, IParserNode Tree) {
        IParserNode LineNode = null;
        int offset=0;
        int previous=0;
        string found;
        int foundSingle=0;
        int foundDual=0;
        bool stop=false;
        bool next = false;
        List<String> Stack = new List<string>();
        Line = NormalizeWhitespace(Line);
        while (!stop) {
            next = false;
            foundDual = Line.IndexOfAny(s_DualSeparators, offset);
            foundSingle = Line.IndexOfAny(s_SingleSeparators, offset);

            if (foundDual >= 0 && foundDual <= foundSingle) {
                next = true;
                found = Line.Substring(foundDual, 2);
                if (found == "->") { // after function parameter list   
                } else if (found == "//") { // line comment  
                } else { // its a single
                    next = false;
                }
            }
            if (foundSingle >= 0 && !next) {
                next = true;
                found = Line.Substring(foundSingle, 1);
                if (found == " ") {

                } else { // its a single
                    next = false;
                }
            }
  
            if(!next) {
                stop = true;
            }
        }
    }
    private void parseStack(List<String> Stack, bool EOL) {
        int max = Stack.Count;
        String word0;
        if (max >= 1 && EOL) {
            word0 = Stack[0];
        }
    }
        /*assumption:
	 * - pro Zeile nur ein Kommando
	 * - Kommando nicht über mehrere Zeilen verteilt
	 * - Zeile wird mit \r,\n oder \r\n abgeschlossen
	 * - nach functionskopf oder Variablendeklaration kann Kommentar eingeschoben sein
	 */
        private void evaluate(String Line, int LineNo) {

            Obj _obj;
            ObjDecl _objDecl;
            int _offset = 0;
            int _found, _foundB, _foundC;
            String _A, _B, _C;

            Line = parseComment(Line);
            //whitespaces should be normalized and comments removed
            Line = NormalizeWhitespace(Line);

            // using "Objects\Calculator\Calculator_Commander.vi" as Calc
            _found = Line.IndexOf("using ", _offset); // das wäre sicherer mit #using
            if (_found != -1 && _found < 5) {
                _offset = _offset + _found + 7; // after using "
                _found = Line.IndexOf(" as ", _offset);
                _A = Line.Substring(_offset, _found -_offset- 1);  //strip of ""
                _A = _A.Replace(".vi", ".seq"); //classID should point to description file, not commander
                _B = Line.Substring(_found + 4);
                _obj = new Obj(m_Scope, _B,
                        m_Project + m_Model.getSourceDir() + "\\" + _A, m_Comment);
                m_Model.UpdateObjList(_obj);
                m_Comment = ""; m_SlashStarComment = false;
                //return;
            }
            // #include test1.seq
            _found = Line.IndexOf("#include", _offset);
            if (_found != -1) {
                _offset = _offset + _found + 9; // after #include 
                _A = Line.Substring(_offset);
                _A = _A.Substring(1, _A.Length-1-1);//strip off ""
                _obj = new Obj(m_Scope, _A,
                        m_Project + m_Model.getSeqDir() + "\\" + _A, m_Comment);	//Todo das ist falsch bei Subproject-Includes
                m_Model.UpdateObjList(_obj);
                m_Comment = ""; m_SlashStarComment = false;
                //return;
            }
            // function boolAnd (bool bA, bool bB) ->bool bReturn
            //or  function boolAnd (bool bA, bool bB)
            _found = Line.IndexOf("function", _offset);
            if (_found != -1) {
                _C = "";
                _offset = _offset + _found + 9; // after function
                _foundC = Line.IndexOf("->", _offset);
                if (_foundC != -1) {
                    _C = Line.Substring(_foundC + 2);	// after ->	
                } else {
                    _foundC = Line.Length;
                }
                _foundB = Line.IndexOf("(", _offset);
                if (_foundB != -1) {
                    _B = Line.Substring(_foundB + 1, _foundC-_foundB - 2); // in between ( )
                    _A = Line.Substring(_offset, _foundB-_offset);
                    _objDecl = new ObjDecl(m_Scope,
                        m_IsClassDef ? ObjDecl.TClassType.tCTFunc : ObjDecl.TClassType.tCTSeq,
                            _A, _B, _C, m_Comment);
                    m_Model.UpdateObjDecl(_objDecl);
                    m_Comment = ""; m_SlashStarComment = false;
                }
                //return;
            }
            //is it some variable initializer?
            for (int i = 0; i < ModelDocument.BASIC_TYPES.Count; i++) {
                _found = Line.IndexOf(ModelDocument.BASIC_TYPES[i] + " ", 0);
                if (_found == 0) { //datatype should be at start of line, not in between ()
                    _A = ModelDocument.BASIC_TYPES[i];
                    _foundB = _found + (_A.Length + 1);
                    _foundC = Line.IndexOf('=', _foundB);	// double fVolt=2.5
                    if (_foundC == -1) {
                        _foundC = Line.IndexOf(';', _foundB);	// int iX;		
                        if (_foundC == -1) {
                            _foundC = Line.Length; // int iX<lf>
                        }
                    }
                    _B = Line.Substring(_foundB, _foundC-_foundB);
                    _obj = new Obj(m_Scope, _B, _A, m_Comment);
                    m_Model.UpdateObjList(_obj);
                    m_Comment = ""; m_SlashStarComment = false;
                }
            }
            /*
            _found = Line.indexOf("{", _offset);
            if (_found != -1) {
                _offset = _offset + _found + 1;
                m_FoldMarkersDepth = m_FoldMarkersDepth + 1;
                if (m_FoldMarkersDepth == 1) 	//Todo we actually only create foldmarkers for functions; not for/While/if
                    m_FoldMarkers.add(new Position(TextOffset, 0));
            }
            _found = Line.indexOf("}", _offset);
            if (_found != -1) {
                _offset = _offset + _found + 1;
                m_FoldMarkersDepth = m_FoldMarkersDepth - 1;
                if (m_FoldMarkersDepth == 1) {
                    Position p = m_FoldMarkers.get(m_FoldMarkers.size() - 1);
                    p.setLength(TextOffset - p.getOffset());
                }
            }*/

        }

        public int AnalyseFile(String filePath) {
            m_Comment = "";
            m_SlashStarComment = false;
            int _Ret = 0;
            if (!File.Exists(filePath)) 
                return -1;

            string ext = Path.GetExtension(filePath).ToLower();
            if (string.IsNullOrEmpty(ext) || !ext.Equals(".seq"))    //only .seq files are parsed
                return -1;
            m_Scope = m_Model.GetRelativePath(filePath); // the sequence relative to project-dir
            m_IsClassDef = false;
            /* TODO
             * if (resource.getProjectRelativePath().segment(0).equalsIgnoreCase("SOURCE")) {
                m_IsClassDef = true;  //Version 1   its in //SOURCE//...
            }
            if (resource.getProjectRelativePath().segment(0).equalsIgnoreCase("APP")) {
                m_IsClassDef = true;   //Version 2    its in //APP//PLUGINS//...
            }*/
            if (!m_IsClassDef) {//each SEQ includes itself
                m_Model.UpdateObjList(new Obj(m_Scope, "", m_Scope, ""));
            }
            int levelCurrent = 0;
            int levelPrev = -1;
            int lineNo=0;
            foreach (string _line in File.ReadAllLines(filePath)) {
                evaluate(_line, lineNo);

                if (_line.Contains("{")) {
                    levelCurrent++;
                }
                if (_line.Contains("}")) {
                    levelCurrent--;
                }
                uint lev = (uint)levelPrev;
                NPP.SetFoldLevel(lineNo, lev,levelCurrent > levelPrev);  //Todo !!
                levelPrev = levelCurrent;
                lineNo++;
            }
            return _Ret;
        }
    }
}
