using System.Runtime.CompilerServices;

public sealed class Interop
{
	public sealed class Runtime
	{
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public static extern string InvokeJS(string str, out int exceptional_result);
	}
}

