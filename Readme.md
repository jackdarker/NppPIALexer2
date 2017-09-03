﻿**********************************************************************************************
* NppPIALexer-Plugin for Notepad++ 6.0 (and above)
* by JK, 2017.
**********************************************************************************************
This plugin creates Intelisense data for PIA-Framework-projects and provides it for Auto-complete operations in the editor.
________________________________________________________________________________________________________________
Links
________________________________________________________________________________________________________________


http://docs.notepad-plus-plus.org/index.php/Messages_And_Notifications
http://www.scintilla.org/ScintillaDoc.html#Notifications
________________________________________________________________________________________________________________
Installation
________________________________________________________________________________________________________________
) NppNppPIALexer2.dll nach .../Npp/plugins kopieren

) SQLite.Interop.dll,System.Data.SQLite.dll, System.Data.SQLite.dll.config nach ../Npp kopieren

) unter Plugins->NppPIALexer2 aktivieren

) in ProjektView neues Projekt anlegen (innerhalb PIASmart-Projektverzeichnis)

) Rebuild zum erzeugen der Intelisense drücken

________________________________________________________________________________________________________________
Parser

            //  //xyz
            //  /* xyz */
            //  /*  xyz
            //  xyz */
            //  int i
            //  int i=0
            //  i=X
            //  using "xyz" as asd
            //  #include "xyz"
            //  function xyz()
            //  function xyz(bool bOK)
            //  function xyz(bool bOK) -> int asd
            //  { or }
            //  while (bOK || int>0)
________________________________________________________________________________________________________________
Erzeugen der Intelisense
________________________________________________________________________________________________________________

) Plugin parst Sequencen nach using ... um Objekte und deren Klassen zu kennen
	using "Objects\calculator\calcuclator.vi" as Calc

) Plugin parst Sequencen nach Funktionsdeklarationen  
	function kabum(bool x) -> bool,string

) Plugin parst Sequencen nach include; es muss berücksichtigt werden das bei include SeqA -> SeqB -> SeqC führt zu impliziten include SeqA -> SeqC
	A -> B
	B -> C
	C -> D
	C -> E
	-------------------- ergänzen
	A -> C	
	B -> D
	B -> E
	-------------------- ergänzen
	A -> D 
	A -> E	

) einbinden der Klassen-Intelisense s.u.

) das ganze wiederholen wenn Dateien geladen/gespeichert werden
________________________________________________________________________________________________________________
Datenmodel
________________________________________________________________________________________________________________
Intelisense wird in SQLite3 db gespeichert in Projektordner

Tabelle ObjectList 
ID		INT PRIMARY KEY     AUTOINCREMENT
Scope		TEXT		
Object		TEXT
ClassID		TEXT    NOT NULL
State		INT

Tabelle ObjectLinks
ID		INT PRIMARY KEY     AUTOINCREMENT
ID_ObjectList	INT		
ID_ObjectDecl	INT
ID_ObjectListRel	INT	

Tabelle ObjectDecl
ID		INT PRIMARY KEY     AUTOINCREMENT
ClassID		TEXT    NOT NULL
ClassType	INT	NOT NULL
Function	TEXT    NOT NULL
ParameterList	TEXT    
ReturnList	TEXT    
Descr		TEXT
State		INT
Time		INT					<= Zeitstempel als Sekunden seit Mitternacht des 1. Januar 1970

) Bedeutung von State: 
	0 = Objekt nicht auffindbar (d.h. eventuel aus DB löschen)
	1 = Objekt existiert 

) Verknüpfung der Tabellen über:
SELECT * from ObjectList inner join ObjectDecl on ObjectList.ClassID==ObjectDecl.ClassID;
?? aber Main.seq -> Functions.seq -> StandardObj.seq -> Trace
SELECT distinct tab1.Scope,tab2.Object,tab2.ClassID,Function from ObjectLinks inner join ObjectList as tab1 on tab1.ID==ObjectLinks.ID_ObjectList inner join ObjectDecl on ObjectDecl.ID==ObjectLinks.ID_ObjectDecl
inner join ObjectList as tab2 on tab2.ID==ObjectLinks.ID_ObjectListRel order by tab2.Object,Function;


z.B. ObjectList
Scope		Object	ClassID		
---------------------------------------------------------------------------------------------------------------------------------
1 Main.seq	Calc	Calculator			<= Definition Objekt-Klasse
2 Main.seq	Functions.seq	Functions.seq			<= Funktion in include-sequenz
3 Functions.seq	Functions.seq	Functions.seq			<= Funktion in dieser sequenz
4 Main.seq	Main.seq	Main.seq
5 Main.seq	bOK	bool				<= Variable in dieser Sequenz (global)
6 Main.seq::Main	sText	string				<= Variable in dieser Funktion (lokal)
7 Functions.seq	Trace	PrehTrace

z.B. ObjectDecl
ClassID		ClassType	Function	ParameterList		ReturnList		Descr
---------------------------------------------------------------------------------------------------------------------------------
10 Calculator	Class		boolAnd		(bool A,bool B)		-> bool X		x = A & B
11 Calculator	Class		floatEquals	(float A, float B)	-> bool X
12 Preh_Trace	Class		CheckSNState	(string SN,string Type)	-> bool OK ,string ACK
13 Preh_Trace	Class		CheckSNState	(string SN     )	-> bool OK 
14 Functions.seq	Seq	Homing		()			-> bool OK, string Text
15 Functions.seq	Seq	CloseDoor	(bool bClose)		-> bool OK
16 Main.seq	Seq		Main
17 bool		Typ
18 string	Typ

z.B. ObjectLinks
ID_ObjectList	ID_ObjectDecl
---------------------------------------------------------------------------------------------------------------------------------
1		10
1		11
3		14
3		15
3		12
3		13
2		3	weil Main.seq auf Functions.seq verweist...
2		14	müssen deren Variablen auch der Main.seq zugeordnet werden
2		15
2		12
2		13
________________________________________________________________________________________________________________
Abrufen von Intelisense
________________________________________________________________________________________________________________
1) der Lexer erkennt welches Sequenzfile bearbeitet wird 
2) bei Eingabe von "." oder " " oder "(" oder "->" startet der Lexer; nach 2-3 Zeichen Vorschlagliste einblenden
	(in Functions.seq eintippen) "Calc.bo"
   "Calc.boolAnd(bA,"true") -> bOK"
   nach " ": kann Variable oder Seq-Funktion oder KlassenObjekt sein
   nach ".": Objekt-Funktion, aber nur wenn erstes Zeichen nichtnumerisch und vorher Klassenobjekt
   nach "(": Parameterliste, aber nur wenn vorher Objekt-Funktion
   nach ")": Ende Parameterliste
   nach "-": 
   nach ">": Returnlist, aber nur wenn vorher "-" und davor Parameterliste mit ")" abgeschlossen
3) Nachschlagen: Scope=Functions.seq::CloseDoor	Obj=Calc Search=bo Typ=Class (weil ".")
	gibt es ein Objekt Calc im Scope Functions.seq::CloseDoor oder Functions.seq?
	ist es vom Typ Class?
	welche Function fängt mit "bo" an?

) (in Main.seq eintippen) "Ho"  das könnte eine Funktion, ein Klassen-Objekt oder ein Typ sein
) Nachschlagen in ObjList: Scope=Main.seq	Obj Like('Ho%') 	=> KlassenObjekt
  Nachschlagen in ObjDecl: ClassID Like("Ho%") ClassTyp=Typ		=> Typ
  Nachschlagen in ObjList join ObjDecl on ClassID: Function Like("Ho%")	Typ=Seq	=> Function

) kein Intelisense bei "//" ";" oder innerhalb ""

Project
|-->Main.seq
	|--> Calculator Calc	(by using)
	|--> bool bOK
	|--> Main
		|--> string sText
	|--> Functions.seq	(by include)
		|--> CloseDoor

|-->Tools.seq
	|--> Functions.seq	(by include)
		|--> CloseDoor

________________________________________________________________________________________________________________
Import von Klassen-Intelisense		
________________________________________________________________________________________________________________
) wird im Object gespeichert
	Objects\calculator\xyz_Commander.seq

) gleicher Syntax wie in .seq als Funktionsdeklaration
	//desc: returns the logical AND of 2 variables
	//param: bA 
	//param: bB
	//return: bReturn
	function boolAnd(bool bA, bool bB) -> bReturn

________________________________________________________________________________________________________________
  Build & Debug 
________________________________________________________________________________________________________________
- Build-Enviroment is Visual Studio 10
- in VS Configure Output directory to ..\Npp\plugins\ or copy everything manually
- run Npp and activate plugin
- in VS attach to Npp-process or configure debug to run NPP as external program
- log-file created in ...\Npp\plugins\NppPIALexer2.log
________________________________________________________________________________________________________________
  History  
________________________________________________________________________________________________________________
2017.08.20	JKubik
- created based on NppProject Plugin source

