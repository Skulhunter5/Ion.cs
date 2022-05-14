using System;
using System.Collections.Generic;

namespace Ion {

    enum ASTType {
        BLOCK,

        // Expressions
        ASSIGNMENT,
    }

    abstract class AST {
        public AST(ASTType asttype) {
            ASTType = asttype;
        }

        public ASTType ASTType { get; }

        public override string ToString() {
            return "AST(" + ASTType;
        }
    }

    abstract class AST_Expression : AST {
        public AST_Expression(ASTType asttype) : base(asttype) {}
    }

    sealed class AST_Block : AST {
        public AST_Block(List<AST> statements) : base(ASTType.BLOCK) {
            Statements = statements;
        }

        public List<AST> Statements { get; }

        public override string ToString() {
            return base.ToString() + ",children=[" + String.Join(",", Statements) + "])";
        }
    }

    sealed class AST_Assignment : AST_Expression {
        public AST_Assignment(Variable variable, AST_Expression value) : base(ASTType.ASSIGNMENT) {
            Variable = variable;
            Value = value;
        }

        public Variable Variable { get; }
        public AST_Expression Value { get; }

        public override string ToString() {
            return base.ToString() + ",variable=" + Variable + ",value=" + Value + ")";
        }
    }

}