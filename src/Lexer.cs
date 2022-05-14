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

        private Token ParseIdentifier() {
            Position position = new Position(_file, _line, _column);
            string identifier = "";
            while(char.IsLetterOrDigit(c) || c == '_') {
                identifier += c;
                Advance();
            }
            return new Token(TokenType.IDENTIFIER, position, identifier);
        }

        public Token NextToken() {
            SkipWhiteSpace();

            if(c == '\0') return new Token(TokenType.EOF, new Position(_file, _line, _column));

            if(char.IsDigit(c)) return ParseNumber();
            if(char.IsLetter(c) || c == '_') return ParseIdentifier();

            Position position = new Position(_file, _line, _column);

            switch(c) {
                case ';':
                    return AdvanceWith(new Token(TokenType.SEMICOLON, position));
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