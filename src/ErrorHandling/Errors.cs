using System;

namespace Ion {

    abstract class Error {}

    abstract class GeneralError : Error {
        public override string ToString() {
            return "[General] ERROR: ";
        }
    }

    abstract class LexerError : Error {
        public override string ToString() {
            return "[Lexer] ERROR: ";
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
            return base.ToString() + "Invalid character: '" + C + "' (" + ((int) C) + ") at " + Position;
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

}