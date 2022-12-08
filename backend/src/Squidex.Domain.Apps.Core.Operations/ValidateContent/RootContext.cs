// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Core.ValidateContent;

public sealed class RootContext
{
    private readonly ConcurrentBag<ValidationError> errors = new ConcurrentBag<ValidationError>();
    private readonly Scheduler scheduler = new Scheduler();

    public IJsonSerializer Serializer { get; }

    public DomainId ContentId { get; }

    public NamedId<DomainId> AppId { get; }

    public NamedId<DomainId> SchemaId { get; }

    public Schema Schema { get; }

    public ResolvedComponents Components { get; }

    public IEnumerable<ValidationError> Errors
    {
        get => errors;
    }

    public RootContext(
        IJsonSerializer serializer,
        NamedId<DomainId> appId,
        NamedId<DomainId> schemaId,
        Schema schema,
        DomainId contentId,
        ResolvedComponents components)
    {
        AppId = appId;
        Components = components;
        ContentId = contentId;
        Serializer = serializer;
        Schema = schema;
        SchemaId = schemaId;
    }

    public void AddError(IEnumerable<string> path, string message)
    {
        errors.Add(new ValidationError(message, path.ToPathString()));
    }

    public void AddTask(SchedulerTask task)
    {
        scheduler.Schedule(task);
    }

    public void ThrowOnErrors()
    {
        if (!errors.IsEmpty)
        {
            throw new ValidationException(errors.ToList());
        }
    }

    public ValueTask CompleteAsync(
        CancellationToken ct = default)
    {
        return scheduler.CompleteAsync(ct);
    }
}
