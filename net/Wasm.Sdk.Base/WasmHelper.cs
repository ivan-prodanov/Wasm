using System;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Wasm.Sdk
{
    public static class WasmHelper
    {
        public static void FreeGCHandle(int i)
        {
            var gcHandle = (GCHandle)(IntPtr)i;
            if (gcHandle.Target is IDisposable disposable)
            {
                disposable.Dispose();
            }
            gcHandle.Free();
            GC.Collect();
        }
    }
}