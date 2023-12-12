// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json.Serialization;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.System;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Schemas.Json;

[JsonRename(nameof(FieldCollection), "fields")]
public record SchemaSurrogate : Schema, ISurrogate<Schema>
{
    [Obsolete("Old serialization format.")]
    private Schema? schemaDef;

    [JsonPropertyName("schemaDef")]
    [Obsolete("Old serialization format.")]
    public Schema? SchemaDef
    {
        // Because this property is old we old want to read it and never to write it.
        set => schemaDef = value;
    }

    public void FromSource(Schema source)
    {
        SimpleMapper.Map(source, this);
    }

    public Schema ToSource()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        if (schemaDef != null)
        {
            // In previous versions, the actual schema was stored in a nested object.
            return schemaDef with
            {
                Id = Id,
                AppId = AppId,
                Created = Created,
                CreatedBy = CreatedBy,
                IsDeleted = IsDeleted,
                LastModified = LastModified,
                LastModifiedBy = LastModifiedBy,
                SchemaFieldsTotal = SchemaFieldsTotal,
                Version = Version,
            };
        }
#pragma warning restore CS0618 // Type or member is obsolete

        return SimpleMapper.Map(this, new Schema());
    }
}
