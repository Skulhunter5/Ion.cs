namespace Ion {

    enum TokenType {
        EOF,

        SEMICOLON,

        KEYWORD,
        IDENTIFIER,
        INTEGER, FLOAT,

        LPAREN, RPAREN,
        LBRACE, RBRACE,
        LBRACK, RBRACK,

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
            string str = "Token(" + TokenType + ",position=\"" + Position + "\"";
            switch(TokenType) {
                case TokenType.KEYWORD:
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