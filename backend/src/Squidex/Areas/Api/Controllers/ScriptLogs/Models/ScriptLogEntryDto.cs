// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Controllers.ScriptLogs.Models;

public sealed class ScriptLogEntryDto
{
    /// <summary>
    /// The log level, for example 'log', 'info', 'warn' or 'error'.
    /// </summary>
    public string Level { get; set; }

    /// <summary>
    /// The logged message.
    /// </summary>
    public string Message { get; set; }

    public static ScriptLogEntryDto FromDomain(ScriptLogEntry entry)
    {
        return SimpleMapper.Map(entry, new ScriptLogEntryDto());
    }
}
