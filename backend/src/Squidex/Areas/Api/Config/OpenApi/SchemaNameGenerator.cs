// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NJsonSchema.Generation;
using Squidex.Domain.Apps.Core.Rules.Deprecated;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Areas.Api.Config.OpenApi;

public sealed class SchemaNameGenerator : DefaultSchemaNameGenerator
{
    public override string Generate(Type type)
    {
#pragma warning disable CS0618 // Type or member is obsolete
        if (type.BaseType == typeof(RuleAction))
        {
            return $"{type.TypeName(false, "Action")}RuleActionDto";
        }

        if (type == typeof(RuleAction))
        {
            return $"RuleActionDto";
        }
#pragma warning restore CS0618 // Type or member is obsolete

        return base.Generate(type);
    }
}
