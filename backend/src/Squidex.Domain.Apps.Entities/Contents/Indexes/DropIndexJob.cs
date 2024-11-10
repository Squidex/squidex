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

namespace Squidex.Domain.Apps.Entities.Contents.Indexes;

public sealed class DropIndexJob : IJobRunner
{
    public const string TaskName = "dropIndex";
    public const string ArgAppId = "appId";
    public const string ArgAppName = "appName";
    public const string ArgSchemaId = "schemaId";
    public const string ArgSchemaName = "schemaName";
    public const string ArgIndexName = "indexName";
    private readonly IContentRepository contentRepository;

    public string Name => TaskName;

    public DropIndexJob(IContentRepository contentRepository)
    {
        this.contentRepository = contentRepository;
    }

    public static JobRequest BuildRequest(RefToken actor, App app, Schema schema, string name)
    {
        Guard.NotNull(actor);
        Guard.NotNull(app);
        Guard.NotNull(schema);
        Guard.NotNullOrEmpty(name);

        return JobRequest.Create(
            actor,
            TaskName,
            new Dictionary<string, string>
            {
                [ArgAppId] = app.Id.ToString(),
                [ArgAppName] = app.Name,
                [ArgSchemaId] = schema.Id.ToString(),
                [ArgSchemaName] = schema.Name,
                [ArgIndexName] = name
            }) with
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

        if (!context.Job.Arguments.TryGetValue(ArgIndexName, out var indexName))
        {
            throw new DomainException($"Argument '{ArgIndexName}' missing.");
        }

        // Use a readable name to describe the job.
        context.Job.Description = $"Schema {schemaName}: Drop index {indexName}";

        await contentRepository.DropIndexAsync(context.OwnerId, DomainId.Create(schemaId), indexName, ct);
    }
}
