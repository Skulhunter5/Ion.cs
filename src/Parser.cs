using System.Collections.Generic;

namespace Ion {

    class Parser {

        private List<Token> _tokens;
        private int _position = 0;
        private Token Current;

        public Parser(List<Token> tokens) {
            _tokens = tokens;
            Current = _tokens[_position];
        }

        Token NextToken() {
            _position++;
            if(_position >= _tokens.Count) return _tokens[_tokens.Count-1];
            return _tokens[_position];
        }

        void Eat(TokenType tokenType) { // MAYBE: return the eaten token
            if(Current.TokenType != tokenType) return; // CORRECT: throw error
            NextToken();
        }

        public AST_Block run() {
            return ParseBlock();
        }

        private AST_Block ParseBlock() {
            List<AST> statements = new List<AST>();
            while(Current.TokenType != TokenType.EOF) statements.Add(ParseStatement());
            return new AST_Block(statements);
        }

        private AST ParseStatement() {
            AST expression = ParseExpression();
            Eat(TokenType.SEMICOLON);
            return expression;
        }

        private AST ParseExpression() {
            switch(Current.TokenType) {
                case TokenType.IDENTIFIER:
                    string value = Current.Value;
                    switch(NextToken().TokenType) {
                        case TokenType.IDENTIFIER: // Declaration
                            string value2 = Current.Value;
                            NextToken();
                            if(Current.TokenType == TokenType.ASSIGN) {
                                NextToken();
                                AST valueExpression = ParseExpression();
                                // Add variable declaration
                                return new AST_Assignment();
                            } else {
                                // Add variable declaration
                            }
                            break;
                        case TokenType.ASSIGN: // Definition
                            break;
                    }
                    break;
                default:
                    return null; // CORRECT: throw error
            }
        }

    }

}