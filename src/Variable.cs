namespace Ion {

    class Variable {

        private static int nextId = 0;
        private static int NextId() { return nextId++; }

        public Variable(string identifier) {
            Identifier = identifier;
        }

        public int Id { get; }
        public string Identifier { get; }

        public string GenerateAssembly() {
            return "var_" + Id + ": resq 1\n";
        }

        public override string ToString() {
            return "Variable(identifier=\"" + Identifier + "\")";
        }
    }

}