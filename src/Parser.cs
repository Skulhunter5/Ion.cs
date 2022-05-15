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
            if(_position >= _tokens.Count) return _tokens[_tokens.Count - 1];
            Current = _tokens[_position];
            return Current;
        }

        AST_Expression NextTokenWith(AST_Expression ast) {
            NextToken();
            return ast;
        }

        void Eat(TokenType tokenType) { // MAYBE: return the eaten token
            if(Current.TokenType != tokenType) ErrorSystem.AddError_i(new ExpectedDifferentTokenError(tokenType, Current));
            NextToken();
        }

        public AST_Block run() {
            return ParseBlock(true);
        }

        // TODO: add error handling for EOF in block
        private AST_Block ParseBlock(bool root = false) {
            List<AST> statements = new List<AST>();
            bool useBraces = false;
            if(Current.TokenType == TokenType.LBRACE) {
                NextToken(); // Eat: LBRACE
                useBraces = true;
            }
            if(root || useBraces) {
                while(Current.TokenType != TokenType.EOF && (!useBraces || Current.TokenType != TokenType.RBRACE)) {
                    AST ast = ParseStatement();
                    if(ast == null) continue;
                    statements.Add(ast);
                }
            } else {
                AST ast = ParseStatement();
                if(ast != null) statements.Add(ast);
            }
            if(useBraces) {
                Eat(TokenType.RBRACE);
            }
            return new AST_Block(statements);
        }

        private AST ParseStatement() {
            if(Current.TokenType == TokenType.SEMICOLON) {
                NextToken(); // Eat: SEMICOLON
                return null;
            }
            AST expression = ParseExpression();
            Eat(TokenType.SEMICOLON);
            return expression;
        }

        private AST_Expression ParseExpression() {
            switch(Current.TokenType) {
                case TokenType.IDENTIFIER: { // IDENTIFIER
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
                                Console.WriteLine("]] Unimplemented exception 1: " + Current);
                                throw new NotImplementedException();
                        }
                    }
                case TokenType.INTEGER: { // INTEGER
                        return NextTokenWith(new AST_Integer(Current.Value));
                    }
                case TokenType.FLOAT: { // FLOAT
                        return NextTokenWith(new AST_Float(Current.Value));
                    }
                default:
                    Console.WriteLine("]] Unimplemented exception 2: " + Current);
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