using System;
using System.Collections.Generic;

namespace Ion {

    enum IncDecType {
        BEFORE,
        AFTER,
    }

    class Utils {

        public static Dictionary<TokenType, string> ComparisonOperators = new Dictionary<TokenType, string>() {
            {TokenType.EQ, "e"},
            {TokenType.NEQ, "ne"},
            {TokenType.LT, "l"},
            {TokenType.GT, "g"},
            {TokenType.LTEQ, "le"},
            {TokenType.GTEQ, "ge"},
        };

        private static Dictionary<TokenType, int> Operators = new Dictionary<TokenType, int>() {
            {TokenType.EQ, 1},
            {TokenType.NEQ, 1},
            {TokenType.LT, 1},
            {TokenType.GT, 1},
            {TokenType.LTEQ, 1},
            {TokenType.GTEQ, 1},
            {TokenType.PLUS, 2},
            {TokenType.MINUS, 2},
            {TokenType.STAR, 3},
            {TokenType.SLASH, 3},
        };

        public static bool IsOperator(TokenType tokeType) {
            return Operators.ContainsKey(tokeType);
        }

        public static int GetOperationLevel(TokenType _operator) {
            if(!Operators.ContainsKey(_operator)) throw new NotImplementedException();
            return Operators[_operator];
        }

    }

}