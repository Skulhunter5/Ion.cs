using System.Collections.Generic;

using System; // REMOVE

namespace Ion {

    class DataTypes {

        private Dictionary<string, DataType> types = new Dictionary<string, DataType>() {
            {"void", DataType.I_VOID},
            {"bool", DataType.I_BOOLEAN},
            {"uint64", DataType.I_UINT64},
        };
        private Dictionary<string, DT_Struct> structs = new Dictionary<string, DT_Struct>() {};
        private Dictionary<string, DT_Class> classes = new Dictionary<string, DT_Class>() {};

        public bool DataTypeExists(string identifier) {

            return types.ContainsKey(identifier);
        }

        public DataType GetDataType(Token identifier) {
            if(types.ContainsKey(identifier.Value)) return types[identifier.Value];
            if(structs.ContainsKey(identifier.Value)) return new DataType(structs[identifier.Value]);
            if(classes.ContainsKey(identifier.Value)) return new DataType(classes[identifier.Value]);

            ErrorSystem.AddError_i(new UnknownDataTypeError(identifier));
            return null; // Unreachable
        }

        public void Add(DT_Struct dT_Struct) {
            structs.Add(dT_Struct.Identifier, dT_Struct);
        }
        public void Add(DT_Class dT_Class) {
            classes.Add(dT_Class.Identifier, dT_Class);
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
        public DataType(uint pointerValue, uint pointerN, DT_Class dT_Class, DT_Struct dT_Struct) {
            Value = DataType.POINTER;
            PointerValue = pointerValue;
            PointerN = pointerN;
            DT_Class = dT_Class;
            DT_Struct = dT_Struct;
        }
        public DataType(DT_Class dT_Class) {
            Value = DataType.CLASS;
            DT_Class = dT_Class;
        }
        public DataType(DT_Struct dT_Struct) {
            Value = DataType.STRUCT;
            DT_Struct = dT_Struct;
        }
        public DataType(DT_Class dT_Class, uint pointerN) {
            Value = DataType.POINTER;
            PointerValue = DataType.CLASS;
            PointerN = pointerN;
            DT_Class = dT_Class;
        }
        public DataType(DT_Struct dT_Struct, uint pointerN) {
            Value = DataType.POINTER;
            PointerValue = DataType.STRUCT;
            PointerN = pointerN;
            DT_Struct = dT_Struct;
        }

        public uint Value { get; }
        public uint PointerValue { get; }
        public uint PointerN { get; }
        public DT_Class DT_Class { get; }
        public DT_Struct DT_Struct { get; }

        // STATIC

        public static readonly uint VOID      = 0x000;
        public static readonly uint BOOLEAN   = 0x001;
        public static readonly uint UINT64    = 0x010;
        public static readonly uint POINTER   = 0x011;
        public static readonly uint CLASS     = 0x100;
        public static readonly uint STRUCT     = 0x101;

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

    class DT_Struct {

        public DT_Struct(string identifier, Dictionary<string, DataType> fields) {
            Identifier = identifier;
            Fields = fields;
        }

        public string Identifier { get; }
        public Dictionary<string, DataType> Fields { get; }

    }

}