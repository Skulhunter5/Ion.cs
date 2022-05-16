using System;
using System.IO;

namespace Ion {

    class Ion {

        private static readonly string linuxDir = "\\\\wsl$\\Ubuntu-20.04\\shared\\";
        private static readonly string inFile = linuxDir + "test.ion";
        private static readonly string outFile = linuxDir + "test.ion.asm";

        public static void Main(string[] args) {
            Lexer lexer = new Lexer(inFile, File.ReadAllText(inFile));
            var tokens = lexer.run();
            Parser parser = new Parser(tokens);
            Program program = parser.run();
            Console.WriteLine(program);
            string asm = program.GenerateAssembly();
            Console.WriteLine("Generated assembly with " + CountLines(asm) + " lines.");
            File.WriteAllText(outFile, asm);
        }

        private static int CountLines(string text) {
            int n = 1;
            for(int i = 0; i < text.Length; i++) if(text[i] == '\n') n++;
            return n;
        }
        
    }

}
