// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1000 // Keywords should be spaced correctly

namespace Squidex.Domain.Apps.Core.Schemas;

public static class Fields
{
    public static ArrayField Array(long id, string name, Partitioning partitioning,
        ArrayFieldProperties? properties = null, params NestedField[] fields)
    {
        return new ArrayField { Id = id, Name = name, Partitioning = partitioning, Properties = properties ?? new(), FieldCollection = new FieldCollection<NestedField>(fields) };
    }

    public static RootField<AssetsFieldProperties> Assets(long id, string name, Partitioning partitioning,
        AssetsFieldProperties? properties = null)
    {
        return new RootField<AssetsFieldProperties> { Id = id, Name = name, Partitioning = partitioning, Properties = properties ?? new () };
    }

    public static RootField<BooleanFieldProperties> Boolean(long id, string name, Partitioning partitioning,
        BooleanFieldProperties? properties = null)
    {
        return new RootField<BooleanFieldProperties> { Id = id, Name = name, Partitioning = partitioning, Properties = properties ?? new() };
    }

    public static RootField<ComponentFieldProperties> Component(long id, string name, Partitioning partitioning,
        ComponentFieldProperties? properties = null)
    {
        return new RootField<ComponentFieldProperties> { Id = id, Name = name, Partitioning = partitioning, Properties = properties ?? new() };
    }

    public static RootField<ComponentsFieldProperties> Components(long id, string name, Partitioning partitioning,
        ComponentsFieldProperties? properties = null)
    {
        return new RootField<ComponentsFieldProperties> { Id = id, Name = name, Partitioning = partitioning, Properties = properties ?? new() };
    }

    public static RootField<DateTimeFieldProperties> DateTime(long id, string name, Partitioning partitioning,
        DateTimeFieldProperties? properties = null)
    {
        return new RootField<DateTimeFieldProperties> { Id = id, Name = name, Partitioning = partitioning, Properties = properties ?? new() };
    }

    public static RootField<GeolocationFieldProperties> Geolocation(long id, string name, Partitioning partitioning,
        GeolocationFieldProperties? properties = null)
    {
        return new RootField<GeolocationFieldProperties> { Id = id, Name = name, Partitioning = partitioning, Properties = properties ?? new() };
    }

    public static RootField<JsonFieldProperties> Json(long id, string name, Partitioning partitioning,
        JsonFieldProperties? properties = null)
    {
        return new RootField<JsonFieldProperties> { Id = id, Name = name, Partitioning = partitioning, Properties = properties ?? new() };
    }

    public static RootField<NumberFieldProperties> Number(long id, string name, Partitioning partitioning,
        NumberFieldProperties? properties = null)
    {
        return new RootField<NumberFieldProperties> { Id = id, Name = name, Partitioning = partitioning, Properties = properties ?? new() };
    }

    public static RootField<ReferencesFieldProperties> References(long id, string name, Partitioning partitioning,
        ReferencesFieldProperties? properties = null)
    {
        return new RootField<ReferencesFieldProperties> { Id = id, Name = name, Partitioning = partitioning, Properties = properties ?? new() };
    }

    public static RootField<RichTextFieldProperties> RichText(long id, string name, Partitioning partitioning,
        RichTextFieldProperties? properties = null)
    {
        return new RootField<RichTextFieldProperties> { Id = id, Name = name, Partitioning = partitioning, Properties = properties ?? new() };
    }

    public static RootField<StringFieldProperties> String(long id, string name, Partitioning partitioning,
        StringFieldProperties? properties = null)
    {
        return new RootField<StringFieldProperties> { Id = id, Name = name, Partitioning = partitioning, Properties = properties ?? new() };
    }

    public static RootField<TagsFieldProperties> Tags(long id, string name, Partitioning partitioning,
        TagsFieldProperties? properties = null)
    {
        return new RootField<TagsFieldProperties> { Id = id, Name = name, Partitioning = partitioning, Properties = properties ?? new() };
    }

    public static RootField<UIFieldProperties> UI(long id, string name, Partitioning partitioning,
        UIFieldProperties? properties = null)
    {
        return new RootField<UIFieldProperties> { Id = id, Name = name, Partitioning = partitioning, Properties = properties ?? new() };
    }

    public static NestedField<AssetsFieldProperties> Assets(long id, string name,
        AssetsFieldProperties? properties = null)
    {
        return new NestedField<AssetsFieldProperties> { Id = id, Name = name, Properties = properties ?? new() };
    }

    public static NestedField<BooleanFieldProperties> Boolean(long id, string name,
        BooleanFieldProperties? properties = null)
    {
        return new NestedField<BooleanFieldProperties> { Id = id, Name = name, Properties = properties ?? new() };
    }

    public static NestedField<ComponentFieldProperties> Component(long id, string name,
        ComponentFieldProperties? properties = null)
    {
        return new NestedField<ComponentFieldProperties> { Id = id, Name = name, Properties = properties ?? new() };
    }

    public static NestedField<ComponentsFieldProperties> Components(long id, string name,
        ComponentsFieldProperties? properties = null)
    {
        return new NestedField<ComponentsFieldProperties> { Id = id, Name = name, Properties = properties ?? new() };
    }

    public static NestedField<DateTimeFieldProperties> DateTime(long id, string name,
        DateTimeFieldProperties? properties = null)
    {
        return new NestedField<DateTimeFieldProperties> { Id = id, Name = name, Properties = properties ?? new() };
    }

    public static NestedField<GeolocationFieldProperties> Geolocation(long id, string name,
        GeolocationFieldProperties? properties = null)
    {
        return new NestedField<GeolocationFieldProperties> { Id = id, Name = name, Properties = properties ?? new() };
    }

    public static NestedField<JsonFieldProperties> Json(long id, string name,
        JsonFieldProperties? properties = null)
    {
        return new NestedField<JsonFieldProperties> { Id = id, Name = name, Properties = properties ?? new() };
    }

    public static NestedField<NumberFieldProperties> Number(long id, string name,
        NumberFieldProperties? properties = null)
    {
        return new NestedField<NumberFieldProperties> { Id = id, Name = name, Properties = properties ?? new() };
    }

    public static NestedField<ReferencesFieldProperties> References(long id, string name,
        ReferencesFieldProperties? properties = null)
    {
        return new NestedField<ReferencesFieldProperties> { Id = id, Name = name, Properties = properties ?? new() };
    }

    public static NestedField<RichTextFieldProperties> RichText(long id, string name,
        RichTextFieldProperties? properties = null)
    {
        return new NestedField<RichTextFieldProperties> { Id = id, Name = name, Properties = properties ?? new() };
    }

    public static NestedField<StringFieldProperties> String(long id, string name,
        StringFieldProperties? properties = null)
    {
        return new NestedField<StringFieldProperties> { Id = id, Name = name, Properties = properties ?? new() };
    }

    public static NestedField<TagsFieldProperties> Tags(long id, string name,
        TagsFieldProperties? properties = null)
    {
        return new NestedField<TagsFieldProperties> { Id = id, Name = name, Properties = properties ?? new() };
    }

    public static NestedField<UIFieldProperties> UI(long id, string name,
        UIFieldProperties? properties = null)
    {
        return new NestedField<UIFieldProperties> { Id = id, Name = name, Properties = properties ?? new() };
    }

    public static Schema AddArray(this Schema schema, long id, string name, Partitioning partitioning,
        Func<ArrayField, ArrayField>? handler = null, ArrayFieldProperties? properties = null)
    {
        var field = Array(id, name, partitioning, properties);

        if (handler != null)
        {
            field = handler(field);
        }

        return schema.AddField(field);
    }

    public static Schema AddAssets(this Schema schema, long id, string name, Partitioning partitioning,
        AssetsFieldProperties? properties = null)
    {
        return schema.AddField(Assets(id, name, partitioning, properties));
    }

    public static Schema AddBoolean(this Schema schema, long id, string name, Partitioning partitioning,
        BooleanFieldProperties? properties = null)
    {
        return schema.AddField(Boolean(id, name, partitioning, properties));
    }

    public static Schema AddComponent(this Schema schema, long id, string name, Partitioning partitioning,
        ComponentFieldProperties? properties = null)
    {
        return schema.AddField(Component(id, name, partitioning, properties));
    }

    public static Schema AddComponents(this Schema schema, long id, string name, Partitioning partitioning,
        ComponentsFieldProperties? properties = null)
    {
        return schema.AddField(Components(id, name, partitioning, properties));
    }

    public static Schema AddDateTime(this Schema schema, long id, string name, Partitioning partitioning,
        DateTimeFieldProperties? properties = null)
    {
        return schema.AddField(DateTime(id, name, partitioning, properties));
    }

    public static Schema AddGeolocation(this Schema schema, long id, string name, Partitioning partitioning,
        GeolocationFieldProperties? properties = null)
    {
        return schema.AddField(Geolocation(id, name, partitioning, properties));
    }

    public static Schema AddJson(this Schema schema, long id, string name, Partitioning partitioning,
        JsonFieldProperties? properties = null)
    {
        return schema.AddField(Json(id, name, partitioning, properties));
    }

    public static Schema AddNumber(this Schema schema, long id, string name, Partitioning partitioning,
        NumberFieldProperties? properties = null)
    {
        return schema.AddField(Number(id, name, partitioning, properties));
    }

    public static Schema AddReferences(this Schema schema, long id, string name, Partitioning partitioning,
        ReferencesFieldProperties? properties = null)
    {
        return schema.AddField(References(id, name, partitioning, properties));
    }

    public static Schema AddRichText(this Schema schema, long id, string name, Partitioning partitioning,
        RichTextFieldProperties? properties = null)
    {
        return schema.AddField(RichText(id, name, partitioning, properties));
    }

    public static Schema AddString(this Schema schema, long id, string name, Partitioning partitioning,
        StringFieldProperties? properties = null)
    {
        return schema.AddField(String(id, name, partitioning, properties));
    }

    public static Schema AddTags(this Schema schema, long id, string name, Partitioning partitioning,
        TagsFieldProperties? properties = null)
    {
        return schema.AddField(Tags(id, name, partitioning, properties));
    }

    public static Schema AddUI(this Schema schema, long id, string name, Partitioning partitioning,
        UIFieldProperties? properties = null)
    {
        return schema.AddField(UI(id, name, partitioning, properties));
    }

    public static ArrayField AddAssets(this ArrayField field, long id, string name,
        AssetsFieldProperties? properties = null)
    {
        return field.AddField(Assets(id, name, properties));
    }

    public static ArrayField AddBoolean(this ArrayField field, long id, string name,
        BooleanFieldProperties? properties = null)
    {
        return field.AddField(Boolean(id, name, properties));
    }

    public static ArrayField AddComponent(this ArrayField field, long id, string name,
        ComponentFieldProperties? properties = null)
    {
        return field.AddField(Component(id, name, properties));
    }

    public static ArrayField AddComponents(this ArrayField field, long id, string name,
        ComponentsFieldProperties? properties = null)
    {
        return field.AddField(Components(id, name, properties));
    }

    public static ArrayField AddDateTime(this ArrayField field, long id, string name,
        DateTimeFieldProperties? properties = null)
    {
        return field.AddField(DateTime(id, name, properties));
    }

    public static ArrayField AddGeolocation(this ArrayField field, long id, string name,
        GeolocationFieldProperties? properties = null)
    {
        return field.AddField(Geolocation(id, name, properties));
    }

    public static ArrayField AddJson(this ArrayField field, long id, string name,
        JsonFieldProperties? properties = null)
    {
        return field.AddField(Json(id, name, properties));
    }

    public static ArrayField AddNumber(this ArrayField field, long id, string name,
        NumberFieldProperties? properties = null)
    {
        return field.AddField(Number(id, name, properties));
    }

    public static ArrayField AddReferences(this ArrayField field, long id, string name,
        ReferencesFieldProperties? properties = null)
    {
        return field.AddField(References(id, name, properties));
    }

    public static ArrayField AddRichText(this ArrayField field, long id, string name,
        RichTextFieldProperties? properties = null)
    {
        return field.AddField(RichText(id, name, properties));
    }

    public static ArrayField AddString(this ArrayField field, long id, string name,
        StringFieldProperties? properties = null)
    {
        return field.AddField(String(id, name, properties));
    }

    public static ArrayField AddTags(this ArrayField field, long id, string name,
        TagsFieldProperties? properties = null)
    {
        return field.AddField(Tags(id, name, properties));
    }

    public static ArrayField AddUI(this ArrayField field, long id, string name,
        UIFieldProperties? properties = null)
    {
        return field.AddField(UI(id, name, properties));
    }
}
