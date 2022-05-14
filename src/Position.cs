namespace Ion {

    class Position {
        public Position(string file, int line, int column) {
            File = file;
            Line = line;
            Column = column;
        }

        public string File { get; }
        public int Line { get; }
        public int Column { get; }

        public Position Derive(int dLine, int dColumn) {
            return new Position(File, Line + dLine, Column + dColumn);
        }

        public override string ToString() {
            return File + ":" + Line + ":" + Column;
        }
    }

}