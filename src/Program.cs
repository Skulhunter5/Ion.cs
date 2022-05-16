using System.Collections.Generic;

namespace Ion {

    class Program {
        public Program(Dictionary<string, Function> functions, Dictionary<string, Variable> variables) {
            Functions = functions;
            Variables = variables;
        }

        public Dictionary<string, Function> Functions { get; }
        public Dictionary<string, Variable> Variables { get; }

        public string GenerateAssembly() {
            if(!Functions.ContainsKey("main")) ErrorSystem.AddError_i(new MissingEntryPointError());

            string asm = "";
            asm += "BITS 64\n";

            asm += "segment .bss\n";
            asm += "    cbuf: resb 1\n";
            foreach(Variable variable in Variables.Values) asm += variable.GenerateAssembly();

            asm += "segment .text\n";
            foreach(Function function in Functions.Values) asm += function.GenerateAssembly();
            asm += "global _start\n";
            asm += "_start:\n";
            asm += "    call function_" + Functions["main"].Id + "\n";
            
            asm += "exit:\n";
            asm += "    mov rax, 60\n";
            asm += "    mov rdi, 0\n";
            asm += "    syscall\n";

            return asm;
        }

        public override string ToString() {
            string str = "Program:\n- functions:\n";
            foreach(Function function in Functions.Values) str += "  - '" + function.Identifier + "':\n    - body: " + function.Body + "\n";
            return str;
        }
    }

}