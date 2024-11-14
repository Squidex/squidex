// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.Repositories;
using Squidex.Domain.Apps.Entities.Jobs;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Contents.Indexes;

public sealed class CreateIndexJob : IJobRunner
{
    public const string TaskName = "createIndex";
    public const string ArgAppId = "appId";
    public const string ArgAppName = "appName";
    public const string ArgSchemaId = "schemaId";
    public const string ArgSchemaName = "schemaName";
    public const string ArgFieldName = "field_";
    private readonly IContentRepository contentRepository;

    public string Name => TaskName;

    public CreateIndexJob(IContentRepository contentRepository)
    {
        this.contentRepository = contentRepository;
    }

    public static JobRequest BuildRequest(RefToken actor, App app, Schema schema, IndexDefinition index)
    {
        Guard.NotNull(actor);
        Guard.NotNull(app);
        Guard.NotNull(schema);
        Guard.NotNull(index);

        var args = new Dictionary<string, string>
        {
            [ArgAppId] = app.Id.ToString(),
            [ArgAppName] = app.Name,
            [ArgSchemaId] = schema.Id.ToString(),
            [ArgSchemaName] = schema.Name
        };

        foreach (var field in index)
        {
            args[$"{ArgFieldName}{field.Name}"] = field.Order.ToString();
        }

        return JobRequest.Create(
            actor,
            TaskName,
            args) with
        {
            AppId = app.NamedId()
        };
    }

    public async Task RunAsync(JobRunContext context,
        CancellationToken ct)
    {
        // The other arguments are just there for debugging purposes. Therefore do not validate them.
        if (!context.Job.Arguments.TryGetValue(ArgSchemaId, out var schemaId))
        {
            throw new DomainException($"Argument '{ArgSchemaId}' missing.");
        }

        if (!context.Job.Arguments.TryGetValue(ArgSchemaName, out var schemaName))
        {
            throw new DomainException($"Argument '{ArgSchemaName}' missing.");
        }

        var index = new IndexDefinition();

        foreach (var (arg, value) in context.Job.Arguments)
        {
            if (!arg.StartsWith(ArgFieldName, StringComparison.Ordinal))
            {
                continue;
            }

            var field = arg[ArgFieldName.Length..];

            if (!Enum.TryParse<SortOrder>(value, out var order))
            {
                throw new DomainException($"Invalid sort order {order} for field {field}.");
            }

            index.Add(new IndexField(field, order));
        }

        if (index.Count == 0)
        {
            throw new DomainException("Index does not contain an field.");
        }

        // Use a readable name to describe the job.
        context.Job.Description = $"Schema {schemaName}: Create index {index.ToName()}";

        await contentRepository.CreateIndexAsync(context.OwnerId, DomainId.Create(schemaId), index, ct);
    }
}
