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
                Eat(TokenType.KEYWORD, "function");
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
                        NextToken(); // Eat: KEYWORD "if"
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
                    case "while": {
                        NextToken(); // Eat: KEYWORD "while"
                        Eat(TokenType.LPAREN);
                        AST_Expression condition = ParseExpression();
                        Eat(TokenType.RPAREN);
                        AST whileBlock = ParseBlock();
                        return new AST_While(condition, whileBlock);
                    }
                    case "do": {
                        NextToken(); // Eat: KEYWORD "do"
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

                        NextToken(); // Eat: KEYWORD switch

                        Eat(TokenType.LPAREN);
                        AST_Expression switchedExpression = ParseExpression();
                        Eat(TokenType.RPAREN);

                        Eat(TokenType.LBRACE);
                        while(Current.TokenType == TokenType.KEYWORD) {
                            if(Current.Value == "case") {
                                NextToken(); // Eat: KEYWORD "case"
                                caseExpressions.Add(ParseExpression());
                                Eat(TokenType.COLON);
                                caseBlocks.Add(ParseBlock());
                            } else if(Current.Value == "default") {
                                NextToken(); // Eat: KEYWORD "default"
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
            List<AST_Expression> particles = new List<AST_Expression>();
            List<TokenType> operators = new List<TokenType>();

            particles.Add(ParseExpressionParticle());
            while(Utils.IsOperator(Current.TokenType)) {
                operators.Add(Current.TokenType);
                NextToken(); // Eat: [mathematical operator]
                particles.Add(ParseExpressionParticle());
            }

            // TODO: connect the particles with the operators
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
                NextToken(); // Eat: [unary operator]
                return new AST_Unary(_operator, ParseExpressionParticle());
            }

            if(Current.TokenType == TokenType.LPAREN) {
                NextToken(); // Eat: LPAREN
                AST_Expression expression = ParseExpression();
                Eat(TokenType.RPAREN);
                return expression;
            }

            if(Current.TokenType == TokenType.KEYWORD && Current.Value == "var") { // TEMPORARY
                NextToken(); // Eat: KEYWORD "var"
                string varName = Current.Value;
                Eat(TokenType.IDENTIFIER);
                AST_Assignment assignment = null;
                Variable variable = DeclareVariable(varName);
                if(Current.TokenType == TokenType.ASSIGN) { // IDENTIFIER IDENTIFIER =
                    NextToken(); // Eat: ASSIGN
                    AST_Expression valueExpression = ParseExpression();
                    assignment = new AST_Assignment(variable, valueExpression);
                }
                return assignment;
            }

            switch(Current.TokenType) {
                case TokenType.IDENTIFIER: { // IDENTIFIER
                    Token tok = Current;
                    string val = Current.Value;
                    switch(NextToken().TokenType) {
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
                        case TokenType.INCREMENT: return NextTokenWith(new AST_Increment(GetVariable(tok.Value, tok.Position), IncDecType.AFTER));// Eat: INCREMENT
                        case TokenType.DECREMENT: return NextTokenWith(new AST_Decrement(GetVariable(tok.Value, tok.Position), IncDecType.AFTER));// Eat: INCREMENT
                        default: return new AST_Access(GetVariable(val, tok.Position));
                    }
                }
                case TokenType.INTEGER: return NextTokenWith(new AST_Integer(Current.Value));
                case TokenType.FLOAT: return NextTokenWith(new AST_Float(Current.Value));
                case TokenType.INCREMENT: {
                    NextToken(); // Eat: INCREMENT
                    Token tok = Current;
                    Eat(TokenType.IDENTIFIER);
                    return new AST_Increment(GetVariable(tok.Value, tok.Position), IncDecType.BEFORE);
                }
                case TokenType.DECREMENT: {
                    NextToken(); // Eat: INCREMENT
                    Token tok = Current;
                    Eat(TokenType.IDENTIFIER);
                    return new AST_Decrement(GetVariable(tok.Value, tok.Position), IncDecType.BEFORE);
                }
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