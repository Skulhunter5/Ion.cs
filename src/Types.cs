using System.Collections.Generic;

using System; // REMOVE

namespace Ion {

    class DataTypes {

        private Dictionary<string, DataType> primitiveTypes = new Dictionary<string, DataType>() {
            {DataType.INT.Identifier, DataType.INT},
            {"int", DataType.INT},
        };

        public bool DataTypeExists(string identifier) {
            return primitiveTypes.ContainsKey(identifier);
        }

        public DataType GetDataType(Token identifier) {
            if(!primitiveTypes.ContainsKey(identifier.Value)) ErrorSystem.AddError_i(new UnknownDataTypeError(identifier));
            return primitiveTypes[identifier.Value];
        }

    }

    class DataType {
        public DataType(string identifier) {
            Identifier = identifier;
        }

        public string Identifier { get; }

        public override string ToString() {
            return "TYPE('" + Identifier + "')";
        }

        // STATIC - PRIMITIVES

        public static DataType INT = new DataType("int64");

    }

}