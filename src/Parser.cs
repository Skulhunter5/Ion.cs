using System;
using System.Collections.Generic;

namespace Ion {

    class Parser {

        private List<Token> _tokens;
        private int _position = 0;
        private Token Current;

        private Dictionary<string, Function> _functions = new Dictionary<string, Function>();
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

        void Eat(TokenType tokenType, string value) { // MAYBE: return the eaten token
            if(Current.TokenType != tokenType) ErrorSystem.AddError_i(new ExpectedDifferentTokenError(tokenType, Current));
            if(Current.Value != value) ErrorSystem.AddError_i(new ExpectedDifferentValueError(value, Current));
            NextToken();
        }

        public Program run() {
            ParseDeclaration();
            return new Program(_functions, _variables);
        }

        private void ParseDeclaration() {
            while(Current.TokenType != TokenType.EOF) {
                Eat(TokenType.IDENTIFIER, "function");
                string val = Current.Value;
                Eat(TokenType.IDENTIFIER);
                Eat(TokenType.LPAREN);
                Eat(TokenType.RPAREN);
                AST body = ParseBlock();
                _functions.Add(val, new Function(val, body));
            }
        }

        private AST ParseBlock() {
            List<AST> statements = new List<AST>();

            bool isMultiline = false;
            if(Current.TokenType == TokenType.LBRACE) {
                NextToken(); // Eat: LBRACE
                isMultiline = true;
            }

            if(isMultiline) {
                while(Current.TokenType != TokenType.EOF && (!isMultiline || Current.TokenType != TokenType.RBRACE)) {
                    AST ast = ParseStatement();
                    if(ast == null) continue;
                    statements.Add(ast);
                }

                if(Current.TokenType == TokenType.EOF) ErrorSystem.AddError_i(new MissingCharacterError('}', Current.Position));

                Eat(TokenType.RBRACE);
            } else return ParseStatement();

            return new AST_Block(statements);
        }

        private AST ParseStatement() {
            if(Current.TokenType == TokenType.SEMICOLON) {
                NextToken(); // Eat: SEMICOLON
                return null;
            }

            if(Current.TokenType == TokenType.LBRACE) return ParseBlock();

            if(Current.TokenType == TokenType.KEYWORD) {
                switch(Current.Value) {
                    case "if": {
                        NextToken(); // Eat: IDENTIFIER "if"
                        Eat(TokenType.LPAREN);
                        AST_Expression condition = ParseExpression();
                        Eat(TokenType.RPAREN);
                        AST ifBlock = ParseBlock();
                        AST elseBlock = null;
                        if(Current.TokenType == TokenType.KEYWORD && Current.Value == "else") {
                            NextToken(); // Eat: IDENTIFIER "else"
                            elseBlock = ParseBlock();
                        }
                        return new AST_If(condition, ifBlock, elseBlock);
                    }
                }
            }

            if(Current.TokenType == TokenType.IDENTIFIER && Current.Value == "putc") {
                NextToken(); // Eat: IDENTIFIER "putc"
                Eat(TokenType.LPAREN);
                AST_Expression expr = ParseExpression();
                Eat(TokenType.RPAREN);
                Eat(TokenType.SEMICOLON);
                return new AST_Put_c(expr);
            }

            AST expression = ParseExpression();
            Eat(TokenType.SEMICOLON);
            return expression;
        }

        private AST_Expression ParseExpression() {
            switch(Current.TokenType) {
                case TokenType.IDENTIFIER: { // IDENTIFIER
                    Token tok = Current;
                    string val = Current.Value;
                    switch(NextToken().TokenType) {
                        case TokenType.IDENTIFIER: { // IDENTIFIER IDENTIFIER
                            string val2 = Current.Value;
                            NextToken();
                            if(Current.TokenType == TokenType.ASSIGN) { // IDENTIFIER IDENTIFIER =
                                NextToken();
                                AST_Expression valueExpression = ParseExpression();
                                Variable variable = DeclareVariable(val2);
                                return new AST_Assignment(variable, valueExpression);
                            } else {
                                DeclareVariable(val2);
                                return null;
                            }
                        }
                        case TokenType.ASSIGN: { // IDENTIFIER =
                            NextToken(); // Eat: ASSIGN
                            AST_Expression valueExpression = ParseExpression();
                            return new AST_Assignment(GetVariable(tok.Value, tok.Position), valueExpression);
                        }
                        case TokenType.LPAREN: {
                            NextToken(); // Eat: LPAREN
                            Eat(TokenType.RPAREN);
                            if(!_functions.ContainsKey(val)) ErrorSystem.AddError_i(new UnknownFunctionError(tok));
                            return new AST_FunctionCall(_functions[val]);
                        }
                        default: return new AST_Access(GetVariable(val, tok.Position));
                    }
                }
                case TokenType.INTEGER: return NextTokenWith(new AST_Integer(Current.Value));
                case TokenType.FLOAT: return NextTokenWith(new AST_Float(Current.Value));
                default:
                    Console.WriteLine("]] Unimplemented exception 1: " + Current);
                    throw new NotImplementedException();
            }
        }

        private Variable DeclareVariable(string identifier) {
            Variable variable = new Variable(identifier);
            _variables.Add(identifier, variable);
            return variable;
        }

        private Variable GetVariable(string identifier, Position position) {
            if(!_variables.ContainsKey(identifier)) ErrorSystem.AddError_i(new UnknownVariableError(identifier, position));
            return _variables[identifier];
        }

    }

}