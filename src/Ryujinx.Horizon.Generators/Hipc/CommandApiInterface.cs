using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Ryujinx.Horizon.Generators.Hipc
{
    class CommandApiInterface
    {
        public ClassDeclarationSyntax ClassDeclarationSyntax { get; }
        public List<MethodDeclarationSyntax> CommandImplementations { get; }

        public CommandApiInterface(ClassDeclarationSyntax classDeclarationSyntax)
        {
            ClassDeclarationSyntax = classDeclarationSyntax;
            CommandImplementations = new List<MethodDeclarationSyntax>();
        }
    }
}
