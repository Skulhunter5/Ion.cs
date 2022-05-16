namespace Ion {

    class Function {

        private static int nextId = 0;
        private static int NextId() { return nextId++; }

        public Function(string identifier, AST body) {
            Id = NextId();

            Identifier = identifier;
            Body = body;
        }

        public int Id { get; }
        public string Identifier { get; }
        public AST Body { get; }

        public string GenerateAssembly() {
            string asm = "";
            asm += "function_" + Id + ": ;; " + Identifier + "\n";
            asm += Body.GenerateAssembly();
            asm += "    ret\n";
            return asm;
        }

        public override string ToString() {
            return "Function:\n- identifier: " + Identifier + "'\n- body: " + Body;
        }
    }

}