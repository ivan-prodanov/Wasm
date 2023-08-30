using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Wasm.Sdk.Analyzer.Generator;
using Wasm.Sdk.Analyzer.Resolvers;
using Xunit;

namespace Wasm.Sdk.Analyzer.Tests.TypeModelResolverTests
{
    public class TypeTests
    {
        internal (TypeModelResolver resolver, IGeneratorContext context, IPropertySymbol property) ArrangeProperty(string code)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(code);
            var compilation = CSharpCompilation
                .Create("Test", new List<SyntaxTree> { syntaxTree })
                .AddReferences(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));

            var resolver = new TypeModelResolver(compilation);
            var context = Mock.Of<IGeneratorContext>();
            var model = compilation.GetSemanticModel(syntaxTree);

            var property = syntaxTree.GetRoot().ChildNodes().OfType<ClassDeclarationSyntax>().First().ChildNodes().OfType<PropertyDeclarationSyntax>().First();
            var propertySymbol = model.GetDeclaredSymbol(property);

            return (resolver, context, propertySymbol);
        }
    }
}
