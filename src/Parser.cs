using System;
using System.Collections.Generic;

namespace Ion {

    class Parser {

        private List<Token> _tokens;
        private int _position = 0;
        private Token Current;

        private Dictionary<string, Variable> _variables = new Dictionary<string, Variable>();

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
            while(Current.TokenType != TokenType.EOF) {
                AST ast = ParseStatement();
                if(ast == null) continue;
                statements.Add(ast);
            }
            return new AST_Block(statements);
        }

        private AST ParseStatement() {
            AST expression = ParseExpression();
            Eat(TokenType.SEMICOLON);
            return expression;
        }

        private AST_Expression ParseExpression() {
            switch(Current.TokenType) {
                case TokenType.IDENTIFIER: // IDENTIFIER
                    string value = Current.Value;
                    switch(NextToken().TokenType) {
                        case TokenType.IDENTIFIER: // IDENTIFIER IDENTIFIER
                            string value2 = Current.Value;
                            NextToken();
                            if(Current.TokenType == TokenType.ASSIGN) { // IDENTIFIER IDENTIFIER =
                                NextToken();
                                AST_Expression valueExpression = ParseExpression();
                                Variable variable = DeclareVariable(value2);
                                return new AST_Assignment(variable, valueExpression);
                            } else {
                                DeclareVariable(value2);
                                return null;
                            }
                        case TokenType.ASSIGN: { // IDENTIFIER =
                            NextToken(); // Eat: ASSIGN
                            AST_Expression valueExpression = ParseExpression();
                            return new AST_Assignment(GetVariable(value), valueExpression);
                        }
                        default:
                            throw new NotImplementedException();
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        private Variable DeclareVariable(string identifier) {
            Variable variable = new Variable(identifier);
            _variables.Add(identifier, variable);
            return variable;
        }

        private Variable GetVariable(string identifier) {
            if(!_variables.ContainsKey(identifier)) throw new NotImplementedException();
            return _variables[identifier];
        }

    }

}