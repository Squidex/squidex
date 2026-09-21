// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting;

public interface IScriptLogStore
{
    void Log(DomainId appId, string name, ScriptLog log);

    Task<IReadOnlyList<ScriptLogRecord>> QueryAsync(DomainId appId, string? name, int skip, int take,
        CancellationToken ct = default);
}
