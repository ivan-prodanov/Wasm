using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using Wasm.Sdk.Analyzer.Extensions;

namespace Wasm.Sdk.Analyzer
{
    internal class WasmSyntaxReceiver : ISyntaxReceiver
    {
        public List<ClassDeclarationSyntax> WasmClassesDeclarations { get; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classSyntax && classSyntax.HasAttribute("Wasm"))
            {
                WasmClassesDeclarations.Add(classSyntax);
            }
        }
    }
}
