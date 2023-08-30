using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wasm.Sdk.Analyzer.Extensions
{
    public static class RoslynExtensions
    {
        public static bool HasAttribute(this ClassDeclarationSyntax classSyntax, string attributeName)
        {
            return classSyntax.AttributeLists.Count > 0 &&
                   classSyntax.AttributeLists.SelectMany(al => al.Attributes
                           .Where(a =>
                           {
                               var attribute = a.Name;
                               if (a.Name is QualifiedNameSyntax qualifiedNameSyntax)
                               {
                                   attribute = qualifiedNameSyntax.Right;
                               }
                               return (attribute as IdentifierNameSyntax).Identifier.Text == attributeName;
                           }))
                       .Any();
        }

        public static bool IsDerivedFromClassOrInterface(this INamedTypeSymbol type, INamedTypeSymbol type2)
        {
            var currentType = type;
            while (currentType != null)
            {
                if (currentType == type2 || currentType.OriginalDefinition == type2)
                {
                    return true;
                }
                currentType = currentType.BaseType;
            }

            return type.AllInterfaces.Any(i => i == type2 || i.OriginalDefinition == type2);
        }

        public static List<string> GetUsings(this CompilationUnitSyntax root)
        {
            return root.ChildNodes()
                .OfType<UsingDirectiveSyntax>()
                .Select(n => n.Name.ToString())
                .ToList();
        }

        public static CompilationUnitSyntax GetCompilationUnit(this SyntaxNode syntaxNode)
        {
            return syntaxNode.Ancestors().OfType<CompilationUnitSyntax>().FirstOrDefault();
        }

        public static bool IsPrimitive(this ITypeSymbol typeSymbol)
        {
            return Constants.PrimitiveSpecialTypes.Contains(typeSymbol.SpecialType);
        }
    }
}
