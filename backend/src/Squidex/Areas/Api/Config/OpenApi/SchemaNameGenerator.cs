// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema.Generation;
using Squidex.Domain.Apps.Core.Rules;
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

        return base.Generate(type);
    }
}
