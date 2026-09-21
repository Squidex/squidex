// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.ScriptLogs.Models;

public sealed class ScriptLogDto
{
    /// <summary>
    /// The ID of the log.
    /// </summary>
    public DomainId Id { get; set; }

    /// <summary>
    /// The name of the script, for example 'contents/my-schema/create' or 'assets/annotate'.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// The time when the script has been executed.
    /// </summary>
    public Instant Timestamp { get; set; }

    /// <summary>
    /// The log entries.
    /// </summary>
    public ScriptLogEntryDto[] Entries { get; set; }

    /// <summary>
    /// The total number of entries the script has logged. Can be greater than the number of stored entries, because only the first entries are kept.
    /// </summary>
    public int TotalEntries { get; set; }

    public static ScriptLogDto FromDomain(ScriptLogRecord record)
    {
        return SimpleMapper.Map(record, new ScriptLogDto
        {
            Entries = record.Entries.Select(ScriptLogEntryDto.FromDomain).ToArray(),
        });
    }
}
