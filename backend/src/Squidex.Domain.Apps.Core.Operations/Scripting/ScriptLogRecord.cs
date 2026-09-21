// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting;

public sealed class ScriptLogRecord
{
    public DomainId Id { get; set; } = DomainId.NewGuid();

    public DomainId AppId { get; set; }

    public string Name { get; set; }

    public Instant Timestamp { get; set; }

    public List<ScriptLogEntry> Entries { get; set; } = [];

    public int TotalEntries { get; set; }
}
