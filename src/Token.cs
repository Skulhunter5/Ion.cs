namespace Ion {

    enum TokenType {
        EOF,

        SEMICOLON,

        IDENTIFIER,
        INTEGER, FLOAT,

        EQ, LT, GT, LTEQ, GTEQ, NEQ,
        ASSIGN, NOT,
    }

    class Token {
        public Token(TokenType tokenType, Position position) {
            TokenType = tokenType;
            Position = position;
        }
        public Token(TokenType tokenType, Position position, string value) {
            TokenType = tokenType;
            Position = position;
            Value = value;
        }

        public TokenType TokenType { get; }
        public Position Position { get; }
        public string Value { get; }

        public override string ToString() {
            string str = "Token(" + TokenType;
            switch(TokenType) {
                case TokenType.IDENTIFIER:
                case TokenType.INTEGER:
                case TokenType.FLOAT:
                    str += ",value=\"" + Value + "\"";
                    break;
            }
            return str + ")";
        }

    }

}