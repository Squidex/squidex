// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Rules.Json;

public sealed record RuleSorrgate : Rule, ISurrogate<Rule>
{
    [Obsolete("Old serialization format.")]
    private Rule? ruleDef;

    [JsonPropertyName("ruleDef")]
    [Obsolete("Old serialization format.")]
    public Rule? RuleDef
    {
        // Because this property is old we old want to read it and never to write it.
        set => ruleDef = value;
    }

    public void FromSource(Rule source)
    {
        SimpleMapper.Map(source, this);
    }

    public Rule ToSource()
    {
        var result = this;

#pragma warning disable CS0618 // Type or member is obsolete
        if (ruleDef != null)
        {
            // In previous versions, the actual rule was stored in a nested object.
            return ruleDef with
            {
                Id = Id,
                AppId = AppId,
                Created = Created,
                CreatedBy = CreatedBy,
                IsDeleted = IsDeleted,
                LastModified = LastModified,
                LastModifiedBy = LastModifiedBy,
                Version = Version,
            };
        }
#pragma warning restore CS0618 // Type or member is obsolete

        if (result.Trigger is IMigrated<RuleTrigger> migrated)
        {
            return this with { Trigger = migrated.Migrate() };
        }

        return this;
    }
}
