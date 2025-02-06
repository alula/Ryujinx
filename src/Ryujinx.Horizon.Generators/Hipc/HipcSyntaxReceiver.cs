using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ryujinx.Horizon.Generators.Hipc
{
    class HipcSyntaxReceiver : ISyntaxReceiver
    {
        public List<CommandImplInterface> CommandImplInterfaces { get; }
        public List<CommandApiInterface> CommandApiInterfaces { get; }

        private static string CommandCmifAttr = HipcGenerator.CommandAttributeName.Replace("Attribute", string.Empty);
        private static string ImplementApiAttr = HipcGenerator.ImplementApiAttributeName.Replace("Attribute", string.Empty);

        public HipcSyntaxReceiver()
        {
            CommandImplInterfaces = new();
            CommandApiInterfaces = new();
        }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if (syntaxNode is ClassDeclarationSyntax classDeclaration)
            {
                if (!classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword) || classDeclaration.BaseList == null)
                {
                    return;
                }

                // todo: shitty hack
                bool hasImplApiAttribute = classDeclaration.AttributeLists
                    .Any(x => x.Attributes.Any(y => y.Name.ToString() == ImplementApiAttr));

                if (hasImplApiAttribute)
                {
                    VisitCommandApi(classDeclaration);
                }
                else
                {
                    VisitCommandImpl(classDeclaration);
                }
            }
        }

        private void VisitCommandImpl(ClassDeclarationSyntax classDeclaration)
        {
            var commandInterface = new CommandImplInterface(classDeclaration);

            foreach (var memberDeclaration in classDeclaration.Members)
            {
                if (memberDeclaration is MethodDeclarationSyntax methodDeclaration)
                {
                    VisitMethod(commandInterface.CommandImplementations, methodDeclaration);
                }
            }

            CommandImplInterfaces.Add(commandInterface);
        }

        private void VisitCommandApi(ClassDeclarationSyntax classDeclaration)
        {
            var commandInterface = new CommandApiInterface(classDeclaration);

            foreach (var syntaxNode in IterateBaseTypeSyntaxNodes(classDeclaration.BaseList.Types))
            {
                if (syntaxNode is InterfaceDeclarationSyntax interfaceDeclaration)
                {

                    foreach (var memberDeclaration in interfaceDeclaration.Members)
                    {
                        if (memberDeclaration is MethodDeclarationSyntax methodDeclaration)
                        {
                            VisitMethod(commandInterface.CommandImplementations, methodDeclaration);
                        }
                    }
                }
            }

            CommandApiInterfaces.Add(commandInterface);
        }

        private static IEnumerable<SyntaxNode> IterateBaseTypeSyntaxNodes(SeparatedSyntaxList<BaseTypeSyntax> baseListTypes)
        {
            foreach (var baseType in baseListTypes)
            {
                foreach (var syntaxNode in baseType.DescendantNodes())
                {
                    yield return syntaxNode;
                }
            }
        }

        internal static void VisitMethod(List<MethodDeclarationSyntax> commandImplementations, MethodDeclarationSyntax methodDeclaration)
        {
            if (methodDeclaration.AttributeLists.Count != 0)
            {
                foreach (var attributeList in methodDeclaration.AttributeLists)
                {
                    if (attributeList.Attributes.Any(x => x.Name.ToString().Contains(CommandCmifAttr)))
                    {
                        commandImplementations.Add(methodDeclaration);
                        break;
                    }
                }
            }
        }
    }
}
