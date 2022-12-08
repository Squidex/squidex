// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Entities.Schemas.Commands;

public sealed class ConfigureFieldRules : SchemaCommand
{
    public FieldRuleCommand[]? FieldRules { get; set; }

    public FieldRules ToFieldRules()
    {
        if (FieldRules?.Length > 0)
        {
            return new FieldRules(FieldRules.Select(x => x.ToFieldRule()).ToList());
        }
        else
        {
            return Core.Schemas.FieldRules.Empty;
        }
    }
}
