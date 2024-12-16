// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema.Generation;
using Squidex.Domain.Apps.Core.Rules.Old;
using Squidex.Flows;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Config.OpenApi;

public sealed class SchemaNameGenerator : DefaultSchemaNameGenerator
{
    public override string Generate(Type type)
    {
        if (type.BaseType == typeof(RuleAction))
        {
            return $"{type.TypeName(false, "Action")}RuleActionDto";
        }

        if (type == typeof(RuleAction))
        {
            return $"RuleActionDto";
        }

        if (type.GetInterfaces().Contains(typeof(IFlowStep)))
        {
            return $"{type.TypeName(false, "FlowStep")}FlowStepDto";
        }

        if (type == typeof(IFlowStep))
        {
            return $"FlowStepDto";
        }

        return base.Generate(type);
    }
}
