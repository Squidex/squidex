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
public class CodeGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        static TestModel? TransformTest(GeneratorSyntaxContext ctx)
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
                classSyntax.TypeParameterList.Parameters.Count == 0 ||
                classSyntax.TypeParameterList.Parameters[0].Identifier.Text != "TContext")
            {
                return null;
            }

            var reuseLabel = "default";
            foreach (var attributeList in classSyntax.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var name = attribute.Name.ToString();
                    if (name != "ReuseLabel" && name != "ReuseLabelAttribute")
                    {
                        continue;
                    }

                    if (attribute.ArgumentList?.Arguments.Count != 1)
                    {
                        continue;
                    }

                    var value = attribute.ArgumentList.Arguments[0];
                    if (value.Expression is not LiteralExpressionSyntax literal)
                    {
                        continue;
                    }

                    var candidate = literal.Token.ValueText;
                    if (!string.IsNullOrWhiteSpace(candidate))
                    {
                        reuseLabel = candidate;
                    }
                }
            }

            var namespaceDeclaration =
                classSyntax.Ancestors()
                    .OfType<BaseNamespaceDeclarationSyntax>().First();

            return new TestModel
            {
                BaseName = classSyntax.Identifier.Text,
                ClassName = classSyntax.Identifier.Text.Substring(2),
                ClassNamespace = namespaceDeclaration.Name.ToString(),
                CollectionSuffix = reuseLabel.ToPascalCase(),
                CollectionLabel = reuseLabel,
                HasContentContext = classSyntax.TypeParameterList.Parameters.Count == 2,
            };
        }

        var fieldDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
            static (node, _) =>
            {
                return node is ClassDeclarationSyntax;
            },
            static (ctx, _) => TransformTest(ctx))
            .Where(x => x != null);

        WriteTests(context, fieldDeclarations);
        WriteFixtures(context, fieldDeclarations!);
    }

    private static void WriteTests(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<TestModel?> fieldDeclarations)
    {
        var testTemplateStream = typeof(CodeGenerator).Assembly.GetManifestResourceStream("Squidex.TestTemplate.handlebar")!;
        var testTemplateText = new StreamReader(testTemplateStream).ReadToEnd();
        var testTemplateFunc = Handlebars.Compile(testTemplateText);

        context.RegisterSourceOutput(fieldDeclarations, (context, model) =>
        {
            var source = testTemplateFunc(model);

            context.AddSource($"{model!.BaseName}_Tests.cs", SourceText.From(source, Encoding.UTF8));
        });
    }

    private static void WriteFixtures(IncrementalGeneratorInitializationContext context, IncrementalValuesProvider<TestModel> fieldDeclarations)
    {
        var fixtureTemplateStream = typeof(CodeGenerator).Assembly.GetManifestResourceStream("Squidex.FixtureTemplate.handlebar")!;
        var fixtureTemplateText = new StreamReader(fixtureTemplateStream).ReadToEnd();
        var fixtureTemplateFunc = Handlebars.Compile(fixtureTemplateText);

        var fixtureDeclarations = fieldDeclarations.Select(
            static (x, ct) =>
            {
                return new FixtureModel { Label = x.CollectionLabel, Name = x.CollectionSuffix };
            })
            .Collect().SelectMany((values, _) => values.GroupBy(x => x.Label).Select(x => x.First()));

        context.RegisterSourceOutput(fixtureDeclarations, (context, model) =>
        {
            var source = fixtureTemplateFunc(model);

            context.AddSource($"{model.Name}_Fixtures.cs", SourceText.From(source, Encoding.UTF8));
        });
    }
}
