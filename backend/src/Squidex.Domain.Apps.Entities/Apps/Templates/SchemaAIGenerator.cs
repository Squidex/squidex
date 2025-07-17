// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using ConsoleTables;
using Microsoft.Extensions.Options;
using Squidex.AI.Implementation.OpenAI;
using Squidex.CLI.Commands.Implementation.AI;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Json;

namespace Squidex.Domain.Apps.Entities.Apps.Templates;

public sealed class SchemaAIGenerator(
    IQueryCache queryCache,
    SessionFactory sessionFactory,
    IOptions<SchemasOptions> schemasOptions,
    IOptions<OpenAIChatOptions> openAIOptions)
{
    private const int MaxContentItems = 20;

    public async Task<SchemaAIResult> ExecuteAsync(App app, string prompt, int numberOfContentItems, bool execute,
        CancellationToken ct)
    {
        var apiKey = openAIOptions.Value.ApiKey;
        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new NotSupportedException("OpenAI ApiKey not configured.");
        }

        var request = new GenerateRequest
        {
            Description = prompt,
            GenerateImages = false,
            NumberOfAttempts = 3,
            NumberOfContentItems = Math.Min(MaxContentItems, numberOfContentItems),
            OpenAIApiKey = apiKey,
            SystemPrompt = schemasOptions.Value.GeneratePrompt,
        };

        var generator = new AIContentGenerator(queryCache);
        var generated = await generator.GenerateAsync(request, ct);

        using var cliLog = new StringLogger();
        WriteSchema(generated, cliLog);
        WriteContent(generated, cliLog);

        if (execute)
        {
            var session = sessionFactory.CreateSession(app);

            var executor = new AIContentExecutor(session, cliLog);
            await executor.ExecuteAsync(request, generated, ct);
        }

        return new SchemaAIResult(cliLog.Lines.ToReadonlyList(), execute ? generated.Schema.Name : null);
    }

    private static void WriteSchema(GeneratedContent generated, StringLogger log)
    {
        log.WriteLine($"Schema Name: {generated.Schema.Name}");
        log.WriteLine();
        log.WriteLine("Schema Fields:");

        var schemaTable = new ConsoleTable("Name", "Type", "Required", "Localized");

        schemaTable.Options.EnableCount = false;
        foreach (var field in generated.Schema.Fields)
        {
            schemaTable.AddRow(field.Name, field.Type, field.IsRequired, field.IsLocalized);
        }

        log.WriteLine(schemaTable.ToString());
    }

    private static void WriteContent(GeneratedContent generated, StringLogger log)
    {
        if (generated.Contents.Count > 0)
        {
            const int MaximumPreview = 3;

            log.WriteLine();
            log.WriteLine("Contents:");
            log.WriteJson(generated.Contents.Take(MaximumPreview));

            var more = generated.Contents.Count - MaximumPreview;
            if (more > 0)
            {
                log.WriteLine($"+ {more} content items");
            }
        }
    }
}
