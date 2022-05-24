using System.Collections.Generic;

using System; // REMOVE

namespace Ion {

    class DataTypes {

        private Dictionary<string, DataType> primitiveTypes = new Dictionary<string, DataType>() {
            {"void", DataType.I_VOID},
            {"bool", DataType.I_BOOLEAN},
            {"uint64", DataType.I_UINT64},
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

        public DataType(uint value) {
            Value = value;
        }
        public DataType(uint pointerValue, uint pointerN) {
            Value = DataType.POINTER;
            PointerValue = pointerValue;
            PointerN = pointerN;
        }
        public DataType(uint pointerValue, uint pointerN, DT_Class dT_Class) {
            Value = DataType.POINTER;
            PointerValue = pointerValue;
            PointerN = pointerN;
            DT_Class = dT_Class;
        }
        public DataType(uint value, DT_Class dT_Class) {
            Value = value;
            PointerValue = DataType.CLASS;
            DT_Class = dT_Class;
        }

        public uint Value { get; }
        public uint PointerValue { get; }
        public uint PointerN { get; }
        public DT_Class DT_Class { get; }

        // STATIC

        public static readonly uint VOID      = 0x000;
        public static readonly uint BOOLEAN   = 0x001;
        public static readonly uint UINT64    = 0x010;
        public static readonly uint POINTER   = 0x011;
        public static readonly uint CLASS     = 0x100;

        public static readonly DataType I_VOID      = new DataType(DataType.VOID);
        public static readonly DataType I_BOOLEAN   = new DataType(DataType.BOOLEAN);
        public static readonly DataType I_UINT64    = new DataType(DataType.UINT64);
        public static readonly DataType I_POINTER   = new DataType(DataType.POINTER, DataType.VOID);

        public static readonly Dictionary<string, DataType> typeDict = new Dictionary<string, DataType>() {
            {"bool",    I_BOOLEAN},
            {"uint64",  I_UINT64},
            {"pointer",     I_POINTER},
        };
        public static readonly Dictionary<uint, string> stringDict = new Dictionary<uint, string>() {
            {DataType.VOID,     "void"},
            {DataType.BOOLEAN,  "bool"},
            {DataType.UINT64,   "uint64"},
            {DataType.POINTER,  "pointer"},
        };

    }

    class DT_Class {

        public DT_Class(string identifier) {
            Identifier = identifier;
        }

        public string Identifier { get; }

    }

}