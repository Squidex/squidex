// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;

namespace Squidex.Domain.Apps.Core.Schemas
{
    public static class Fields
    {
        public static RootField<ArrayFieldProperties> Array(long id, string name, Partitioning partitioning, params NestedField[] fields)
        {
            var result = new ArrayField(id, name, partitioning, new ArrayFieldProperties());

            if (fields != null)
            {
                foreach (var field in fields)
                {
                    result = result.AddField(field);
                }
            }

            return result;
        }

        public static ArrayField Array(long id, string name, Partitioning partitioning, ArrayFieldProperties properties = null)
        {
            return new ArrayField(id, name, partitioning, properties ?? new ArrayFieldProperties());
        }

        public static RootField<AssetsFieldProperties> Assets(long id, string name, Partitioning partitioning, AssetsFieldProperties properties = null)
        {
            return new RootField<AssetsFieldProperties>(id, name, partitioning, properties ?? new AssetsFieldProperties());
        }

        public static RootField<BooleanFieldProperties> Boolean(long id, string name, Partitioning partitioning, BooleanFieldProperties properties = null)
        {
            return new RootField<BooleanFieldProperties>(id, name, partitioning, properties ?? new BooleanFieldProperties());
        }

        public static RootField<DateTimeFieldProperties> DateTime(long id, string name, Partitioning partitioning, DateTimeFieldProperties properties = null)
        {
            return new RootField<DateTimeFieldProperties>(id, name, partitioning, properties ?? new DateTimeFieldProperties());
        }

        public static RootField<GeolocationFieldProperties> Geolocation(long id, string name, Partitioning partitioning, GeolocationFieldProperties properties = null)
        {
            return new RootField<GeolocationFieldProperties>(id, name, partitioning, properties ?? new GeolocationFieldProperties());
        }

        public static RootField<JsonFieldProperties> Json(long id, string name, Partitioning partitioning, JsonFieldProperties properties = null)
        {
            return new RootField<JsonFieldProperties>(id, name, partitioning, properties ?? new JsonFieldProperties());
        }

        public static RootField<NumberFieldProperties> Number(long id, string name, Partitioning partitioning, NumberFieldProperties properties = null)
        {
            return new RootField<NumberFieldProperties>(id, name, partitioning, properties ?? new NumberFieldProperties());
        }

        public static RootField<ReferencesFieldProperties> References(long id, string name, Partitioning partitioning, ReferencesFieldProperties properties = null)
        {
            return new RootField<ReferencesFieldProperties>(id, name, partitioning, properties ?? new ReferencesFieldProperties());
        }

        public static RootField<StringFieldProperties> String(long id, string name, Partitioning partitioning, StringFieldProperties properties = null)
        {
            return new RootField<StringFieldProperties>(id, name, partitioning, properties ?? new StringFieldProperties());
        }

        public static RootField<TagsFieldProperties> Tags(long id, string name, Partitioning partitioning, TagsFieldProperties properties = null)
        {
            return new RootField<TagsFieldProperties>(id, name, partitioning, properties ?? new TagsFieldProperties());
        }

        public static NestedField<AssetsFieldProperties> Assets(long id, string name, AssetsFieldProperties properties = null)
        {
            return new NestedField<AssetsFieldProperties>(id, name, properties ?? new AssetsFieldProperties());
        }

        public static NestedField<BooleanFieldProperties> Boolean(long id, string name, BooleanFieldProperties properties = null)
        {
            return new NestedField<BooleanFieldProperties>(id, name, properties ?? new BooleanFieldProperties());
        }

        public static NestedField<DateTimeFieldProperties> DateTime(long id, string name, DateTimeFieldProperties properties = null)
        {
            return new NestedField<DateTimeFieldProperties>(id, name, properties ?? new DateTimeFieldProperties());
        }

        public static NestedField<GeolocationFieldProperties> Geolocation(long id, string name, GeolocationFieldProperties properties = null)
        {
            return new NestedField<GeolocationFieldProperties>(id, name, properties ?? new GeolocationFieldProperties());
        }

        public static NestedField<JsonFieldProperties> Json(long id, string name, JsonFieldProperties properties = null)
        {
            return new NestedField<JsonFieldProperties>(id, name, properties ?? new JsonFieldProperties());
        }

        public static NestedField<NumberFieldProperties> Number(long id, string name, NumberFieldProperties properties = null)
        {
            return new NestedField<NumberFieldProperties>(id, name, properties ?? new NumberFieldProperties());
        }

        public static NestedField<ReferencesFieldProperties> References(long id, string name, ReferencesFieldProperties properties = null)
        {
            return new NestedField<ReferencesFieldProperties>(id, name, properties ?? new ReferencesFieldProperties());
        }

        public static NestedField<StringFieldProperties> String(long id, string name, StringFieldProperties properties = null)
        {
            return new NestedField<StringFieldProperties>(id, name, properties ?? new StringFieldProperties());
        }

        public static NestedField<TagsFieldProperties> Tags(long id, string name, TagsFieldProperties properties = null)
        {
            return new NestedField<TagsFieldProperties>(id, name, properties ?? new TagsFieldProperties());
        }

        public static Schema AddArray(this Schema schema, long id, string name, Partitioning partitioning, Func<ArrayField, ArrayField> handler, ArrayFieldProperties properties = null)
        {
            var field = Array(id, name, partitioning, properties);

            if (handler != null)
            {
                field = handler(field);
            }

            return schema.AddField(field);
        }

        public static Schema AddAssets(this Schema schema, long id, string name, Partitioning partitioning, AssetsFieldProperties properties = null)
        {
            return schema.AddField(Assets(id, name, partitioning, properties));
        }

        public static Schema AddBoolean(this Schema schema, long id, string name, Partitioning partitioning, BooleanFieldProperties properties = null)
        {
            return schema.AddField(Boolean(id, name, partitioning, properties));
        }

        public static Schema AddDateTime(this Schema schema, long id, string name, Partitioning partitioning, DateTimeFieldProperties properties = null)
        {
            return schema.AddField(DateTime(id, name, partitioning, properties));
        }

        public static Schema AddGeolocation(this Schema schema, long id, string name, Partitioning partitioning, GeolocationFieldProperties properties = null)
        {
            return schema.AddField(Geolocation(id, name, partitioning, properties));
        }

        public static Schema AddJson(this Schema schema, long id, string name, Partitioning partitioning, JsonFieldProperties properties = null)
        {
            return schema.AddField(Json(id, name, partitioning, properties));
        }

        public static Schema AddNumber(this Schema schema, long id, string name, Partitioning partitioning, NumberFieldProperties properties = null)
        {
            return schema.AddField(Number(id, name, partitioning, properties));
        }

        public static Schema AddReferences(this Schema schema, long id, string name, Partitioning partitioning, ReferencesFieldProperties properties = null)
        {
            return schema.AddField(References(id, name, partitioning, properties));
        }

        public static Schema AddString(this Schema schema, long id, string name, Partitioning partitioning, StringFieldProperties properties = null)
        {
            return schema.AddField(String(id, name, partitioning, properties));
        }

        public static Schema AddTags(this Schema schema, long id, string name, Partitioning partitioning, TagsFieldProperties properties = null)
        {
            return schema.AddField(Tags(id, name, partitioning, properties));
        }

        public static ArrayField AddAssets(this ArrayField field, long id, string name, AssetsFieldProperties properties = null)
        {
            return field.AddField(Assets(id, name, properties));
        }

        public static ArrayField AddBoolean(this ArrayField field, long id, string name, BooleanFieldProperties properties = null)
        {
            return field.AddField(Boolean(id, name, properties));
        }

        public static ArrayField AddDateTime(this ArrayField field, long id, string name, DateTimeFieldProperties properties = null)
        {
            return field.AddField(DateTime(id, name, properties));
        }

        public static ArrayField AddGeolocation(this ArrayField field, long id, string name, GeolocationFieldProperties properties = null)
        {
            return field.AddField(Geolocation(id, name, properties));
        }

        public static ArrayField AddJson(this ArrayField field, long id, string name, JsonFieldProperties properties = null)
        {
            return field.AddField(Json(id, name, properties));
        }

        public static ArrayField AddNumber(this ArrayField field, long id, string name, NumberFieldProperties properties = null)
        {
            return field.AddField(Number(id, name, properties));
        }

        public static ArrayField AddReferences(this ArrayField field, long id, string name, ReferencesFieldProperties properties = null)
        {
            return field.AddField(References(id, name, properties));
        }

        public static ArrayField AddString(this ArrayField field, long id, string name, StringFieldProperties properties = null)
        {
            return field.AddField(String(id, name, properties));
        }

        public static ArrayField AddTags(this ArrayField field, long id, string name, TagsFieldProperties properties = null)
        {
            return field.AddField(Tags(id, name, properties));
        }
    }
}
