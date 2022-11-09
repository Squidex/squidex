// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas;

public static class Fields
{
    public static ArrayField Array(long id, string name, Partitioning partitioning,
        ArrayFieldProperties? properties = null, IFieldSettings? settings = null, params NestedField[] fields)
    {
        return new ArrayField(id, name, partitioning, fields, properties, settings);
    }

    public static RootField<AssetsFieldProperties> Assets(long id, string name, Partitioning partitioning,
        AssetsFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new RootField<AssetsFieldProperties>(id, name, partitioning, properties, settings);
    }

    public static RootField<BooleanFieldProperties> Boolean(long id, string name, Partitioning partitioning,
        BooleanFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new RootField<BooleanFieldProperties>(id, name, partitioning, properties, settings);
    }

    public static RootField<ComponentFieldProperties> Component(long id, string name, Partitioning partitioning,
        ComponentFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new RootField<ComponentFieldProperties>(id, name, partitioning, properties, settings);
    }

    public static RootField<ComponentsFieldProperties> Components(long id, string name, Partitioning partitioning,
        ComponentsFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new RootField<ComponentsFieldProperties>(id, name, partitioning, properties, settings);
    }

    public static RootField<DateTimeFieldProperties> DateTime(long id, string name, Partitioning partitioning,
        DateTimeFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new RootField<DateTimeFieldProperties>(id, name, partitioning, properties, settings);
    }

    public static RootField<GeolocationFieldProperties> Geolocation(long id, string name, Partitioning partitioning,
        GeolocationFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new RootField<GeolocationFieldProperties>(id, name, partitioning, properties, settings);
    }

    public static RootField<JsonFieldProperties> Json(long id, string name, Partitioning partitioning,
        JsonFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new RootField<JsonFieldProperties>(id, name, partitioning, properties, settings);
    }

    public static RootField<NumberFieldProperties> Number(long id, string name, Partitioning partitioning,
        NumberFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new RootField<NumberFieldProperties>(id, name, partitioning, properties, settings);
    }

    public static RootField<ReferencesFieldProperties> References(long id, string name, Partitioning partitioning,
        ReferencesFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new RootField<ReferencesFieldProperties>(id, name, partitioning, properties, settings);
    }

    public static RootField<StringFieldProperties> String(long id, string name, Partitioning partitioning,
        StringFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new RootField<StringFieldProperties>(id, name, partitioning, properties, settings);
    }

    public static RootField<TagsFieldProperties> Tags(long id, string name, Partitioning partitioning,
        TagsFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new RootField<TagsFieldProperties>(id, name, partitioning, properties, settings);
    }

    public static RootField<UIFieldProperties> UI(long id, string name, Partitioning partitioning,
        UIFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new RootField<UIFieldProperties>(id, name, partitioning, properties, settings);
    }

    public static NestedField<AssetsFieldProperties> Assets(long id, string name,
        AssetsFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new NestedField<AssetsFieldProperties>(id, name, properties, settings);
    }

    public static NestedField<BooleanFieldProperties> Boolean(long id, string name,
        BooleanFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new NestedField<BooleanFieldProperties>(id, name, properties, settings);
    }

    public static NestedField<ComponentFieldProperties> Component(long id, string name,
        ComponentFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new NestedField<ComponentFieldProperties>(id, name, properties, settings);
    }

    public static NestedField<ComponentsFieldProperties> Components(long id, string name,
        ComponentsFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new NestedField<ComponentsFieldProperties>(id, name, properties, settings);
    }

    public static NestedField<DateTimeFieldProperties> DateTime(long id, string name,
        DateTimeFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new NestedField<DateTimeFieldProperties>(id, name, properties, settings);
    }

    public static NestedField<GeolocationFieldProperties> Geolocation(long id, string name,
        GeolocationFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new NestedField<GeolocationFieldProperties>(id, name, properties, settings);
    }

    public static NestedField<JsonFieldProperties> Json(long id, string name,
        JsonFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new NestedField<JsonFieldProperties>(id, name, properties, settings);
    }

    public static NestedField<NumberFieldProperties> Number(long id, string name,
        NumberFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new NestedField<NumberFieldProperties>(id, name, properties, settings);
    }

    public static NestedField<ReferencesFieldProperties> References(long id, string name,
        ReferencesFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new NestedField<ReferencesFieldProperties>(id, name, properties, settings);
    }

    public static NestedField<StringFieldProperties> String(long id, string name,
        StringFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new NestedField<StringFieldProperties>(id, name, properties, settings);
    }

    public static NestedField<TagsFieldProperties> Tags(long id, string name,
        TagsFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new NestedField<TagsFieldProperties>(id, name, properties, settings);
    }

    public static NestedField<UIFieldProperties> UI(long id, string name,
        UIFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return new NestedField<UIFieldProperties>(id, name, properties, settings);
    }

    public static Schema AddArray(this Schema schema, long id, string name, Partitioning partitioning,
        Func<ArrayField, ArrayField>? handler = null, ArrayFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        var field = Array(id, name, partitioning, properties, settings);

        if (handler != null)
        {
            field = handler(field);
        }

        return schema.AddField(field);
    }

    public static Schema AddAssets(this Schema schema, long id, string name, Partitioning partitioning,
        AssetsFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return schema.AddField(Assets(id, name, partitioning, properties, settings));
    }

    public static Schema AddBoolean(this Schema schema, long id, string name, Partitioning partitioning,
        BooleanFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return schema.AddField(Boolean(id, name, partitioning, properties, settings));
    }

    public static Schema AddComponent(this Schema schema, long id, string name, Partitioning partitioning,
        ComponentFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return schema.AddField(Component(id, name, partitioning, properties, settings));
    }

    public static Schema AddComponents(this Schema schema, long id, string name, Partitioning partitioning,
        ComponentsFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return schema.AddField(Components(id, name, partitioning, properties, settings));
    }

    public static Schema AddDateTime(this Schema schema, long id, string name, Partitioning partitioning,
        DateTimeFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return schema.AddField(DateTime(id, name, partitioning, properties, settings));
    }

    public static Schema AddGeolocation(this Schema schema, long id, string name, Partitioning partitioning,
        GeolocationFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return schema.AddField(Geolocation(id, name, partitioning, properties, settings));
    }

    public static Schema AddJson(this Schema schema, long id, string name, Partitioning partitioning,
        JsonFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return schema.AddField(Json(id, name, partitioning, properties, settings));
    }

    public static Schema AddNumber(this Schema schema, long id, string name, Partitioning partitioning,
        NumberFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return schema.AddField(Number(id, name, partitioning, properties, settings));
    }

    public static Schema AddReferences(this Schema schema, long id, string name, Partitioning partitioning,
        ReferencesFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return schema.AddField(References(id, name, partitioning, properties, settings));
    }

    public static Schema AddString(this Schema schema, long id, string name, Partitioning partitioning,
        StringFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return schema.AddField(String(id, name, partitioning, properties, settings));
    }

    public static Schema AddTags(this Schema schema, long id, string name, Partitioning partitioning,
        TagsFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return schema.AddField(Tags(id, name, partitioning, properties, settings));
    }

    public static Schema AddUI(this Schema schema, long id, string name, Partitioning partitioning,
        UIFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return schema.AddField(UI(id, name, partitioning, properties, settings));
    }

    public static ArrayField AddAssets(this ArrayField field, long id, string name,
        AssetsFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return field.AddField(Assets(id, name, properties, settings));
    }

    public static ArrayField AddBoolean(this ArrayField field, long id, string name,
        BooleanFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return field.AddField(Boolean(id, name, properties, settings));
    }

    public static ArrayField AddComponent(this ArrayField field, long id, string name,
        ComponentFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return field.AddField(Component(id, name, properties, settings));
    }

    public static ArrayField AddComponents(this ArrayField field, long id, string name,
        ComponentsFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return field.AddField(Components(id, name, properties, settings));
    }

    public static ArrayField AddDateTime(this ArrayField field, long id, string name,
        DateTimeFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return field.AddField(DateTime(id, name, properties, settings));
    }

    public static ArrayField AddGeolocation(this ArrayField field, long id, string name,
        GeolocationFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return field.AddField(Geolocation(id, name, properties, settings));
    }

    public static ArrayField AddJson(this ArrayField field, long id, string name,
        JsonFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return field.AddField(Json(id, name, properties, settings));
    }

    public static ArrayField AddNumber(this ArrayField field, long id, string name,
        NumberFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return field.AddField(Number(id, name, properties, settings));
    }

    public static ArrayField AddReferences(this ArrayField field, long id, string name,
        ReferencesFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return field.AddField(References(id, name, properties, settings));
    }

    public static ArrayField AddString(this ArrayField field, long id, string name,
        StringFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return field.AddField(String(id, name, properties, settings));
    }

    public static ArrayField AddTags(this ArrayField field, long id, string name,
        TagsFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return field.AddField(Tags(id, name, properties, settings));
    }

    public static ArrayField AddUI(this ArrayField field, long id, string name,
        UIFieldProperties? properties = null, IFieldSettings? settings = null)
    {
        return field.AddField(UI(id, name, properties, settings));
    }
}
