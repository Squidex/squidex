// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Schemas
{
    public static class Fields
    {
        public static Field<AssetsFieldProperties> Assets(long id, string name, Partitioning partitioning, AssetsFieldProperties properties = null)
        {
            return new Field<AssetsFieldProperties>(id, name, partitioning, properties ?? new AssetsFieldProperties());
        }

        public static Field<BooleanFieldProperties> Boolean(long id, string name, Partitioning partitioning, BooleanFieldProperties properties = null)
        {
            return new Field<BooleanFieldProperties>(id, name, partitioning, properties ?? new BooleanFieldProperties());
        }

        public static Field<DateTimeFieldProperties> DateTime(long id, string name, Partitioning partitioning, DateTimeFieldProperties properties = null)
        {
            return new Field<DateTimeFieldProperties>(id, name, partitioning, properties ?? new DateTimeFieldProperties());
        }

        public static Field<GeolocationFieldProperties> Geolocation(long id, string name, Partitioning partitioning, GeolocationFieldProperties properties = null)
        {
            return new Field<GeolocationFieldProperties>(id, name, partitioning, properties ?? new GeolocationFieldProperties());
        }

        public static Field<JsonFieldProperties> Json(long id, string name, Partitioning partitioning, JsonFieldProperties properties = null)
        {
            return new Field<JsonFieldProperties>(id, name, partitioning, properties ?? new JsonFieldProperties());
        }

        public static Field<NumberFieldProperties> Number(long id, string name, Partitioning partitioning, NumberFieldProperties properties = null)
        {
            return new Field<NumberFieldProperties>(id, name, partitioning, properties ?? new NumberFieldProperties());
        }

        public static Field<ReferencesFieldProperties> References(long id, string name, Partitioning partitioning, ReferencesFieldProperties properties = null)
        {
            return new Field<ReferencesFieldProperties>(id, name, partitioning, properties ?? new ReferencesFieldProperties());
        }

        public static Field<StringFieldProperties> String(long id, string name, Partitioning partitioning, StringFieldProperties properties = null)
        {
            return new Field<StringFieldProperties>(id, name, partitioning, properties ?? new StringFieldProperties());
        }

        public static Field<TagsFieldProperties> Tags(long id, string name, Partitioning partitioning, TagsFieldProperties properties = null)
        {
            return new Field<TagsFieldProperties>(id, name, partitioning, properties ?? new TagsFieldProperties());
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
    }
}
