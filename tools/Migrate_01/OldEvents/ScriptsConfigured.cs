// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;

namespace Migrate_01.OldEvents
{
    [EventType(nameof(ScriptsConfigured))]
    [Obsolete]
    public sealed class ScriptsConfigured : SchemaEvent, IMigrated<IEvent>
    {
        public string ScriptQuery { get; set; }

        public string ScriptCreate { get; set; }

        public string ScriptUpdate { get; set; }

        public string ScriptDelete { get; set; }

        public string ScriptChange { get; set; }

        public IEvent Migrate()
        {
            var scripts = new Dictionary<string, string>();

            if (!string.IsNullOrWhiteSpace(ScriptQuery))
            {
                scripts[Scripts.Query] = ScriptQuery;
            }

            if (!string.IsNullOrWhiteSpace(ScriptCreate))
            {
                scripts[Scripts.Create] = ScriptCreate;
            }

            if (!string.IsNullOrWhiteSpace(ScriptUpdate))
            {
                scripts[Scripts.Update] = ScriptUpdate;
            }

            if (!string.IsNullOrWhiteSpace(ScriptDelete))
            {
                scripts[Scripts.Delete] = ScriptDelete;
            }

            if (!string.IsNullOrWhiteSpace(ScriptChange))
            {
                scripts[Scripts.Change] = ScriptChange;
            }

            return SimpleMapper.Map(this, new SchemaScriptsConfigured { Scripts = scripts });
        }
    }
}
