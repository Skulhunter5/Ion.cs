using System;
using System.Collections.Generic;

namespace Ion {

    enum ASTType {
        BLOCK
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

    sealed class AST_Block : AST {
        public AST_Block(List<AST> statements) : base(ASTType.BLOCK) {
            Statements = statements;
        }

        public List<AST> Statements { get; }

        public override string ToString() {
            return base.ToString() + ", children=[" + String.Join(",", Statements) + "])";
        }
    }

}