namespace Ion {

    class Variable {
        public Variable(string identifier) {
            Identifier = identifier;
        }

        public string Identifier { get; }

        public override string ToString() {
            return "Variable(identifier=\"" + Identifier + "\")";
        }
    }

}