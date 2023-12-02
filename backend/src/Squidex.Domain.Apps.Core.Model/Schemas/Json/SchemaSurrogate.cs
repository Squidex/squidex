// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Reflection;

namespace Squidex.Domain.Apps.Core.Schemas.Json;

public sealed record SchemaSurrogate : Schema, ISurrogate<Schema>
{
    public new FieldSurrogate[] Fields { get; set; }

    public void FromSource(Schema source)
    {
        SimpleMapper.Map(source, this);

        Fields =
            source.Fields.Select(x =>
                new FieldSurrogate
                {
                    Id = x.Id,
                    Name = x.Name,
                    Children = CreateChildren(x),
                    IsHidden = x.IsHidden,
                    IsLocked = x.IsLocked,
                    IsDisabled = x.IsDisabled,
                    Partitioning = x.Partitioning.Key,
                    Properties = x.RawProperties
                }).ToArray();
    }

    private static FieldSurrogate[]? CreateChildren(IField field)
    {
        if (field is ArrayField arrayField)
        {
            return arrayField.Fields.Select(x =>
                new FieldSurrogate
                {
                    Id = x.Id,
                    Name = x.Name,
                    IsHidden = x.IsHidden,
                    IsLocked = x.IsLocked,
                    IsDisabled = x.IsDisabled,
                    Properties = x.RawProperties
                }).ToArray();
        }

        return null;
    }

    public Schema ToSource()
    {
        var schema = SimpleMapper.Map(this, new Schema());

        if (Fields != null)
        {
            schema = schema with
            {
                FieldCollection = new FieldCollection<RootField>(Fields.Select(x => x.ToField()).ToArray())
            };
        }

        return schema;
    }
}
