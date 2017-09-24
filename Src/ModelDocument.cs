using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using System.Data.SQLite; //SQLiteNET wrapper

namespace NppPIALexer2 {

	public class Obj {
	
	public Obj(String scope,String name, String classID, String Descr) {
		m_ID=0;
		m_Scope=scope;
		m_Name=name;
		m_ClassID=classID;
		m_Descr = Descr.Substring(0, Math.Min(Descr.Length,1000)); //limit size because DB cannot handle big strings
	}
	
	public long ID() {return m_ID;}
	public void updateID(long iD) {m_ID=iD;}
	public String Name(){return m_Name;}
	public String Scope(){return m_Scope;}
	public String ClassID(){return m_ClassID;}
	public String Description(){return m_Descr;}

	public String toString() {
		return Name();
	}
	private	long m_ID;
	private	String m_Scope;
	private	String m_Name;
	private	String m_ClassID;
	private	String m_Descr;
}

    public class ObjDecl {

	public enum TClassType { 
        tCTFunc=(0),	// its Class-Function
		tCTSeq =(1),		// its a function in a sequence
		tCTType=(2),		// a basic type like double
		tCTClass=(4)	// its a class/commander
	};	    
	
	public ObjDecl(String classID, TClassType type,String function,String parameter,String returns, String Descr) {
		m_ID=0;
		m_ClassID=classID;
		m_ClassType=type;
		m_Params=parameter;
		m_Returns=returns;
		m_Function=function;
		m_Descr = Descr.Substring(0, Math.Min(Descr.Length,1000)); //limit size because DB cannot handle big strings
	}
	public long ID() {return m_ID;}
	public void updateID(int iD) {m_ID=iD;}
	public String Function(){return m_Function;}
	public String Params(){return m_Params;}
	public String Returns(){return m_Returns;}
	public String ClassID(){return m_ClassID;}
	public TClassType ClassType() {return m_ClassType;}
	public String Description(){return m_Descr;}
	private	long m_ID;
	private	TClassType m_ClassType;
	private	String m_Function;
	private	String m_Params;
	private	String m_Returns;
	private	String m_ClassID;
	private	String m_Descr;

}
    public class ModelDocument : IDisposable {
        public static String FileExtension = ".seq";
    public static List<String> BASIC_TYPES {
        get {
            var ret = new List<String>();
            ret.Add("bool");
            ret.Add("int");
            ret.Add("double");
            ret.Add("string");
            ret.Add("variant");
            return ret;
        }
     }
    public void Dispose() {
        Dispose(true);
        //?? GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (disposing) { //???          
        }
    }
    private SQLiteTransaction m_TrUpdate;
    private SQLiteConnection m_dbConnection;
    private String m_ProjectDir;

    private int SEQ_VERSION = 2;
    public String getSourceDir() {
        if (SEQ_VERSION == 1) {
            return "\\SOURCE";
        }
        return "\\App\\plugins"; //pointing to compiled plugins
    }
    public String getSeqDir() {
        if (SEQ_VERSION == 1) {
            return "PRG\\SEQ";
        }
        return "Projects\\ZBF\\Sequences"; //Todo: we need project selector !
    }

    public ModelDocument(String ProjectPath) {
        //SEQ_VERSION = Version;
        m_ProjectDir = Path.GetDirectoryName(ProjectPath);
        //TODO	Eclipse shows error because of db when searching all files - how to hide the db
        String x=Path.Combine(m_ProjectDir, "Intelisense.db");
        InitDB(x);
    }
    public bool shutdown() {  //Todo: close DB-stuff
        return true;
    }
    /// <summary>
    /// Returns path relative to ProjectDir
    /// </summary>
    /// <param name="Path"></param>
    /// <returns></returns>
    char[] charsToTrim = { Path.DirectorySeparatorChar };
    public String GetRelativePath(String Path) {
        String _Ret = "";
        if (Path!=null & m_ProjectDir!=null && 
            Path.Length > m_ProjectDir.Length) {
            if(m_ProjectDir.Equals(Path.Substring(0,m_ProjectDir.Length))) {
                _Ret = Path.Substring(m_ProjectDir.Length).TrimStart(charsToTrim);
            }

        }
        return _Ret;
    }
    private SQLiteConnection GetDB() {
        return m_dbConnection;
    }
    //opens or creates the Intelisense-DB
    private void InitDB(String Path) {
        Boolean Init = true;
        try {

           // SQLiteConnection.CreateFile(Path);
            m_dbConnection = new SQLiteConnection("Data Source=" + Path + ";Version=3;");
            m_dbConnection.Open();
          
            SQLiteCommand command = new SQLiteCommand("SELECT count(*) FROM sqlite_master WHERE type='table' AND name='Config' COLLATE NOCASE", m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
                Init = (reader.GetInt32(0)== 0);
            reader.Dispose();
            command.Dispose();

            if (Init) {

                ExecuteSimpleQuery("DROP TABLE IF EXISTS Config");
                ExecuteSimpleQuery("DROP TABLE IF EXISTS ObjectList");
                ExecuteSimpleQuery("DROP TABLE IF EXISTS ObjectDecl");
                ExecuteSimpleQuery("DROP TABLE IF EXISTS ObjectLinks");
                ExecuteSimpleQuery("DROP TABLE IF EXISTS ObjectLinksTemp");
                ExecuteSimpleQuery("DROP TABLE IF EXISTS Markers");
                ExecuteSimpleQuery("CREATE TABLE Config " +
                               "(ID INTEGER PRIMARY KEY   AUTOINCREMENT," +
                               " Name TEXT NOT NULL, " +
                               " ValueInt INT , " +
                               " ValueText TEXT )");

                ExecuteSimpleQuery("CREATE TABLE ObjectList (" +
                          " ID INTEGER PRIMARY KEY AUTOINCREMENT,File TEXT," +
                          " Scope TEXT, Object TEXT, ClassID TEXT NOT NULL, State INT,Descr TEXT)");
                ExecuteSimpleQuery("CREATE TABLE ObjectDecl (" +
                          " ID INTEGER PRIMARY KEY AUTOINCREMENT, ClassID TEXT NOT NULL," +
                          " Function TEXT NOT NULL, Params TEXT, Returns TEXT, ClassType INT, State INT, Time INT, Descr TEXT)");
                ExecuteSimpleQuery("CREATE TABLE ObjectLinks (" +
                          " ID INTEGER PRIMARY KEY AUTOINCREMENT, ID_ObjectList INT," +
                          " ID_ObjectDecl INT, ID_ObjectListRel INT)");
                ExecuteSimpleQuery("CREATE TABLE ObjectLinksTemp (ID_A INT, ID_B INT)");
                ExecuteSimpleQuery("CREATE INDEX i1 ON ObjectLinksTemp (ID_A,ID_B);");
                ExecuteSimpleQuery("CREATE TABLE Markers (" +
                          " ID INTEGER PRIMARY KEY AUTOINCREMENT, Type INT," +
                          " Scope TEXT, Start INT, Length INT)");

            }

            UpdateConfig("Version", "", 100);

            //////////////////////////////////////////////////////////////////
        } catch (Exception e) {
            HandleDBError(e);
        }
    }
    public void RebuildAll() {
        PrepareForUpdate();
        RebuildObjList();
        CleanupDeadLinks();
    }

    // marks every entry that needs to be updated
    public void PrepareForUpdate() {
        ExecuteSimpleQuery("Update ObjectList Set State=0;");
        ExecuteSimpleQuery("Update ObjectDecl Set State=0;");
        try {
            m_TrUpdate = GetDB().BeginTransaction();
            //??GetDB().AutoCommit=(false); // no Auto-commit !!
        } catch (Exception e) {
            HandleDBError(e);
        }
        //??ExecuteSimpleQuery("Begin transaction;");  
        //Rebuild-Functions will call alot inserts and if we dont wrap them into an transaction it will be slow	
        
    }
    //deletes entrys that are not valid anymore and recreates new
    public void CleanupDeadLinks() {
        RebuildClassDefinition();
        //nachdem wir die Klassendeklarationen importiert haben 
        // müssen die Links von der Seq auf die Klasse neu erstellt werden
        // Main.seq-> Calc -> Calculator
        // aber auch tiefer verlinkt
        // Main.seq->test.seq->functions.seq-> Trace-> PrehTrace
        // zu Main.seq -> Trace -> PrehTrace
        String _SQL;
        //clear the old data
        ExecuteSimpleQuery("delete from ObjectLinksTemp;");
        ExecuteSimpleQuery("delete from ObjectLinks;");

        //erstmal die einfachen Verlinkungen eintragen
        //SELECT Scope,Object,Function,ObjectList.ClassID,ObjectDecl.ClassID,ClassType,
        //ObjectList.ID,ObjectDecl.ID from ObjectList inner join ObjectDecl on ObjectList.ClassID==ObjectDecl.ClassID;
        _SQL = "Insert Into ObjectLinks (ID_ObjectList,ID_ObjectDecl, ID_ObjectListRel) " +
            " SELECT ObjectList.ID,ObjectDecl.ID,ObjectList.ID from ObjectList inner join ObjectDecl on ObjectList.ClassID==ObjectDecl.ClassID;";
        ExecuteSimpleQuery(_SQL);

        //jetzt für jede Seq prüfen in welcher andere Seq sie included ist (ID_ObjectListA -> ID_ObjectListB); in temp. Tabelle eintragen	
        _SQL = "Insert Into ObjectLinksTemp (ID_A,ID_B) " +
            " SELECT distinct tab1.ID,tab2.ID FROM ObjectList as tab1 inner join ObjectList as tab2 on tab1.ClassID==tab2.Scope " +
            " left join ObjectDecl on ObjectDecl.ClassID==tab2.ClassID ";
        ExecuteSimpleQuery(_SQL);

        // in der temporären Tabelle werden die Seq-Verknüpfungen aufgelöst: 
        // 1) SELECT tab2.id,tab1.value FROM MyTable as tab1 inner join MyTable as tab2 on tab1.id==tab2.value; ausführen
        // 2) zurückgelieferte Ergebnisse in Tabelle anfügen
        // 3) das ganze so lange wiederholen bis Select kein Ergebnis mehr liefert
        // die Tabelle enthält nun für jede Seq auch Verweise auf indirekt eingebundene Seq
        // Meine Fresse diese query verstehe ich schon jetzt nicht mehr
        String _SQLSelect = "SELECT distinct tab2.ID_A,tab1.ID_B FROM ObjectLinksTemp as tab1 inner join " +
            " ObjectLinksTemp as tab2 on (tab1.ID_A==tab2.ID_B AND tab1.ID_A!=tab1.ID_B AND tab2.ID_A!=tab2.ID_B ) " +
            " where not exists (SELECT ID_A, ID_B FROM ObjectLinksTemp where ID_A==tab2.ID_A AND ID_B==tab1.ID_B);";
        _SQL = "Insert Into ObjectLinksTemp (ID_A,ID_B) ";  //Todo sub-seq are still listed doubled ?!
        //Todo this is slow because of the nested Select; use EXCEPT/INTERSECT instead??
        _SQL += _SQLSelect;
        Boolean _Finished = false;
        try {
            while (!_Finished) {
                _Finished = true;
                ExecuteSimpleQuery(_SQL);
                SQLiteCommand command = new SQLiteCommand(_SQLSelect, m_dbConnection);
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read() && !_Finished) {
                    _Finished = false;
                }
                reader.Dispose();
                command.Dispose();
            }

            //jetzt für jeden Eintrag in temp. Tabelle die bereits vorhandenen Einträge in ObjectLinks duplizieren 
            // 1) Insert INTO ObjectLinks (ID_ObjectList,ID_ObjectDecl) SELECT Mytable.ID,ID_ObjectDecl from ObjectLinks inner join MyTable on Mytable.value==ID_ObjectList
            _SQL = "Insert INTO ObjectLinks (ID_ObjectList,ID_ObjectDecl, ID_ObjectListRel) " +
                "SELECT ObjectLinksTemp.ID_A,ObjectLinks.ID_ObjectDecl,ObjectLinksTemp.ID_B " +
                "from ObjectLinksTemp left join ObjectLinks on ObjectLinksTemp.ID_B==ObjectLinks.ID_ObjectList where ObjectLinksTemp.ID_A!=ObjectLinksTemp.ID_B";
            ExecuteSimpleQuery(_SQL);
            m_TrUpdate.Commit();
            //GetDB().commit();//ExecuteSimpleQuery("Commit transaction;"); //finish Rebuild-Transaction
            //??GetDB().setAutoCommit(true);
        } catch (Exception e) {
            HandleDBError(e);
        }
    }
    
    void RebuildObjList() {
    	
    // add the basic types to Intelisense
    ObjDecl _A; 
    for (int i=0 ; i<BASIC_TYPES.Count;i++ ) {
    	_A= new ObjDecl(BASIC_TYPES[i],ObjDecl.TClassType.tCTType,"","","","");
    	UpdateObjDecl(_A);
    }

   // Parser _parser = new Parser(this, m_ProjectDir);
    Tokenizer _tokenizer = new Tokenizer();
    LinkedList<Tokenizer.Token> _Tokens = new LinkedList<Tokenizer.Token>(); 
    List<String> Dirs = new List<String>() ;  //stack of directories relative to _path
    int k=0;
    try {
    	Dirs.Add( Path.Combine(m_ProjectDir,getSeqDir()));
    	while (k< Dirs.Count) {
            FileInfo[] _files = new DirectoryInfo(Dirs[k]).GetFiles();
            for (int i = 0; i < _files.Length; i++ ) {
                if (_files[i].Extension.Equals(".seq",StringComparison.OrdinalIgnoreCase)) {
                    //??_parser.AnalyseFile(_files[i].FullName);
                    _Tokens.AddLast(_tokenizer.TokenizeFile(_files[i].FullName));
                }
            }
            DirectoryInfo[] _Dirs = new DirectoryInfo(Dirs[k]).GetDirectories();
            for (int i = 0; i < _Dirs.Length; i++ ) {
                // If the file is a directory (or is in some way invalid) we'll skip it
                    Dirs.Add(_Dirs[i].FullName);
            }
            k++;
    	}
        Parser2 _parser2 = new Parser2(this, m_ProjectDir);
        _parser2.ParseTokens(_Tokens);
        //Todo update database with parserresult
        LinkedList<Parser2.CmdBase>.Enumerator _Cmds;
        List<String>.Enumerator _Scopes = _parser2.GetScopes().GetEnumerator();
        while(_Scopes.MoveNext()) {
           // if(!m_IsClassDef) {//each SEQ includes itself
            {
                this.UpdateObjList(new Obj(_Scopes.Current, "", _Scopes.Current, ""));
            }
            _Cmds=_parser2.GetCmds(_Scopes.Current).GetEnumerator();
            while(_Cmds.MoveNext()) {
                PublishCmdToDB(_Scopes.Current, _Cmds.Current);
            }

        }
        
    }
    finally {	
    }
    	       	
    }
    void PublishCmdToDB (String Scope, Parser2.CmdBase Cmd ) {
        if(Cmd.GetType().Equals(typeof(Parser2.CmdDecl))) {
            Parser2.CmdDecl Cmd2 = (Parser2.CmdDecl)Cmd;
            Obj _obj = new Obj(Scope, Cmd2.m_Name, Cmd2.m_Type, "m_Comment");
            UpdateObjList(_obj);
        } else if(Cmd.GetType().Equals(typeof(Parser2.CmdInclude))) {
            Parser2.CmdInclude Cmd2 = (Parser2.CmdInclude)Cmd; 
            Obj _obj = new Obj(Scope, Cmd2.m_Path,
                 m_ProjectDir + getSeqDir() + "\\" + Cmd2.m_Path, "m_Comment");	//Todo das ist falsch bei Subproject-Includes
            UpdateObjList(_obj);
        } else if(Cmd.GetType().Equals(typeof(Parser2.CmdUsing))) {
            Parser2.CmdUsing Cmd2 = (Parser2.CmdUsing)Cmd;
            Obj _obj = new Obj(Scope, Cmd2.m_Name,
                        m_ProjectDir + getSourceDir() + "\\" + Cmd2.m_Path, "m_Comment");
            UpdateObjList(_obj);
        } else if(Cmd.GetType().Equals(typeof(Parser2.CmdFunctionDecl))) {
            Parser2.CmdFunctionDecl Cmd2 = (Parser2.CmdFunctionDecl)Cmd;
            ObjDecl _objDecl = new ObjDecl(Scope,
                           /* m_IsClassDef*/false ? ObjDecl.TClassType.tCTFunc : ObjDecl.TClassType.tCTSeq,
                                Cmd2.m_Name, Cmd2.m_Params.ToString(), Cmd2.m_Returns.ToString(), "m_Comment");
            UpdateObjDecl(_objDecl);
        }
    }
    //after building ObjList we now have to compile each class into ObjDecl
    void RebuildClassDefinition() {
        /*
        //find each entry in ObjList that is not linked to ObjDecl
    //??	SeqParser _parser(this);
        String _SQL ="SELECT distinct ObjectList.ClassID as ID2,ObjectDecl.ClassID as ID1 from ObjectList " +
                "Left outer join ObjectDecl on ObjectList.ClassID==ObjectDecl.ClassID where ID1 IS NULL;";
        String _filepath;
        Statement stmt = null;
        try {
            stmt = s_DBConn.createStatement();
            ResultSet rs = stmt.executeQuery( _SQL );
            while ( rs.next() ) {
               _filepath= rs.getString("ID2");
        //??       _parser.AnalyseFile(true,m_ProjectPath,_filepath);
            }
            rs.close();
            stmt.close();

        } catch ( Exception e ) {
            HandleDBError( e);
        }*/
        //clear the old data
        ExecuteSimpleQuery("delete from ObjectList where State==0;");
        ExecuteSimpleQuery("delete from ObjectDecl where State==0;");
    }
    private void HandleDBError(Exception e) {
        Utility.Debug("database issue: {0}\n{1}", e.Message, e.StackTrace);
    }
    //writes a value in the config-Table
    private void UpdateConfig(String Name, String ValueString, int ValueInt) {
        String sql = "";
        Boolean Update = false;
        try {
            SQLiteCommand command = new SQLiteCommand(
                "SELECT ID,Name,ValueText,ValueInt FROM Config where Name='" + Name + "';", m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                Update = true;
                int id = reader.GetInt32(reader.GetOrdinal("ID"));
                String name = reader.GetString(reader.GetOrdinal("Name"));
            }
            reader.Dispose();
            command.Dispose();

            if (Update) {
                sql = "Update Config Set ValueText='" + ValueString +
                        "',ValueInt=" + String.Format("{0}", ValueInt) +
                        " where Name='" + Name + "';";
            } else {
                sql = "INSERT INTO Config (Name,ValueText,ValueInt) " +
                         "VALUES ( '" + Name + "','" + ValueString + "'," +
                            String.Format("{0}", ValueInt) + ");";
            }
            ExecuteSimpleQuery(sql);
        } catch (Exception e) {
            HandleDBError(e);
        }
    }
    //gets a value from the config-table
    private int GetConfigInt(String Name) {
        int Return = 0;
        Boolean Exists = false;
        try {
            SQLiteCommand command = new SQLiteCommand(
                "SELECT ID,Name,ValueText,ValueInt FROM Config;", m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                //Todo fix non exist
                Exists = true;
                Return = reader.GetInt32(reader.GetOrdinal("ValueInt"));
            }
            reader.Dispose();
            command.Dispose();
        } catch (Exception e) {
            HandleDBError(e);
        }
        return Return;
    }
    //gets a value from the config-table
    private String GetConfigString(String Name) {
        String Return = "";
        Boolean Exists = false;
        try {
            SQLiteCommand command = new SQLiteCommand(
                "SELECT ID,Name,ValueText,ValueInt FROM Config;", m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                //Todo fix non exist
                Exists = true;
                Return = reader.GetString(reader.GetOrdinal("ValueString"));
            }
            reader.Dispose();
            command.Dispose();
            
        } catch (Exception e) {
            HandleDBError(e);
        }
        return Return;
    }

    //insert/update Object
    public void UpdateObjList(Obj theObj) {
        RefreshObjListID(theObj);
        String _SQL = ("");
        if (theObj.ID() > 0) {
            _SQL = "Update ObjectList Set Scope='" + theObj.Scope() +
                "',Object='" + theObj.Name() +
                "',ClassID='" + theObj.ClassID() +
                "',State=1" +
                " ,Descr='" + theObj.Description() +
                " ' where ID=" + theObj.ID().ToString();
        } else {
            _SQL = "INSERT INTO ObjectList (Scope , Object , ClassID, State, Descr) VALUES('" +
                    theObj.Scope() + "', '" +
                    theObj.Name() + "', '" + theObj.ClassID() +
                    "',1,'" + theObj.Description() + "');";
        }
        ExecuteSimpleQuery(_SQL);
        RefreshObjListID(theObj);	//get Ids after insert
    }
    //fetch the primary key for the dataset or set to invalid
    int RefreshObjListID(Obj theObj) {
        String _SQL = ("SELECT ID from ObjectList where Scope=='");
        _SQL += theObj.Scope() + "' AND Object='" + theObj.Name() + "';";
        int Return = 0;
        Boolean Exists = false;
        try {
            SQLiteCommand command = new SQLiteCommand(_SQL, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                //Todo fix non exist
                Exists = true;
                Return = reader.GetInt32(reader.GetOrdinal("ID"));
            }
            reader.Dispose();
            command.Dispose();
        } catch (Exception e) {
            HandleDBError(e);
        }
        theObj.updateID(Return);
        return Return;
    }
    //insert/update ObjectDeclaration
    public void UpdateObjDecl(ObjDecl theObj) {
        RefreshObjDeclID(theObj);

        DateTime Now = DateTime.Now;
        String _SQL;

        if (theObj.ID() > 0) {
            _SQL = "Update ObjectDecl Set ClassID='" +
                    theObj.ClassID() +
                "',Function='" + theObj.Function() +
                "',Params='" + theObj.Params() +
                "',Returns='" + theObj.Returns() +
                "',ClassType=" + ((int)theObj.ClassType()).ToString() +
                " ,State=1" +
                " ,Descr='" + theObj.Description() +
                " ',Time=" + (Now.ToBinary().ToString()) +
                " where ID=" + (theObj.ID().ToString());
        } else {
            _SQL = "INSERT INTO ObjectDecl (ClassID, Function, Params, Returns, ClassType, State, Time,Descr) VALUES('" +
            theObj.ClassID() +
                "', '" + theObj.Function() + "', '" + theObj.Params() +
                "', '" + theObj.Returns() + "', " + ((int)theObj.ClassType()).ToString() +
                ",1" + "," + (Now.ToBinary().ToString()) + ",'" + theObj.Description() + "');";
        }

        ExecuteSimpleQuery(_SQL);
        RefreshObjDeclID(theObj);	//get Ids after insert
    }
    //fetch the primary key for the dataset or set to invalid
    int RefreshObjDeclID(ObjDecl theObj) {
        String _SQL = ("SELECT ID from ObjectDecl where ClassID=='");
        _SQL += theObj.ClassID() + "' AND Function='" + theObj.Function() + "';";
        int Return = 0;
        Boolean Exists = false;
        try {
            SQLiteCommand command = new SQLiteCommand(_SQL, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                //Todo fix non exist
                Exists = true;
                Return = reader.GetInt32(reader.GetOrdinal("ID"));
            }
            reader.Dispose();
            command.Dispose();        
        } catch (Exception e) {
            HandleDBError(e);
        }
        theObj.updateID(Return);
        return Return;

    }
    private void ExecuteSimpleQuery(String SQL) {
        try {
            SQLiteCommand command = new SQLiteCommand(SQL, m_dbConnection);
            command.ExecuteNonQuery();
            command.Dispose();

        } catch (Exception e) {
            HandleDBError(e);
        }
    }
    public List<ObjDecl> lookupAll(String Word, String Intent, String Scope)  {
        //  List<ObjDecl> proposals = new List<ObjDecl>();
	    
	    int _EndOfObject=Intent.IndexOf(".");
	    int _EndOfFunction=Intent.IndexOf("(");
	    int _EndOfParams=Intent.IndexOf(")");
	    String _Object = "";
	    String _Function = "";
	    String _Search ="";
	    
	    if (_EndOfObject>=0) {
	    	_Object=Intent.Substring(0, _EndOfObject);	//entered Obj.   	->show function of Object
	    	if (_EndOfFunction>=0) {
                _Function = Intent.Substring(_EndOfObject + 1, _EndOfFunction - (_EndOfObject + 1)); //entered Obj.foo(  ->show function of Object
	    	} else if (_EndOfObject+1<Intent.Length){
                _Search = Intent.Substring(_EndOfObject + 1, Intent.Length - (_EndOfObject + 1)); //entered Obj.foo  ->show function of Object
	    	} else {
	    		//Todo
	    		// wenn "Calc.boolAnd(b" eingegeben wird was sollte weiterhin die Funktionsdeklaration eingeblendet werden
	    		// aber auch passende Variablen eingeblednet werden, z.b. "bReturn"
	    		// d.h. Filtern nach Variablen mit Typ aus Parameterliste
	    	}   		
	    } else {
	    	if (_EndOfFunction>=0) {
	    		_Function = Intent.Substring(0, _EndOfFunction); //entered foo(  ->show function of Seq
	    	} else {
	    		_Search=Intent; //entered Obj  ->show Objects
	    	}
	    }
	    _Search= _Search.Trim();
	    _Function = _Function.Trim();
	    
	    List<ObjDecl> _found;
        if (!_Search.Equals("") && _Function.Equals("")) {
	    	_found =SearchObject(_Search,Scope,_Object);
        } else if (!_Function.Equals("")) {
	    	_found =SearchObject(_Function,Scope,_Object);
        } else if (!_Search.Equals("")) {
	    	_found = new List<ObjDecl>(); 
	    	//Todo show params
	    } else { //Search is empty
	    	_found = new List<ObjDecl>();
	    }
	    String _Descr;
	  // ContextInformation _context;
      
	  /*  List<ObjDecl>.Enumerator iter=_found.GetEnumerator();
	    while (iter.MoveNext())
	    {
	    	//_context=null;
	    	ObjDecl fct = iter.Current;
            int _length = (_Search.Length - _Object.Length - _Function.Length);
	    	if (!_Object.Equals("")) _length--;
            if (fct.ClassType() == ObjDecl.TClassType.tCTFunc || fct.ClassType() == ObjDecl.TClassType.tCTSeq) {
	    		_Descr=fct.Function()+"(" + fct.Params()+")->" + fct.Returns()+"\n"+
	            		fct.Description() +fct.ClassID();
	    		//_context = new ContextInformation("A","B");
            } else if (fct.ClassType() == ObjDecl.TClassType.tCTType) {
	    		_Descr=fct.ClassID()+" "+fct.Function()+"\n"+fct.Description();
	    	} else {
	    		_Descr=fct.Function()+"\n"+fct.Description()+fct.ClassID();
	    	}
	    	proposals.Add(new CompletionProposal(
	            fct.Function(),
	            offset - _length,
	            _length,
	            fct.Function().length(),
	            Resources.IMG_BULLET_GREEN,
	            fct.Function(),
	            _context,_Descr));
	    }
	    return proposals;*/
        return _found;
	  }
    //liefert die Objekte und Variablen einer Sequenz
    public List<Obj> GetObjects(String Scope) {
        List<Obj> Result = new List<Obj>();
        String _SQL = "SELECT Scope,Object, ClassID FROM ObjectList "; //Todo da fehlen die Variablen aus Subsequencen
        _SQL += " where Scope Like('" + Scope + "') AND Object!=''";
        try {
            SQLiteCommand command = new SQLiteCommand(_SQL, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                Result.Add(new Obj(reader.GetString(reader.GetOrdinal("Scope")),
                        reader.GetString(reader.GetOrdinal("Object")),
                        reader.GetString(reader.GetOrdinal("ClassID")), ""));
            }
            reader.Dispose();
            command.Dispose();
            
        } catch (Exception e) {
            HandleDBError(e);
        } finally {
        }
        return Result;
    }
    //liefert die Funktionen einer Sequenz
    public List<ObjDecl> GetFunctions(String Scope) {
        List<ObjDecl> Result = new List<ObjDecl>();
        String _SQL2 = " from ObjectLinks inner join ObjectList as tab1 on tab1.ID==ObjectLinks.ID_ObjectList" +
             " inner join ObjectList as tab2 on tab2.ID==ObjectLinks.ID_ObjectListRel " +
             " inner join ObjectDecl on ObjectDecl.ID==ObjectLinks.ID_ObjectDecl ";
        String _SQL = "SELECT distinct tab1.ClassID,Function,Params,Returns,ClassType "; //Todo da kommen doppelte Einträge bei Funktionen aus Subsequencen
        _SQL += _SQL2;
        _SQL += " where tab1.Scope Like('" + Scope + "') AND ObjectDecl.ClassType==" +
                ((int)ObjDecl.TClassType.tCTSeq).ToString();
        try {
            SQLiteCommand command = new SQLiteCommand(_SQL, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                Result.Add(new ObjDecl(reader.GetString(reader.GetOrdinal("ClassID")),
                        (ObjDecl.TClassType)reader.GetInt32(reader.GetOrdinal("ClassType")),
                        reader.GetString(reader.GetOrdinal("Function")),
                        reader.GetString(reader.GetOrdinal("Params")),
                        reader.GetString(reader.GetOrdinal("Returns")), ""));
            }
            reader.Dispose();
            command.Dispose();
        } catch (Exception e) {
            HandleDBError(e);
        } finally {
        }
        return Result;
    }
    public List<String> GetParams(String Scope, String Object, String Function) {
        List<String> Result = new List<String>();
        String _SQL;
        // Todo maybe its more efficient to put this into stored procedure?
        String _SQL2 = " from ObjectLinks inner join ObjectList as tab1 on tab1.ID==ObjectLinks.ID_ObjectList " +
              "inner join ObjectDecl on ObjectDecl.ID==ObjectLinks.ID_ObjectDecl " +
              "inner join ObjectList as tab2 on tab2.ID==ObjectLinks.ID_ObjectListRel ";

        _SQL = "SELECT distinct Params" + _SQL2 + "where tab1.Scope=='";
        _SQL += Scope + "' AND tab2.Object=='" + Object + "' AND Function=='" + Function + "'" + " order by Params;";

        try {
            SQLiteCommand command = new SQLiteCommand(_SQL, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                Result.Add(reader.GetString(reader.GetOrdinal("Params")));
            }
            reader.Dispose();
            command.Dispose();
            
        } catch (Exception e) {
            HandleDBError(e);
        } finally {
        }
        return Result;

    }
    public List<String> GetReturns(String Scope, String Object, String Function) {
        List<String> Result = new List<String>();
        String _SQL;
        // Todo maybe its more efficient to put this into stored procedure?
        String _SQL2 = " from ObjectLinks inner join ObjectList as tab1 on tab1.ID==ObjectLinks.ID_ObjectList " +
              "inner join ObjectDecl on ObjectDecl.ID==ObjectLinks.ID_ObjectDecl" +
              "inner join ObjectList as tab2 on tab2.ID==ObjectLinks.ID_ObjectListRel ";

        _SQL = "SELECT distinct Returns" + _SQL2 + "where tab1.Scope=='";
        _SQL += Scope + "' AND tab2.Object=='" + Object + "' AND Function=='" + Function + "'" + " order by Params;";
        try {
            SQLiteCommand command = new SQLiteCommand(_SQL, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                Result.Add(reader.GetString(reader.GetOrdinal("Returns")));
            }
            reader.Dispose();
            command.Dispose();
            
        } catch (Exception e) {
            HandleDBError(e);
        } finally {
        }
        return Result;

    }
    public List<ObjDecl> SearchObject(String BeginsWith, String Scope, String Object) {
        List<ObjDecl> Result = new List<ObjDecl>();
        String _SQL;
        // Todo maybe its more efficient to put this into stored procedure?
        String _SQL2 = " from ObjectLinks inner join ObjectList as tab1 on tab1.ID==ObjectLinks.ID_ObjectList" +
             " inner join ObjectList as tab2 on tab2.ID==ObjectLinks.ID_ObjectListRel " +
             " left join ObjectDecl on ObjectDecl.ID==ObjectLinks.ID_ObjectDecl ";
        String _ClassType;
        if (Object.Equals("")) {
            // because output needs to be sorted, the UNION needs to be wrapped in additional SELECT for ordering
            _SQL = "select Col1,_ClassTyp,ClassID,Descr,Params,Returns From ( ";
            //is it a class-object
            //because left join ObjectDecl ClassType might be null if no Class-SEQ was imported
            _ClassType = ((int)ObjDecl.TClassType.tCTClass).ToString();
            _SQL += "SELECT distinct tab2.Object as Col1," + _ClassType +
                    " as _ClassTyp, tab2.ClassID,tab1.Descr, '' as Params, '' as Returns " + _SQL2 + "where tab1.Scope=='";
            _SQL += Scope + "'" + " AND (ClassType isnull OR ClassType==" + ((int)ObjDecl.TClassType.tCTFunc).ToString() + ")" +  //??
                    " AND tab2.Object Like('" + BeginsWith + "%') ";
            _SQL = _SQL + " UNION ";
            //is it a SEQ-function
            _ClassType = ((int)ObjDecl.TClassType.tCTSeq).ToString();
            _SQL = _SQL + "SELECT distinct Function as Col1," + _ClassType +
                    " as _ClassTyp, tab2.ClassID,ObjectDecl.Descr, ObjectDecl.Params, ObjectDecl.Returns " + _SQL2 + "where tab1.Scope=='";
            _SQL = _SQL + Scope + "' AND ClassType==" + _ClassType +
                    " AND Function Like('" + BeginsWith + "%')";
            _SQL = _SQL + " UNION ";
            //is it a variable of basic type
            _ClassType = ((int)ObjDecl.TClassType.tCTType).ToString();
            _SQL = _SQL + "SELECT distinct tab2.Object as Col1," + _ClassType +
                    " as _ClassTyp, tab2.ClassID ,tab2.Descr, '' as Params, '' as Returns " + _SQL2 + "where tab1.Scope=='";
            _SQL = _SQL + Scope + "' AND ClassType==" + _ClassType + " AND tab2.Object Like('" + BeginsWith + "%')";
            _SQL = _SQL + ") order by Col1; ";

        } else { // its a function of an object
            _ClassType = ((int)ObjDecl.TClassType.tCTFunc).ToString();
            _SQL = "SELECT distinct Function as Col1,ObjectDecl.ClassID,ObjectDecl.Descr, ObjectDecl.Params, ObjectDecl.Returns, " +
            _ClassType + " as _ClassTyp" + _SQL2 + "where tab1.Scope=='";
            _SQL += Scope + "' AND tab2.Object=='" + Object + "' AND Function Like('" + BeginsWith + "%')" + " order by Function;";

        }
        try {
            SQLiteCommand command = new SQLiteCommand(_SQL, m_dbConnection);
            SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read()) {
                Result.Add(new ObjDecl((reader.GetString(reader.GetOrdinal("ClassID"))),
                        (ObjDecl.TClassType)(reader.GetInt32(reader.GetOrdinal("_ClassTyp"))),
                        reader.GetString(reader.GetOrdinal("Col1")),
                        reader.GetString(reader.GetOrdinal("Params")), reader.GetString(reader.GetOrdinal("Returns")),
                        reader.GetString(reader.GetOrdinal("Descr"))));
            }
            reader.Dispose();
            command.Dispose();

        } catch (Exception e) {
            HandleDBError(e);
        } finally {
        }
        return Result;
    }

}
}
