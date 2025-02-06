using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Ryujinx.Horizon.Generators.Hipc
{
    class CommandImplInterface
    {
        public ClassDeclarationSyntax ClassDeclarationSyntax { get; }
        public List<MethodDeclarationSyntax> CommandImplementations { get; }

        public CommandImplInterface(ClassDeclarationSyntax classDeclarationSyntax)
        {
            ClassDeclarationSyntax = classDeclarationSyntax;
            CommandImplementations = new List<MethodDeclarationSyntax>();
        }
    }
}
