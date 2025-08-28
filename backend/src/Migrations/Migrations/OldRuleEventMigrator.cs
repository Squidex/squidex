// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Migrations.OldEvents;
using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Reflection;

namespace Migrations.Migrations;

public sealed class OldRuleEventMigrator(TypeRegistry typeRegistry, IJsonSerializer serializer) : IEventMigrator
{
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
    private readonly string[] migratedEvents =
    [
        typeRegistry.GetName<IEvent>(typeof(RuleCreated)),
        typeRegistry.GetName<IEvent>(typeof(RuleUpdated)),
    ];

    public string? ProcessEvent(string type, string json)
    {
        if (!migratedEvents.Contains(type))
        {
            return null;
        }

        var parsed = serializer.Deserialize<JsonValue>(json);

        var isChanged = false;
        void Handle(JsonValue value)
        {
            if (value.Type == JsonValueType.Object)
            {
                var obj = value.AsObject;
                foreach (var (key, nested) in obj)
                {
                    if (key == "actionType" && nested.Value is string flowStepType)
                    {
                        if (!typeRegistry.TryGetType<RuleAction>(flowStepType, out _))
                        {
                            isChanged = true;
                            obj["actionType"] = "Webhook";
                        }
                    }
                    else
                    {
                        Handle(nested);
                    }
                }
            }
            else if (value.Type == JsonValueType.Array)
            {
                foreach (var item in value.AsArray)
                {
                    Handle(item);
                }
            }
        }

        Handle(parsed);
        if (!isChanged)
        {
            return null;
        }

        return serializer.Serialize(parsed);
    }
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
}
