using System.Collections.Generic;

namespace Ion {

    class Lexer {

        private string _file, _text;

        private int _position;
        private int _line = 1, _column = 1;

        public Lexer(string file, string text) {
            _file = file;
            _text = text;
        }

        private char c {
            get {
                if(_position >= _text.Length) return '\0';
                return _text[_position];
            }
        }

        private char Peak(int offset) {
            if(_position + offset >= _text.Length) return '\0';
            return _text[_position + offset];
        }

        private char Advance() {
            _position++;
            _column++;
            while(c == '\n') {
                _line++;
                _column = 1;
                _position++;
            }
            return c;
        }

        private Token AdvanceWith(Token token) {
            Advance();
            return token;
        }

        private void SkipWhiteSpace() {
            while(char.IsWhiteSpace(c)) Advance();
        }

        private Token ParseNumber() {
            Position position = new Position(_file, _line, _column);
            string value = "";
            bool isFloat = false;
            while(char.IsDigit(c) || c == '.') {
                value += c;
                if(c == '.') {
                    if(isFloat) ErrorSystem.AddError_i(new MultipleDotsInNumberError(position));
                    isFloat = true;
                }
                Advance();
            }
            return new Token(isFloat ? TokenType.FLOAT : TokenType.INTEGER, position, value);
        }

        private static readonly HashSet<string> keywords = new HashSet<string>() {
            // IF
            "if",
            "else",
            // WHILE / DO_WHILE
            "while",
            "do",
            // SWITCH
            "switch",
            "case",
            "default",
            
            // TEMPORARY
            "function",
            "var",
        };

        private Token ParseIdentifier() {
            Position position = new Position(_file, _line, _column);
            string identifier = "";
            while(char.IsLetterOrDigit(c) || c == '_') {
                identifier += c;
                Advance();
            }

            if(keywords.Contains(identifier)) return new Token(TokenType.KEYWORD, position, identifier);
            return new Token(TokenType.IDENTIFIER, position, identifier);
        }

        private static readonly Dictionary<char, TokenType> singleCharTokens = new Dictionary<char, TokenType>() {
            // SEMICOLON
            {';', TokenType.SEMICOLON},
            // COLON
            {':', TokenType.COLON},
            // parenthesis
            {'(', TokenType.LPAREN},
            {')', TokenType.RPAREN},
            {'{', TokenType.LBRACE},
            {'}', TokenType.RBRACE},
            {'[', TokenType.LBRACK},
            {']', TokenType.RBRACK},
        };

        public Token NextToken() {
            SkipWhiteSpace();

            if(c == '\0') return new Token(TokenType.EOF, new Position(_file, _line, _column));

            if(char.IsDigit(c)) return ParseNumber();
            if(char.IsLetter(c) || c == '_') return ParseIdentifier();

            Position position = new Position(_file, _line, _column);

            if(singleCharTokens.ContainsKey(c)) return AdvanceWith(new Token(singleCharTokens[c], position));

            switch(c) {
                case '+':
                    Advance();
                    if(c == '=') return AdvanceWith(new Token(TokenType.PLUS_EQ, position));
                    if(c == '+') return AdvanceWith(new Token(TokenType.INCREMENT, position));
                    return new Token(TokenType.PLUS, position);
                case '-':
                    Advance();
                    if(c == '=') return AdvanceWith(new Token(TokenType.MINUS_EQ, position));
                    if(c == '-') return AdvanceWith(new Token(TokenType.DECREMENT, position));
                    return new Token(TokenType.MINUS, position);
                case '*':
                    Advance();
                    if(c == '=') return AdvanceWith(new Token(TokenType.STAR_EQ, position));
                    return new Token(TokenType.STAR, position);
                case '/':
                    Advance();
                    if(c == '=') return AdvanceWith(new Token(TokenType.SLASH_EQ, position));
                    return new Token(TokenType.SLASH, position);
                case '=':
                    Advance();
                    if(c == '=') return AdvanceWith(new Token(TokenType.EQ, position));
                    return new Token(TokenType.ASSIGN, position);
                case '!':
                    Advance();
                    if(c == '=') return AdvanceWith(new Token(TokenType.NEQ, position));
                    return new Token(TokenType.NOT, position);
                case '<':
                    Advance();
                    if(c == '=') return AdvanceWith(new Token(TokenType.LTEQ, position));
                    return new Token(TokenType.LT, position);
                case '>':
                    Advance();
                    if(c == '=') return AdvanceWith(new Token(TokenType.GTEQ, position));
                    return new Token(TokenType.GT, position);
            }

            ErrorSystem.AddError_i(new UnexpectedCharacterError(position, c));
            return null; // Unreachable
        }

        public List<Token> run() {
            List<Token> tokens = new List<Token>();

            Token token = NextToken();
            while(token.TokenType != TokenType.EOF) {
                tokens.Add(token);
                token = NextToken();
            }
            tokens.Add(token);

            return tokens;
        }

    }

}