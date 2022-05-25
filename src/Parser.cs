using System;
using System.Collections.Generic;

namespace Ion {

    class Parser {

        private List<Token> _tokens;
        private int _position = 0;
        private Token Current;

        private Dictionary<string, Function> _functions = new Dictionary<string, Function>();
        private Dictionary<string, Variable> _variables = new Dictionary<string, Variable>();
        private DataTypes _types = new DataTypes();

        public Parser(List<Token> tokens) {
            _tokens = tokens;
            Current = _tokens[_position];
        }

        private Token Peek(int offset) {
            if(_position + offset >= _tokens.Count) return _tokens[_tokens.Count - 1];
            return _tokens[_position + offset];
        }

        private void Back() {
            if(_position > 0) _position--;
            Current = _tokens[_position];
        }

        private Token Eat() {
            _position++;
            if(_position >= _tokens.Count) return _tokens[_tokens.Count - 1];
            Current = _tokens[_position];
            return Current;
        }

        private AST_Expression EatWith(AST_Expression ast) {
            Eat();
            return ast;
        }

        private void Eat(TokenType tokenType) { // MAYBE: return the eaten token
            if(Current.TokenType != tokenType) ErrorSystem.AddError_i(new ExpectedDifferentTokenError(tokenType, Current));
            Eat();
        }

        private void Eat(TokenType tokenType, string value) { // MAYBE: return the eaten token
            if(Current.TokenType != tokenType) ErrorSystem.AddError_i(new ExpectedDifferentTokenError(tokenType, Current));
            if(Current.Value != value) ErrorSystem.AddError_i(new ExpectedDifferentValueError(value, Current));
            Eat();
        }

        private DataType ParseDataType() {
            Token value = Current;
            Eat(TokenType.IDENTIFIER);
            DataType dataType = _types.GetDataType(value);
            uint pointerN = 0;
            while(Current.TokenType == TokenType.STAR) pointerN++;
            if(pointerN > 0) dataType = new DataType(dataType.PointerValue, pointerN, dataType.DT_Class, dataType.DT_Struct);
            return dataType;
        }

        public Program run() {
            ParseDeclaration();
            return new Program(_functions, _variables);
        }

        private void ParseDeclaration() {
            while(Current.TokenType != TokenType.EOF) {
                if(Current.TokenType == TokenType.KEYWORD) {
                    if(Current.Value == "struct") {
                        Eat(); // KEYWORD "struct"
                        Token name = Current;
                        Eat(TokenType.IDENTIFIER);
                        DT_Struct dT_Struct = ParseStruct(name);
                        _types.Add(dT_Struct);
                    }
                } else if(Current.TokenType == TokenType.IDENTIFIER) {
                    DataType dataType = ParseDataType();
                    Token name = Current;
                    Eat(TokenType.IDENTIFIER);
                    if(Current.TokenType == TokenType.LPAREN) {
                        Eat(TokenType.LPAREN);
                        Eat(TokenType.RPAREN);
                        AST body = ParseBlock();
                        _functions.Add(name.Value, new Function(name.Value, body));
                    } else if(Current.TokenType == TokenType.SEMICOLON) {
                        Token token = Current;
                        Eat(); // IDENTIFIER
                        Eat(TokenType.IDENTIFIER);
                        DeclareVariable(name, dataType);
                        Eat(TokenType.SEMICOLON);
                    } else throw new NotImplementedException();
                } else throw new NotImplementedException();
            }
        }

        private DT_Struct ParseStruct(Token name) {
            Eat(TokenType.LBRACE);

            Dictionary<string, DataType> fields = new Dictionary<string, DataType>();
            while(Current.TokenType != TokenType.RBRACE) {
                DataType dataType = ParseDataType();
                Token fieldName = Current;
                Eat(TokenType.IDENTIFIER);
                Eat(TokenType.SEMICOLON);
                fields.Add(fieldName.Value, dataType);
            }

            Eat(TokenType.RBRACE);

            return new DT_Struct(name.Value, fields);
        }

        private AST ParseBlock() {
            List<AST> statements = new List<AST>();

            bool isMultiline = false;
            if(Current.TokenType == TokenType.LBRACE) {
                Eat(); // LBRACE
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
                Eat(); // SEMICOLON
                return null;
            }

            if(Current.TokenType == TokenType.LBRACE) return ParseBlock();

            if(Current.TokenType == TokenType.KEYWORD) {
                switch(Current.Value) {
                    case "if": {
                        Eat(); // KEYWORD "if"
                        Eat(TokenType.LPAREN);
                        AST_Expression condition = ParseExpression();
                        Eat(TokenType.RPAREN);
                        AST ifBlock = ParseBlock();
                        AST elseBlock = null;
                        if(Current.TokenType == TokenType.KEYWORD && Current.Value == "else") {
                            Eat(); // IDENTIFIER "else"
                            elseBlock = ParseBlock();
                        }
                        return new AST_If(condition, ifBlock, elseBlock);
                    }
                    case "while": {
                        Eat(); // KEYWORD "while"
                        Eat(TokenType.LPAREN);
                        AST_Expression condition = ParseExpression();
                        Eat(TokenType.RPAREN);
                        AST whileBlock = ParseBlock();
                        return new AST_While(condition, whileBlock);
                    }
                    case "do": {
                        Eat(); // KEYWORD "do"
                        AST doWhileBlock = ParseBlock();
                        Eat(TokenType.KEYWORD, "while");
                        Eat(TokenType.LPAREN);
                        AST_Expression condition = ParseExpression();
                        Eat(TokenType.RPAREN);
                        Eat(TokenType.SEMICOLON);
                        return new AST_DoWhile(condition, doWhileBlock);
                    }
                    case "switch": {
                        List<AST_Expression> caseExpressions = new List<AST_Expression>();
                        List<AST> caseBlocks = new List<AST>();
                        AST defaultBlock = null;

                        Eat(); // KEYWORD switch

                        Eat(TokenType.LPAREN);
                        AST_Expression switchedExpression = ParseExpression();
                        Eat(TokenType.RPAREN);

                        Eat(TokenType.LBRACE);
                        while(Current.TokenType == TokenType.KEYWORD) {
                            if(Current.Value == "case") {
                                Eat(); // KEYWORD "case"
                                caseExpressions.Add(ParseExpression());
                                Eat(TokenType.COLON);
                                caseBlocks.Add(ParseBlock());
                            } else if(Current.Value == "default") {
                                Eat(); // KEYWORD "default"
                                Eat(TokenType.COLON);
                                defaultBlock = ParseBlock();
                            } else ErrorSystem.AddError_i(new UnexpectedTokenError(Current, "'case' or 'default'"));
                        }
                        Eat(TokenType.RBRACE);
                        return new AST_Switch(switchedExpression, caseExpressions, caseBlocks, defaultBlock);
                    }
                }
            }

            if(Current.TokenType == TokenType.IDENTIFIER && Current.Value == "putc") {
                Eat(); // IDENTIFIER "putc"
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
            List<AST_Expression> particles = new List<AST_Expression>();
            List<TokenType> operators = new List<TokenType>();

            particles.Add(ParseExpressionParticle());
            while(Utils.IsOperator(Current.TokenType)) {
                operators.Add(Current.TokenType);
                Eat(); // [operator]
                particles.Add(ParseExpressionParticle());
            }

            Reduce(particles, operators, 0, 0);

            return particles[0];
        }

        private void Reduce(List<AST_Expression> particles, List<TokenType> operators, int start, int level) {
            if(operators.Count == 0) return;
            if(level == 0) level = Utils.GetOperationLevel(operators[0]);
            while(particles.Count > start + 1) {
                if(Utils.GetOperationLevel(operators[start]) < level) level = Utils.GetOperationLevel(operators[0]);
                if(operators.Count > start + 1 && Utils.GetOperationLevel(operators[start + 1]) > level) {
                    Reduce(particles, operators, start + 1, Utils.GetOperationLevel(operators[start + 1]));
                    continue;
                }
                particles[start] = new AST_Conjunction(operators[start], particles[start], particles[start + 1]);
                particles.RemoveAt(start + 1);
                operators.RemoveAt(start);
            }
        }

        private AST_Expression ParseExpressionParticle() {
            if(Current.TokenType == TokenType.MINUS) {
                TokenType _operator = Current.TokenType;
                Eat(); // [unary operator]
                return new AST_Unary(_operator, ParseExpressionParticle());
            }

            if(Current.TokenType == TokenType.LPAREN) {
                Eat(); // LPAREN
                AST_Expression expression = ParseExpression();
                Eat(TokenType.RPAREN);
                return expression;
            }

            switch(Current.TokenType) {
                case TokenType.IDENTIFIER: {
                    Token token = Current;
                    Eat(); // IDENTIFIER
                    switch(Current.TokenType) {
                        case TokenType.LPAREN: {
                            Eat(); // LPAREN
                            Eat(TokenType.RPAREN);
                            if(!_functions.ContainsKey(token.Value)) ErrorSystem.AddError_i(new UnknownFunctionError(token));
                            return new AST_FunctionCall(_functions[token.Value]);
                        }
                        case TokenType.INCREMENT: return EatWith(new AST_Increment(GetVariable(token), IncDecType.AFTER)); // Eat: INCREMENT
                        case TokenType.DECREMENT: return EatWith(new AST_Decrement(GetVariable(token), IncDecType.AFTER)); // Eat: INCREMENT
                        default: {
                            if(Utils.AssignmentTokens.Contains(Current.TokenType)) {
                                TokenType assigmentType = Current.TokenType;
                                Eat(); // [assignment token]
                                AST_Expression valueExpression = ParseExpression();
                                return new AST_Assignment(GetVariable(token), assigmentType, valueExpression);
                            }

                            if(Current.TokenType == TokenType.IDENTIFIER || Current.TokenType == TokenType.STAR) {
                                Back();
                                DataType dataType = ParseDataType();
                                Token name = Current;
                                Eat(TokenType.IDENTIFIER);

                                AST_Assignment assignment = null;
                                Variable variable = DeclareVariable(name, _types.GetDataType(token));
                                if(Current.TokenType == TokenType.ASSIGN) {
                                    Eat(); // ASSIGN
                                    AST_Expression valueExpression = ParseExpression();
                                    assignment = new AST_Assignment(variable, TokenType.ASSIGN, valueExpression);
                                }
                                return assignment;
                            }

                            return new AST_Access(GetVariable(token));
                        }
                    }
                }
                case TokenType.INTEGER: return EatWith(new AST_Integer(Current.Value));
                case TokenType.FLOAT: return EatWith(new AST_Float(Current.Value));
                case TokenType.INCREMENT: {
                    Eat(); // INCREMENT
                    Token token = Current;
                    Eat(TokenType.IDENTIFIER);
                    return new AST_Increment(GetVariable(token), IncDecType.BEFORE);
                }
                case TokenType.DECREMENT: {
                    Eat(); // INCREMENT
                    Token token = Current;
                    Eat(TokenType.IDENTIFIER);
                    return new AST_Decrement(GetVariable(token), IncDecType.BEFORE);
                }
                default:
                    Console.WriteLine("]] Unimplemented exception 1: " + Current);
                    throw new NotImplementedException();
            }
        }

        private Variable DeclareVariable(Token identifier, DataType dataType) {
            if(_variables.ContainsKey(identifier.Value)) ErrorSystem.AddError_i(new VariableRedeclarationError(identifier));

            Variable variable = new Variable(identifier.Value, dataType);
            _variables.Add(identifier.Value, variable);
            return variable;
        }

        private Variable GetVariable(Token identifier) {
            if(!_variables.ContainsKey(identifier.Value)) ErrorSystem.AddError_i(new UnknownVariableError(identifier.Value, identifier.Position));
            return _variables[identifier.Value];
        }

    }

}