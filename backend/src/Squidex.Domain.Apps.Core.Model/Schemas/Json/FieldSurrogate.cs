// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas.Json;

public sealed class FieldSurrogate : IFieldSettings
{
    public long Id { get; set; }

    public string Name { get; set; }

    public string Partitioning { get; set; }

    public bool IsHidden { get; set; }

    public bool IsLocked { get; set; }

    public bool IsDisabled { get; set; }

    public FieldProperties Properties { get; set; }

    public FieldSurrogate[]? Children { get; set; }

    public RootField ToField()
    {
        var partitioning = Core.Partitioning.FromString(Partitioning);

        if (Properties is ArrayFieldProperties arrayProperties)
        {
            var nested = Children?.Select(n => n.ToNestedField()).ToArray() ?? Array.Empty<NestedField>();

            return new ArrayField(Id, Name, partitioning, nested, arrayProperties, this);
        }
        else
        {
            return Properties.CreateRootField(Id, Name, partitioning, this);
        }
    }

    public NestedField ToNestedField()
    {
        return Properties.CreateNestedField(Id, Name, this);
    }
}