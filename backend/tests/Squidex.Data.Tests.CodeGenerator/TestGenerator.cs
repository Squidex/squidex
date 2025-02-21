// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics;
using System.Text;
using HandlebarsDotNet;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Squidex.Data.Tests.CodeGenerator;

[Generator]
public class TestGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var templateStream = typeof(TestGenerator).Assembly.GetManifestResourceStream("Squidex.Template.handlebar")!;
        var templateText = new StreamReader(templateStream).ReadToEnd();

        var template = Handlebars.Compile(templateText);

        static TemplateModel? Transform(GeneratorSyntaxContext ctx)
        {
            var classSyntax = (ClassDeclarationSyntax)ctx.Node;

            var className = classSyntax.Identifier.Text;
            if (!className.StartsWith("EF", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            if (!classSyntax.Modifiers.Any(x => x.IsKind(SyntaxKind.AbstractKeyword)))
            {
                return null;
            }

            if (classSyntax.TypeParameterList == null ||
                classSyntax.TypeParameterList.Parameters.Count != 1 ||
                classSyntax.TypeParameterList.Parameters[0].Identifier.Text != "TContext")
            {
                return null;
            }

            var namespaceDeclaration =
                classSyntax.Ancestors()
                    .OfType<BaseNamespaceDeclarationSyntax>().First();

            return new TemplateModel
            {
                BaseName = classSyntax.Identifier.Text,
                ClassName = classSyntax.Identifier.Text.Substring(2),
                ClassNamespace = namespaceDeclaration.Name.ToString(),
            };
        }

        var fieldDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) =>
            {
                return node is ClassDeclarationSyntax;
            },
            static (ctx, _) => Transform(ctx))
            .Where(x => x != null);

        context.RegisterSourceOutput(fieldDeclarations, (context, model) =>
        {
            var source = template(model);

            context.AddSource($"{model!.BaseName}_Tests.cs", SourceText.From(source, Encoding.UTF8));
        });
    }
}
