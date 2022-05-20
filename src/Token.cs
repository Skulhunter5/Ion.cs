namespace Ion {

    enum TokenType {
        // EOF
        EOF,
        // SEMICOLON
        SEMICOLON,
        // COLON,
        COLON,
        // valued TokenTypes
        KEYWORD,
        IDENTIFIER,
        INTEGER, FLOAT,
        // parenthesis
        LPAREN, RPAREN,
        LBRACE, RBRACE,
        LBRACK, RBRACK,
        // general operators
        STAR,
        // mathematical operators
        INCREMENT, DECREMENT,
        PLUS, MINUS, SLASH,
        // logical operators
        NOT,
        // comparisons
        EQ, NEQ, LT, GT, LTEQ, GTEQ,
        // assignment operators
        ASSIGN, PLUS_EQ, MINUS_EQ, STAR_EQ, SLASH_EQ,
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