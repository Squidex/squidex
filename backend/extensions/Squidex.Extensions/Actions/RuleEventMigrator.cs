// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Backup;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Extensions.Actions.Script;
using Squidex.Flows;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Json;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Extensions.Actions;

public sealed class RuleEventMigrator(TypeRegistry typeRegistry, IJsonSerializer serializer) : IEventMigrator
{
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
                    if (key == "$type" && nested.Value is string flowStepType)
                    {
                        if (!typeRegistry.TryGetType<FlowStep>(flowStepType, out _))
                        {
                            isChanged = true;
                            obj["$type"] = typeRegistry.GetName<FlowStep, ScriptFlowStep>();
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
}
