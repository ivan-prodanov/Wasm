using System;
using System.Collections.Generic;
using System.Text;

namespace Wasm.Sdk.Analyzer.Proxy
{
    class WasmMethod : WasmMethodBase
    {
        public WasmMethod(string name, IEnumerable<WasmParameter> parameters, WasmType returnType, bool isAsync, bool isStatic)
            : base(parameters)
        {
            returnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            ReturnType = returnType;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            ProxyName = $"_{name}";
            TsName = Char.ToLowerInvariant(name[0]) + name.Substring(1); // camelCase
            HasReturnValue = ReturnType.ManagedType != "void" || isAsync;
            IsAsync = isAsync;
            IsStatic = isStatic;

            var managedReturnTypeName = ReturnType.Nullable ? $"{ReturnType.ProxyType}?" : ReturnType.ProxyType;
            if (IsAsync)
            {
                if (ReturnType.ManagedType != "void")
                {
                    ManagedReturnType = $"Task<{managedReturnTypeName}>";
                }
                else
                {
                    ManagedReturnType = "Task";
                }
            }
            else
            {
                ManagedReturnType = managedReturnTypeName;
            }

            var tsType = ReturnType.Nullable ? $"{ReturnType.TsType} | undefined" : ReturnType.TsType;
            TsReturnTypeUnwrapped = tsType;
            TsReturnType = IsAsync ? $"Promise<{TsReturnTypeUnwrapped}>" : TsReturnTypeUnwrapped;
            ReturnTypeSerialized = ReturnType.TypeConversion == TypeConversion.Serialize;
            ReturnTypeTypedArray = ReturnType.TypeConversion == TypeConversion.TypedArray;
        }

        public string Name { get; internal set; }
        public string ProxyName { get; internal set; }
        public string TsName { get; }
        public WasmType ReturnType { get; }
        public string ManagedReturnType { get; }
        public bool ReturnTypeSerialized { get; }
        public bool ReturnTypeTypedArray { get; }
        public bool ReturnTypeManipualted => ReturnTypeSerialized || ReturnTypeTypedArray;
        public string TsReturnType { get; }
        public string TsReturnTypeUnwrapped { get; }
        public bool HasReturnValue { get; }
        public bool IsAsync { get; }
        public bool IsStatic { get; }
    }
}
