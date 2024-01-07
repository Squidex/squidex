// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Jobs;

public sealed class Job
{
    public DomainId Id { get; init; }

    public Instant Started { get; set; }

    public Instant? Stopped { get; set; }

    public string TaskName { get; init; }

    public JobFile? File { get; set; }

    public Dictionary<string, string> Arguments { get; init; }

    public List<string> Log { get; set; } = [];

    public JobStatus Status { get; set; }
}
