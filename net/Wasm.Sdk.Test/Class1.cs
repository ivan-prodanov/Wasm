using ConsoleApp1;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Linq;

namespace Wasm.Sdk.Test
{
    public static class Class1
    {
        public static string Test()
        {
            return $"{TimeZoneInfo.Local.StandardName} :: {DateTime.Now}";
        }

        public static string TestJS()
        {
            var result = Interop.Runtime.InvokeJS("2 + 2", out var except);
            if (except != default)
            {
                return $"Error: {except}";
            }

            return $"Javascript result: {result}";
        }

        public static int TestPlus(int a, int b)
        {
            var externalClass = new ExternalClass();
            var result = externalClass.Plus(a, b);
            return result;
        }

        public static string TestAnalysis()
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(HelloWorldCSharp);
            var compilation = CSharpCompilation.Create("Test.dll");
            compilation.AddSyntaxTrees(syntaxTree);
            var diagnostics = compilation.GetDiagnostics().FirstOrDefault();
            if (diagnostics != null)
            {
                var error = diagnostics.ToString();
                return error;
            }

            return "No errors";
        }

        const string HelloWorldCSharp = @"using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VS
{
class Program
{
    static void Main(string[] args)
    {
        bool isPrime = true;
        Console.WriteLine(""Prime Numbers : "");

        for (int i = 2; i <= 100; i++)
        {
            for (int j = 2; jz <= 100; j++)
            {
                if (i != j && i % j == 0)
                {
                    isPrime = false;
                    break;
                }
            }

            if (isPrime)
            {
                Console.Write(""\t"" + i);
            }
            isPrime = true;
        }

        Console.ReadKey();
    }
}
}";
    }
}
