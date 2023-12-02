// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using System.Text.Json.Serialization;

namespace Squidex.Domain.Apps.Core.Schemas;

public sealed record ArrayField : RootField<ArrayFieldProperties>, IArrayField
{
    [JsonIgnore]
    public IReadOnlyList<NestedField> Fields
    {
        get => FieldCollection.Ordered;
    }

    [JsonIgnore]
    public IReadOnlyDictionary<long, NestedField> FieldsById
    {
        get => FieldCollection.ById;
    }

    [JsonIgnore]
    public IReadOnlyDictionary<string, NestedField> FieldsByName
    {
        get => FieldCollection.ByName;
    }

    public FieldCollection<NestedField> FieldCollection { get; init; } = FieldCollection<NestedField>.Empty;

    [Pure]
    public ArrayField DeleteField(long fieldId)
    {
        return Updatefields(f => f.Remove(fieldId));
    }

    [Pure]
    public ArrayField ReorderFields(List<long> ids)
    {
        return Updatefields(f => f.Reorder(ids));
    }

    [Pure]
    public ArrayField AddField(NestedField field)
    {
        return Updatefields(f => f.Add(field));
    }

    [Pure]
    public ArrayField UpdateField(long fieldId, Func<NestedField, NestedField> updater)
    {
        return Updatefields(f => f.Update(fieldId, updater));
    }

    private ArrayField Updatefields(Func<FieldCollection<NestedField>, FieldCollection<NestedField>> updater)
    {
        var newFields = updater(FieldCollection);

        if (ReferenceEquals(newFields, FieldCollection))
        {
            return this;
        }

        return this with { FieldCollection = newFields };
    }
}
