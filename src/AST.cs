using System;
using System.Collections.Generic;

namespace Ion {

    enum ASTType {
        BLOCK,

        // Control statements
        IF,

        // Expressions
        INTEGER, FLOAT,
        ASSIGNMENT, ACCESS,

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
            return "AST(" + ASTType;
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
            foreach(AST statement in Statements) asm += statement.GenerateAssembly();
            return asm;
        }

        public override string ToString() {
            return base.ToString() + ",children=[" + String.Join(",", Statements) + "])";
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
            asm += Condition.GenerateAssembly();
            asm += "    cmp rax, 0\n";
            asm += "    je if_" + Id + "_else\n";
            asm += IfBlock.GenerateAssembly();
            asm += "    jmp if_" + Id + "_end\n";
            asm += "    if_" + Id + "_else:\n";
            asm += ElseBlock.GenerateAssembly();
            asm += "    if_" + Id + "_end:\n";
            return asm;
        }

        public override string ToString() {
            return base.ToString() + ",condition=" + Condition + ",ifBlock=" + IfBlock + (ElseBlock != null ? ",elseBlock=" + ElseBlock : "") + ")";
        }
    }

    // Expressions

    sealed class AST_Assignment : AST_Expression {
        public AST_Assignment(Variable variable, AST_Expression value) : base(ASTType.ASSIGNMENT) {
            Variable = variable;
            Value = value;
        }

        public Variable Variable { get; }
        public AST_Expression Value { get; }

        public override string GenerateAssembly() {
            string asm = "";
            asm += Value.GenerateAssembly();
            asm += "mov [var_" + Variable.Id + "], rax\n";
            return asm;
        }

        public override string ToString() {
            return base.ToString() + ",variable=" + Variable + ",value=" + Value + ")";
        }
    }

    sealed class AST_Access : AST_Expression {
        public AST_Access(Variable variable) : base(ASTType.ACCESS) {
            Variable = variable;
        }

        public Variable Variable { get; }

        public override string GenerateAssembly() {
            return "    mov rax, [var_" + Variable.Id + "]\n";
        }

        public override string ToString() {
            return base.ToString() + ",variable=" + Variable + ")";
        }
    }

    sealed class AST_Integer : AST_Expression {
        public AST_Integer(string value) : base(ASTType.INTEGER) {
            Value = value;
        }

        public string Value { get; }

        public override string GenerateAssembly() {
            return "    mov rax, " + Value + "\n";
        }

        public override string ToString() {
            return base.ToString() + ",value=" + Value + ")";
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
            return base.ToString() + ",value=" + Value + ")";
        }
    }

    sealed class AST_Put_c : AST { // TEMPORARY
        public AST_Put_c(AST_Expression expression) : base(ASTType.PUT_c) {
            Expression = expression;
        }

        public AST_Expression Expression { get; }

        public override string GenerateAssembly() {
            string asm = "";
            asm += Expression.GenerateAssembly();
            asm += "    mov [cbuf], al\n";
            asm += "    mov rax, 1\n";
            asm += "    mov rdi, 1\n";
            asm += "    mov rsi, cbuf\n";
            asm += "    mov rdx, 1\n";
            asm += "    syscall\n";
            return asm;
        }

        public override string ToString() {
            return base.ToString() + ",expression=" + Expression  + ")";
        }
    }

    sealed class AST_FunctionCall : AST_Expression { // TEMPORARY
        public AST_FunctionCall(Function function) : base(ASTType.PUT_c) {
            Function = function;
        }

        public Function Function { get; }

        public override string GenerateAssembly() {
            string asm = "";
            asm += "    call function_" + Function.Id + "\n";
            return asm;
        }

        public override string ToString() {
            return base.ToString() + ",identifier=" + Function.Identifier  + ")";
        }
    }

}