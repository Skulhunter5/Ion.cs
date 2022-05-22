namespace Ion {

    abstract class Error { }

    abstract class LexerError : Error {
        public override string ToString() {
            return "[Lexer] ERROR: ";
        }
    }

    abstract class ParserError : Error {
        public override string ToString() {
            return "[Parser] ERROR: ";
        }
    }

    // Lexer errors

    sealed class UnexpectedCharacterError : LexerError {
        public UnexpectedCharacterError(Position position, char c) {
            Position = position;
            C = c;
        }

        public Position Position { get; }
        public char C { get; }

        public override string ToString() {
            return base.ToString() + "Invalid character: '" + C + "' (" + ((int)C) + ") at " + Position;
        }
    }

    sealed class MultipleDotsInNumberError : LexerError {
        public MultipleDotsInNumberError(Position position) {
            Position = position;
        }

        public Position Position { get; }

        public override string ToString() {
            return base.ToString() + "Multiple dots in number: beginning at " + Position;
        }
    }

    sealed class ExpectedDifferentTokenError : ParserError {
        public ExpectedDifferentTokenError(TokenType expected, Token got) {
            Expected = expected;
            Got = got;
        }

        public TokenType Expected { get; }
        public Token Got { get; }

        public override string ToString() {
            return base.ToString() + "Unexpected token " + Got + ", expected token of type " + Expected;
        }
    }

    sealed class ExpectedDifferentValueError : ParserError {
        public ExpectedDifferentValueError(string expected, Token got) {
            Expected = expected;
            Got = got;
        }

        public string Expected { get; }
        public Token Got { get; }

        public override string ToString() {
            return base.ToString() + "Unexpected token " + Got + ", expected '" + Expected + "'";
        }
    }

    sealed class MissingCharacterError : ParserError {
        public MissingCharacterError(char c, Position position) {
            C = c;
            Position = position;
        }

        public char C { get; }
        public Position Position { get; }

        public override string ToString() {
            return base.ToString() + "Missing character: '" + C + "' at " + Position;
        }
    }

    sealed class MissingEntryPointError : ParserError {
        public MissingEntryPointError() { }

        public override string ToString() {
            return base.ToString() + "Missing entry point: main";
        }
    }

    sealed class UnknownFunctionError : ParserError {
        public UnknownFunctionError(Token token) {
            Token = token;
        }

        public Token Token { get; }

        public override string ToString() {
            return base.ToString() + "Unknown function: '" + Token.Value + "' at " + Token.Position;
        }
    }

    sealed class UnknownVariableError : ParserError {
        public UnknownVariableError(string identifier, Position position) {
            Identifier = identifier;
            Position = position;
        }

        public string Identifier { get; }
        public Position Position { get; }

        public override string ToString() {
            return base.ToString() + "Unknown variable: '" + Identifier + "' at " + Position;
        }
    }

    sealed class UnexpectedTokenError : ParserError {
        public UnexpectedTokenError(Token got, string expected) {
            Got = got;
            Expected = expected;
        }

        public Token Got { get; }
        public string Expected { get; }

        public override string ToString() {
            return base.ToString() + "Unexpected token: " + Got + ", expected: " + Expected;
        }
    }

    sealed class UnknownDataTypeError : ParserError {
        public UnknownDataTypeError(Token got) {
            Got = got;
        }

        public Token Got { get; }

        public override string ToString() {
            return base.ToString() + "Unknown DataType: '" + Got.Value + "' at " + Got.Position;
        }
    }

    sealed class VariableRedeclarationError : ParserError {
        public VariableRedeclarationError(Token name) {
            Name = name;
        }

        public Token Name { get; }

        public override string ToString() {
            return base.ToString() + "Trying to redeclare variable: '" + Name.Value + "' at " + Name.Position;
        }
    }

    sealed class DataTypeNotValidInGivenContextError : ParserError {
        public DataTypeNotValidInGivenContextError(Token dataType) {
            DataType = dataType;
        }

        public Token DataType { get; }

        public override string ToString() {
            return base.ToString() + "'" + DataType.Value + "' is a type, which is not valid in the given context (" + DataType.Position + ")";
        }
    }

}