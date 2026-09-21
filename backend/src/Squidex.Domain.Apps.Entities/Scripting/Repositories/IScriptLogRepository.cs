// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Scripting.Repositories;

public interface IScriptLogRepository
{
    Task InsertManyAsync(IEnumerable<ScriptLogRecord> records,
        CancellationToken ct = default);

    Task TrimAsync(DomainId appId, int maxCount,
        CancellationToken ct = default);

    Task DeleteAsync(DomainId appId,
        CancellationToken ct = default);

    Task DeleteOlderThanAsync(Instant timestamp,
        CancellationToken ct = default);

    Task<IReadOnlyList<ScriptLogRecord>> QueryAsync(DomainId appId, string? name, int skip, int take,
        CancellationToken ct = default);
}
