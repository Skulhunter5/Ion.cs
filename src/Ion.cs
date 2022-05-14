using System;
using System.IO;

namespace Ion {

    class Program {

        private static readonly string linuxDir = "\\\\wsl$\\Ubuntu-20.04\\shared\\";
        private static readonly string inFile = linuxDir + "test.ion";
        private static readonly string outFile = linuxDir + "test.ion.asm";

        public static void Main(string[] args) {
            Lexer lexer = new Lexer(inFile, File.ReadAllText(inFile));
            var tokens = lexer.run();
            foreach(var token in tokens) {
                Console.WriteLine(token);
            }
        }
        
    }

}
