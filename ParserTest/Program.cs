using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NppPIALexer2;

namespace ParserTest {
    class Program {
        static void Main(string[] args) {
            StringBuilder stringToRead = new StringBuilder();
            stringToRead.AppendLine("int Test");
            stringToRead.AppendLine("//a comment");
            //stringToRead.AppendLine("int Test2 //another comment");
            stringToRead.AppendLine("3+2*5");
            //stringToRead.AppendLine("");
            //stringToRead.AppendLine("3*2+5");
            //stringToRead.AppendLine("Test=X\nTest2=Y");

            Tokenizer test = new Tokenizer( );
            LinkedList<Tokenizer.Token>.Enumerator x = 
                test.Tokenize(stringToRead.ToString()).GetEnumerator();
            while (true) {
                if (x.MoveNext()) {
                    Console.WriteLine(x.Current.GetValue());
                }
            }
        }
    }


}
