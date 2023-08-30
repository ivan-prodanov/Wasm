using System;
using System.Collections.Generic;
using System.Text;

namespace Wasm.Sdk
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class WasmAttribute : Attribute
    {
        public static string Name = nameof(WasmAttribute).Replace("Attribute", string.Empty);
    }
}