using System;
using System.Collections.Generic;

namespace Ion {

    enum ASTType {
        BLOCK,

        // Control statements
        IF,
        WHILE, DO_WHILE,
        SWITCH,

        // Expressions
        INTEGER, FLOAT,
        ASSIGNMENT, ACCESS,

        INCREMENT, DECREMENT,

        UNARY,

        CONJUNCTION,

        // TEMPORARY
        PUT_c,
    }

    abstract class AST {

        protected static int nextId = 0;
        protected static int NextId() { return nextId++; }

        public AST(ASTType asttype) {
            Id = NextId();
            ASTType = asttype;
        }

        public int Id { get; }
        public ASTType ASTType { get; }

        public abstract string GenerateAssembly();

        public override string ToString() {
            return "AST(" + this.ASTType;
        }
    }

    abstract class AST_Expression : AST {
        public AST_Expression(ASTType asttype) : base(asttype) { }
    }

    // Block

    sealed class AST_Block : AST {
        public AST_Block(List<AST> statements) : base(ASTType.BLOCK) {
            Statements = statements;
        }

        public List<AST> Statements { get; }

        public override string GenerateAssembly() {
            string asm = "";
            // TODO: check foreach vs for performance
            foreach(AST statement in this.Statements) asm += statement.GenerateAssembly();
            return asm;
        }

        public override string ToString() {
            return base.ToString() + ",children=[" + String.Join(",", this.Statements) + "])";
        }
    }

    // Control statements

    sealed class AST_If : AST {
        public AST_If(AST_Expression condition, AST ifBlock, AST elseBlock) : base(ASTType.IF) {
            Condition = condition;
            IfBlock = ifBlock;
            ElseBlock = elseBlock;
        }

        public AST_Expression Condition { get; }
        public AST IfBlock { get; }
        public AST ElseBlock { get; }

        public override string GenerateAssembly() {
            string asm = "";
            asm += this.Condition.GenerateAssembly();
            asm += "    cmp rax, 0\n";
            asm += "    je if_" + Id + "_else\n";
            asm += this.IfBlock.GenerateAssembly();
            asm += "    jmp if_" + Id + "_end\n";
            asm += "    if_" + Id + "_else:\n";
            asm += this.ElseBlock.GenerateAssembly();
            asm += "    if_" + Id + "_end:\n";
            return asm;
        }

        public override string ToString() {
            return base.ToString() + ",condition=" + this.Condition + ",ifBlock=" + this.IfBlock + (this.ElseBlock != null ? ",elseBlock=" + this.ElseBlock : "") + ")";
        }
    }

    sealed class AST_While : AST {
        public AST_While(AST_Expression condition, AST whileBlock) : base(ASTType.WHILE) {
            Condition = condition;
            WhileBlock = whileBlock;
        }

        public AST_Expression Condition { get; }
        public AST WhileBlock { get; }

        public override string GenerateAssembly() {
            string asm = "";
            asm += "while_" + Id + "_condition:\n";
            asm += this.Condition.GenerateAssembly();
            asm += "    cmp rax, 0\n";
            asm += "    je while_" + Id + "_end\n";
            asm += "while_" + Id + "_block:\n";
            asm += this.WhileBlock.GenerateAssembly();
            asm += "    jmp while_" + Id + "_condition\n";
            asm += "while_" + Id + "_end:\n";
            return asm;
        }

        public override string ToString() {
            return base.ToString() + ",condition=" + this.Condition + ",whileBlock=" + this.WhileBlock + ")";
        }
    }

    sealed class AST_DoWhile : AST {
        public AST_DoWhile(AST_Expression condition, AST doWhileBlock) : base(ASTType.DO_WHILE) {
            Condition = condition;
            DoWhileBlock = doWhileBlock;
        }

        public AST_Expression Condition { get; }
        public AST DoWhileBlock { get; }

        public override string GenerateAssembly() {
            string asm = "";
            asm += "dowhile_" + Id + "_start:\n";
            asm += this.DoWhileBlock.GenerateAssembly();
            asm += "dowhile_" + Id + "_condition:\n";
            asm += this.Condition.GenerateAssembly();
            asm += "    cmp rax, 0\n";
            asm += "    jne dowhile_" + Id + "_start\n";
            asm += "dowhile_" + Id + "_end:\n";
            return asm;
        }

        public override string ToString() {
            return base.ToString() + ",condition=" + this.Condition + ",doWhileBlock=" + this.DoWhileBlock + ")";
        }
    }

    sealed class AST_Switch : AST {
        public AST_Switch(AST_Expression switchedExpression, List<AST_Expression> caseExpressions, List<AST> caseBlocks, AST defaultBlock) : base(ASTType.SWITCH) {
            SwitchedExpression = switchedExpression;
            CaseExpressions = caseExpressions;
            CaseBlocks = caseBlocks;
            DefaultBlock = defaultBlock;
        }

        public AST_Expression SwitchedExpression { get; }
        public List<AST_Expression> CaseExpressions { get; }
        public List<AST> CaseBlocks { get; }
        public AST DefaultBlock { get; }

        public override string GenerateAssembly() {
            string asm = "";
            asm += "switch_" + Id + "_start:\n"; // probably unnecessary
            for(int i = 0; i < this.CaseExpressions.Count; i++) {
                asm += "switch_" + Id + "_case_" + i + "_condition:\n";
                asm += this.CaseExpressions[i].GenerateAssembly();
                asm += "    mov r15, rax\n";
                asm += this.SwitchedExpression.GenerateAssembly();
                asm += "    cmp rax, r15\n";
                asm += "    jne switch_" + Id + (i < this.CaseExpressions.Count - 1 ? "_case_" + (i+1) + "_condition" : "_default") + "\n";
                asm += "switch_" + Id + "_case_" + i + "_code:\n";
                asm += this.CaseBlocks[i].GenerateAssembly();
                asm += "    jmp switch_" + Id + (i < this.CaseExpressions.Count - 1 ? "_case_" + (i+1) + "_code" : "_default") + "\n";
            }
            asm += "switch_" + Id + "_default:\n";
            if(this.DefaultBlock != null) asm += this.DefaultBlock.GenerateAssembly();
            asm += "switch_" + Id + "_end:\n";
            return asm;
        }

        public override string ToString() {
            return base.ToString() + ",caseExpressions=[" + String.Join(",", this.CaseExpressions) + "],caseBlocks=[" + String.Join(",", this.CaseBlocks) + "]" + (this.DefaultBlock != null ? ",defaultBlock=" + this.DefaultBlock : "") + ")";
        }
    }

    // Expressions

    sealed class AST_Assignment : AST_Expression {
        public AST_Assignment(Variable variable, TokenType assignmentType, AST_Expression value) : base(ASTType.ASSIGNMENT) {
            Variable = variable;
            AssignmentType = assignmentType;
            Value = value;
        }

        public Variable Variable { get; }
        public TokenType AssignmentType { get; }
        public AST_Expression Value { get; }

        public override string GenerateAssembly() {
            string asm = "";
            asm += this.Value.GenerateAssembly();
            if(AssignmentType == TokenType.ASSIGN) asm += "    mov [var_" + this.Variable.Id + "], rax\n";
            else if(AssignmentType == TokenType.PLUS_EQ) asm += "    add [var_" + this.Variable.Id + "], rax\n";
            else if(AssignmentType == TokenType.MINUS_EQ) asm += "    sub [var_" + this.Variable.Id + "], rax\n";
            else if(AssignmentType == TokenType.STAR_EQ) {
                asm += "    mov rbx, [var_" + this.Variable.Id + "]\n";
                asm += "    imul rax, rbx\n";
                asm += "    mov [var_" + this.Variable.Id + "], rax\n";
            } else if(AssignmentType == TokenType.SLASH_EQ) throw new NotImplementedException(); // TODO: implement division
            return asm;
        }

        public override string ToString() {
            return base.ToString() + ",variable=" + this.Variable + ",value=" + this.Value + ")";
        }
    }

    sealed class AST_Access : AST_Expression {
        public AST_Access(Variable variable) : base(ASTType.ACCESS) {
            Variable = variable;
        }

        public Variable Variable { get; }

        public override string GenerateAssembly() {
            return "    mov rax, [var_" + this.Variable.Id + "]\n";
        }

        public override string ToString() {
            return base.ToString() + ",variable=" + this.Variable + ")";
        }
    }

    sealed class AST_Integer : AST_Expression {
        public AST_Integer(string value) : base(ASTType.INTEGER) {
            Value = value;
        }

        public string Value { get; }

        public override string GenerateAssembly() {
            return "    mov rax, " + this.Value + "\n";
        }

        public override string ToString() {
            return base.ToString() + ",value=" + this.Value + ")";
        }
    }

    sealed class AST_Float : AST_Expression {
        public AST_Float(string value) : base(ASTType.FLOAT) {
            Value = value;
        }

        public string Value { get; }

        public override string GenerateAssembly() {
            throw new NotImplementedException();
        }

        public override string ToString() {
            return base.ToString() + ",value=" + this.Value + ")";
        }
    }

    sealed class AST_Put_c : AST { // TEMPORARY
        public AST_Put_c(AST_Expression expression) : base(ASTType.PUT_c) {
            Expression = expression;
        }

        public AST_Expression Expression { get; }

        public override string GenerateAssembly() {
            string asm = "";
            asm += this.Expression.GenerateAssembly();
            asm += "    mov [cbuf], al\n";
            asm += "    mov rax, 1\n";
            asm += "    mov rdi, 1\n";
            asm += "    mov rsi, cbuf\n";
            asm += "    mov rdx, 1\n";
            asm += "    syscall\n";
            return asm;
        }

        public override string ToString() {
            return base.ToString() + ",expression=" + this.Expression + ")";
        }
    }

    sealed class AST_FunctionCall : AST_Expression {
        public AST_FunctionCall(Function function) : base(ASTType.PUT_c) {
            Function = function;
        }

        public Function Function { get; }

        public override string GenerateAssembly() {
            string asm = "";
            asm += "    call function_" + this.Function.Id + "\n";
            return asm;
        }

        public override string ToString() {
            return base.ToString() + ",identifier=" + this.Function.Identifier + ")";
        }
    }

    sealed class AST_Unary : AST_Expression {
        public AST_Unary(TokenType _operator, AST_Expression expression) : base(ASTType.UNARY) {
            Operator = _operator;
            Expression = expression;
        }

        public TokenType Operator { get; }
        public AST_Expression Expression { get; }

        public override string GenerateAssembly() {
            string asm = "";
            asm += Expression.GenerateAssembly();
            if(this.Operator == TokenType.MINUS) asm += "    neg rax\n";
            return asm;
        }

        public override string ToString() {
            return base.ToString() + ",expression=" + this.Expression + ",operator=" + this.Operator + ")";
        }
    }

    sealed class AST_Conjunction : AST_Expression {
        public AST_Conjunction(TokenType _operator, AST_Expression a, AST_Expression b) : base(ASTType.CONJUNCTION) {
            Operator = _operator;
            A = a;
            B = b;
        }

        public TokenType Operator { get; }
        public AST_Expression A { get; }
        public AST_Expression B { get; }

        public override string GenerateAssembly() {
            string asm = "";
            asm += this.A.GenerateAssembly();
            asm += "    push rax\n";
            asm += this.B.GenerateAssembly();
            switch(this.Operator) {
                // CALCULATION
                case TokenType.PLUS: {
                    asm += "    pop rbx\n";
                    asm += "    add rax, rbx\n";
                    break;
                }
                case TokenType.MINUS: {
                    asm += "    mov rbx, rax\n";
                    asm += "    pop rax\n";
                    asm += "    sub rax, rbx\n";
                    break;
                }
                case TokenType.STAR: {
                    asm += "    pop rbx\n";
                    asm += "    imul rax, rbx\n";
                    break;
                }
                case TokenType.SLASH: {
                    throw new NotImplementedException(); // TODO: implement division
                }
                default: {
                    // COMPARISON
                    if(Utils.ComparisonOperators.ContainsKey(this.Operator)) {
                        asm += "    pop rbx\n";
                        asm += "    cmp rbx, rax\n";
                        asm += "    mov rax, 0\n";
                        asm += "    set" + Utils.ComparisonOperators[this.Operator] + " al\n";
                        break;
                    }
                    throw new NotImplementedException();
                }
            }
            return asm;
        }

        public override string ToString() {
            return base.ToString() + ",operator=" + this.Operator + ",a=" + this.A + ",b=" + this.B + ")";
        }
    }

    sealed class AST_Increment : AST_Expression {
        public AST_Increment(Variable variable, IncDecType incDecType) : base(ASTType.INCREMENT) {
            Variable = variable;
            IncDecType = incDecType;
        }

        public Variable Variable { get; }
        public IncDecType IncDecType { get; }

        public override string GenerateAssembly() {
            string asm = "";
            if(this.IncDecType == IncDecType.AFTER) asm += "    mov rax, [var_" + this.Variable.Id + "]\n";
            asm += "    inc QWORD [var_" + this.Variable.Id + "]\n";
            if(this.IncDecType == IncDecType.BEFORE) asm += "    mov rax, [var_" + this.Variable.Id + "]\n";
            return asm;
        }

        public override string ToString() {
            return base.ToString() + ",variable=" + this.Variable + ",incDecType=" + this.IncDecType + ")";
        }
    }

    sealed class AST_Decrement : AST_Expression {
        public AST_Decrement(Variable variable, IncDecType incDecType) : base(ASTType.DECREMENT) {
            Variable = variable;
            IncDecType = incDecType;
        }

        public Variable Variable { get; }
        public IncDecType IncDecType { get; }

        public override string GenerateAssembly() {
            string asm = "";
            if(this.IncDecType == IncDecType.AFTER) asm += "    mov rax, [var_" + this.Variable.Id + "]\n";
            asm += "    dec QWORD [var_" + this.Variable.Id + "]\n";
            if(this.IncDecType == IncDecType.BEFORE) asm += "    mov rax, [var_" + this.Variable.Id + "]\n";
            return asm;
        }

        public override string ToString() {
            return base.ToString() + ",variable=" + this.Variable + ",incDecType=" + this.IncDecType + ")";
        }
    }

}