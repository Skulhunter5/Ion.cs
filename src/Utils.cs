using System;
using System.Collections.Generic;

namespace Ion {

    class Utils {

        private static Dictionary<TokenType, int> Operators = new Dictionary<TokenType, int>() {
            {TokenType.PLUS, 1},
            {TokenType.MINUS, 1},
            {TokenType.STAR, 2},
            {TokenType.SLASH, 2},
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