// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Jobs;

public interface IJobService
{
    Task StartAsync(DomainId ownerId, JobRequest request,
        CancellationToken ct = default);

    Task<List<Job>> GetJobsAsync(DomainId ownerId,
        CancellationToken ct = default);

    Task CancelAsync(DomainId ownerId, string? taskName = null,
        CancellationToken ct = default);

    Task DeleteJobAsync(DomainId ownerId, DomainId jobId,
        CancellationToken ct = default);

    Task DownloadAsync(Job job, Stream stream,
        CancellationToken ct = default);
}
