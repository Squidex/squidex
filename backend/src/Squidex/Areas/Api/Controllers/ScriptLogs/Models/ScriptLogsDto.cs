// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Scripting;

namespace Squidex.Areas.Api.Controllers.ScriptLogs.Models;

public sealed class ScriptLogsDto
{
    /// <summary>
    /// The script logs, newest first.
    /// </summary>
    public ScriptLogDto[] Items { get; set; }

    public static ScriptLogsDto FromDomain(IReadOnlyList<ScriptLogRecord> logs)
    {
        return new ScriptLogsDto { Items = logs.Select(ScriptLogDto.FromDomain).ToArray() };
    }
}
