
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Ryujinx.Horizon.Generators.Hipc
{
    internal static class CodeGenUtils
    {
        public static string GetCanonicalTypeName(Compilation compilation, SyntaxNode syntaxNode)
        {
            TypeInfo typeInfo = compilation.GetSemanticModel(syntaxNode.SyntaxTree).GetTypeInfo(syntaxNode);
            string typeName = typeInfo.Type.ToDisplayString();

            int genericArgsStartIndex = typeName.IndexOf('<');
            if (genericArgsStartIndex >= 0)
            {
                return typeName.Substring(0, genericArgsStartIndex);
            }

            return typeName;
        }

        public static bool HasAttribute(Compilation compilation, ParameterSyntax parameterSyntax, string fullAttributeName)
        {
            foreach (var attributeList in parameterSyntax.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    if (GetCanonicalTypeName(compilation, attribute) == fullAttributeName)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static bool HasAttribute(Compilation compilation, ClassDeclarationSyntax syntaxNode, string fullAttributeName)
        {
            foreach (var attributeList in syntaxNode.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    if (GetCanonicalTypeName(compilation, attribute) == fullAttributeName)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

    }
}
