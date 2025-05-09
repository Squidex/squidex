// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema.Generation;
using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Flows;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Config.OpenApi;

public sealed class SchemaNameGenerator : DefaultSchemaNameGenerator
{
    public override string Generate(Type type)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var result =
            GenerateTypeName<FlowStep>(type, "FlowStep") ??
            GenerateTypeName<EnrichedEvent>(type, "Event") ??
            GenerateTypeName<RuleAction>(type, "Action", "RuleAction") ??
            base.Generate(type);
#pragma warning restore CS0618 // Type or member is obsolete

        return result;
    }

    private static string? GenerateTypeName<T>(Type type, string suffix, string? newSuffix = null)
    {
        newSuffix ??= suffix;

        if (type == typeof(T))
        {
            return $"{type.TypeName(false)}Dto";
        }

        if (type.IsAssignableTo(typeof(T)))
        {
            return $"{type.TypeName(false, suffix)}{newSuffix}Dto";
        }

        return null;
    }
}
