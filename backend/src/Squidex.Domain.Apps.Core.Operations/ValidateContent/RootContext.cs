// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Concurrent;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Core.ValidateContent;

public sealed class RootContext(App app, Schema schema, DomainId contentId, ResolvedComponents components,
    IJsonSerializer serializer)
{
    private readonly ConcurrentBag<ValidationError> errors = [];
    private readonly Scheduler scheduler = new Scheduler();

    public IJsonSerializer Serializer { get; } = serializer;

    public DomainId ContentId { get; } = contentId;

    public App App { get; } = app;

    public Schema Schema { get; } = schema;

    public ResolvedComponents Components { get; } = components;

    public IEnumerable<ValidationError> Errors
    {
        get => errors;
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
