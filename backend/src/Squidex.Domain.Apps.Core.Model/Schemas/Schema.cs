// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.Contracts;
using System.Text.Json.Serialization;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Core.Schemas;

public record Schema : Entity
{
    public NamedId<DomainId> AppId { get; init; }

    public SchemaType Type { get; init; }

    public string Name { get; init; }

    public string? Category { get; init; }

    public bool IsPublished { get; init; }

    public bool IsDeleted { get; init; }

    public FieldCollection<RootField> FieldCollection { get; init; } = FieldCollection<RootField>.Empty;

    public FieldRules FieldRules { get; init; } = FieldRules.Empty;

    public FieldNames FieldsInLists { get; init; } = FieldNames.Empty;

    public FieldNames FieldsInReferences { get; init; } = FieldNames.Empty;

    public SchemaScripts Scripts { get; init; } = new SchemaScripts();

    public SchemaProperties Properties { get; init; } = new SchemaProperties();

    public ReadonlyDictionary<string, string> PreviewUrls { get; init; } = ReadonlyDictionary.Empty<string, string>();

    public long TotalFields { get; init; }

    [JsonIgnore]
    public IReadOnlyList<RootField> Fields
    {
        get => FieldCollection.Ordered;
    }

    [JsonIgnore]
    public IReadOnlyDictionary<long, RootField> FieldsById
    {
        get => FieldCollection.ById;
    }

    [JsonIgnore]
    public IReadOnlyDictionary<string, RootField> FieldsByName
    {
        get => FieldCollection.ByName;
    }

    [Pure]
    public Schema Update(SchemaProperties? properties)
    {
        properties ??= new SchemaProperties();

        if (Properties.Equals(properties))
        {
            return this;
        }

        return this with { Properties = properties };
    }

    [Pure]
    public Schema SetScripts(SchemaScripts? scripts)
    {
        scripts ??= new SchemaScripts();

        if (Scripts.Equals(scripts))
        {
            return this;
        }

        return this with { Scripts = scripts };
    }

    [Pure]
    public Schema SetFieldsInLists(FieldNames? names)
    {
        names ??= FieldNames.Empty;

        if (FieldsInLists.SequenceEqual(names))
        {
            return this;
        }

        return this with { FieldsInLists = names };
    }

    [Pure]
    public Schema SetFieldsInReferences(FieldNames? names)
    {
        names ??= FieldNames.Empty;

        if (FieldsInReferences.SequenceEqual(names))
        {
            return this;
        }

        return this with { FieldsInReferences = names };
    }

    [Pure]
    public Schema SetFieldRules(FieldRules? rules)
    {
        rules ??= FieldRules.Empty;

        if (FieldRules.Equals(rules))
        {
            return this;
        }

        return this with { FieldRules = rules };
    }

    [Pure]
    public Schema Publish()
    {
        if (IsPublished)
        {
            return this;
        }

        return this with { IsPublished = true };
    }

    [Pure]
    public Schema Unpublish()
    {
        if (!IsPublished)
        {
            return this;
        }

        return this with { IsPublished = false };
    }

    [Pure]
    public Schema ChangeCategory(string? category)
    {
        if (string.Equals(Category, category, StringComparison.Ordinal))
        {
            return this;
        }

        return this with { Category = category };
    }

    [Pure]
    public Schema SetPreviewUrls(ReadonlyDictionary<string, string>? previewUrls)
    {
        previewUrls ??= ReadonlyDictionary.Empty<string, string>();

        if (PreviewUrls.Equals(previewUrls))
        {
            return this;
        }

        return this with { PreviewUrls = previewUrls };
    }

    [Pure]
    public Schema DeleteField(long fieldId)
    {
        if (!FieldsById.TryGetValue(fieldId, out var field))
        {
            return this;
        }

        return this with
        {
            FieldCollection = FieldCollection.Remove(fieldId),
            FieldsInLists = FieldsInLists.Remove(field.Name),
            FieldsInReferences = FieldsInReferences.Remove(field.Name)
        };
    }

    [Pure]
    public Schema ReorderFields(List<long> ids)
    {
        return UpdateFields(f => f.Reorder(ids));
    }

    [Pure]
    public Schema AddField(RootField field)
    {
        return UpdateFields(f => f.Add(field));
    }

    [Pure]
    public Schema UpdateField(long fieldId, Func<RootField, RootField> updater)
    {
        return UpdateFields(f => f.Update(fieldId, updater));
    }

    private Schema UpdateFields(Func<FieldCollection<RootField>, FieldCollection<RootField>> updater)
    {
        var newFields = updater(FieldCollection);

        if (ReferenceEquals(newFields, FieldCollection))
        {
            return this;
        }

        return this with { FieldCollection = newFields };
    }
}
