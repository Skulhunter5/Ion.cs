using System;
using System.Collections.Generic;

namespace Ion {

    enum ASTType {
        BLOCK,

        // Control statements
        IF,

        // Expressions
        INTEGER, FLOAT,
        ASSIGNMENT,
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
        public AST_If(AST_Expression condition, AST_Block ifBlock, AST_Block elseBlock) : base(ASTType.IF) {
            Condition = condition;
            IfBlock = ifBlock;
            ElseBlock = elseBlock;
        }

        public AST_Expression Condition { get; }
        public AST_Block IfBlock { get; }
        public AST_Block ElseBlock { get; }

        public override string GenerateAssembly() {
            string asm = "";
            asm += Condition.GenerateAssembly();
            asm += "cmp rax, 0";
            asm += "je if_" + Id + "_else";
            asm += IfBlock.GenerateAssembly();
            asm += "jmp if_" + Id + "_end";
            asm += "if_" + Id + "_else:";
            asm += ElseBlock.GenerateAssembly();
            asm += "if_" + Id + "_end:";
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
            throw new NotImplementedException();
        }

        public override string ToString() {
            return base.ToString() + ",variable=" + Variable + ",value=" + Value + ")";
        }
    }

    sealed class AST_Integer : AST_Expression {
        public AST_Integer(string value) : base(ASTType.INTEGER) {
            Value = value;
        }

        public string Value { get; }

        public override string GenerateAssembly() {
            return "mov rax, " + Value;
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

}