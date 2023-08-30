declare global {
    var BINDING: any;
}

export function invokeMethod(fqn: number, thisArg: number, signature: string, args: any[]): any {
    let instancePtr = 0;
    if (thisArg) {
        // Get class instance Ptr from gcHandle
        instancePtr = BINDING.wasm_get_raw_obj(thisArg);

        // Make sure it hasn't been disposed.
        if (!instancePtr) {
            throw "CalculatorController has been disposed. Attempted to call method on a GCed instance";
        }
    }

    return BINDING.call_method(fqn, instancePtr, signature, args);
}

export function getMethodInfo(classPtr: number, methodFqn: string, parametersCount: number): number {
    return BINDING.find_method(classPtr, methodFqn, parametersCount);
}

export interface IWasmResource {
    /** @internal */
    readonly gcHandle: number;
}

export class WasmHelper {
    private static _assemblyPtr: number;
    private static _classPtr: number;
    private static _freePtr: number;

    private static get assemblyPtr(): number {
        if (!WasmHelper._assemblyPtr){
            WasmHelper._assemblyPtr = BINDING.assembly_load("Wasm.Sdk");
        }

        return WasmHelper._assemblyPtr;
    }

    private static get classPtr(): number {
        if (!WasmHelper._classPtr){
            WasmHelper._classPtr = BINDING.find_class(WasmHelper.assemblyPtr, "Wasm.Sdk", "WasmHelper");
        }

        return WasmHelper._classPtr;
    }

    private static get freePtr(): number {
        if (!WasmHelper._freePtr) {
            WasmHelper._freePtr = getMethodInfo(WasmHelper.classPtr, "FreeGCHandle", 1);
        }

        return WasmHelper._freePtr;
    }

    static free(wasmResource: IWasmResource) {
        invokeMethod(WasmHelper.freePtr, 0, "i", [wasmResource.gcHandle]);
    }
}