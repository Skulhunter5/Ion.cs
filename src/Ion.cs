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
            Console.WriteLine("TOKENS:");
            foreach(var token in tokens) Console.WriteLine(token);
            Console.WriteLine();
            Parser parser = new Parser(tokens);
            AST root = parser.run();
            Console.WriteLine("AST:");
            Console.WriteLine(root);
        }
        
    }

}
