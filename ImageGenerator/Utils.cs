using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImageGenerator {
    static class ExtensionUtils {
        public static IEnumerable<DynValue> GetArray(this Table table) {
            for(int i = 1; i <= table.Length; i++)
                yield return table.Get(i);
        }

        public static IEnumerable<DynValue> GetArrayTyped(this Table table, DataType type, string funcName,
                                            TypeValidationFlags flags = TypeValidationFlags.AutoConvert) {
            return table.GetArray().Select(x => x.CheckType(funcName, type, flags: flags));
        }

        public static IEnumerable<string> GetArrayString(this Table table, string funcName,
                                          TypeValidationFlags flags = TypeValidationFlags.AutoConvert) {
            return table.GetArrayTyped(DataType.String, funcName, flags).Select(x => x.String);
        }

        public static IEnumerable<T> GetArrayUserData<T>(this Table table, string funcName,
                                     TypeValidationFlags flags = TypeValidationFlags.AutoConvert) {
            return table.GetArray().Select(x => x.CheckUserDataType<T>(funcName, flags: flags));
        }
    }
}
