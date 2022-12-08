// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;

namespace Migrations.OldEvents;

[EventType(nameof(ScriptsConfigured))]
[Obsolete("New Event introduced")]
public sealed class ScriptsConfigured : SchemaEvent, IMigrated<IEvent>
{
    public string ScriptQuery { get; set; }

    public string ScriptCreate { get; set; }

    public string ScriptUpdate { get; set; }

    public string ScriptDelete { get; set; }

    public string ScriptChange { get; set; }

    public IEvent Migrate()
    {
        var scripts = new SchemaScripts();

        if (!string.IsNullOrWhiteSpace(ScriptQuery))
        {
            scripts = scripts with { Query = ScriptQuery };
        }

        if (!string.IsNullOrWhiteSpace(ScriptCreate))
        {
            scripts = scripts with { Create = ScriptCreate };
        }

        if (!string.IsNullOrWhiteSpace(ScriptUpdate))
        {
            scripts = scripts with { Update = ScriptUpdate };
        }

        if (!string.IsNullOrWhiteSpace(ScriptDelete))
        {
            scripts = scripts with { Delete = ScriptDelete };
        }

        if (!string.IsNullOrWhiteSpace(ScriptChange))
        {
            scripts = scripts with { Change = ScriptChange };
        }

        return SimpleMapper.Map(this, new SchemaScriptsConfigured { Scripts = scripts });
    }
}
