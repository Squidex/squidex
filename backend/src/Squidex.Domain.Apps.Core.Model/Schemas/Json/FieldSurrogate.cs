// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas.Json;

public sealed class FieldSurrogate
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string Partitioning { get; set; }

    public bool IsHidden { get; set; }

    public bool IsLocked { get; set; }

    public bool IsDisabled { get; set; }

    public FieldProperties Properties { get; set; }

    public FieldSurrogate[]? Children { get; set; }

    public static FieldSurrogate FromSource(RootField source)
    {
        return new FieldSurrogate
        {
            Id = source.Id,
            Name = source.Name,
            Children = source is ArrayField array ? array.Fields.Select(FromSource).ToArray() : null,
            IsLocked = source.IsLocked,
            IsHidden = source.IsHidden,
            IsDisabled = source.IsDisabled,
            Partitioning = source.Partitioning.Key,
            Properties = source.RawProperties
        };
    }

    public static FieldSurrogate FromSource(NestedField source)
    {
        return new FieldSurrogate
        {
            Id = source.Id,
            Name = source.Name,
            IsLocked = source.IsLocked,
            IsHidden = source.IsHidden,
            IsDisabled = source.IsDisabled,
            Properties = source.RawProperties
        };
    }

    public RootField ToRootField()
    {
        var partitioning = Core.Partitioning.FromString(Partitioning);

        if (Properties is ArrayFieldProperties arrayProperties)
        {
            var nested = Children?.Select(n => n.ToNestedField()).ToArray() ?? [];

            return Fields.Array(Id, Name, partitioning, arrayProperties) with
            {
                FieldCollection = new FieldCollection<NestedField>(nested),
                IsLocked = IsLocked,
                IsHidden = IsHidden,
                IsDisabled = IsDisabled
            };
        }
        else
        {
            return Properties.CreateRootField(Id, Name, partitioning) with
            {
                IsLocked = IsLocked,
                IsHidden = IsHidden,
                IsDisabled = IsDisabled
            };
        }
    }

    public NestedField ToNestedField()
    {
        return Properties.CreateNestedField(Id, Name) with
        {
            IsLocked = IsLocked,
            IsHidden = IsHidden,
            IsDisabled = IsDisabled
        };
    }
}
