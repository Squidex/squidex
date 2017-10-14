// ==========================================================================
//  BsonConverter.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using MongoDB.Bson;
using Newtonsoft.Json.Linq;

namespace Squidex.Infrastructure.MongoDb
{
    public static class BsonConverter
    {
        public static BsonDocument ToBson(this JObject source)
        {
            var result = new BsonDocument();

            foreach (var property in source)
            {
                result.Add(property.Key, property.Value.ToBson());
            }

            return result;
        }

        public static JObject ToJson(this BsonDocument source)
        {
            var result = new JObject();

            foreach (var property in source)
            {
                result.Add(property.Name, property.Value.ToJson());
            }

            return result;
        }

        public static BsonArray ToBson(this JArray source)
        {
            var result = new BsonArray();

            foreach (var item in source)
            {
                result.Add(item.ToBson());
            }

            return result;
        }

        public static JArray ToJson(this BsonArray source)
        {
            var result = new JArray();

            foreach (var item in source)
            {
                result.Add(item.ToJson());
            }

            return result;
        }

        public static BsonValue ToBson(this JToken source)
        {
            switch (source)
            {
                case JObject jObject:
                    return jObject.ToBson();
                case JArray jArray:
                    return jArray.ToBson();
                case JValue jValue:
                    return BsonValue.Create(jValue.Value);
            }

            throw new NotSupportedException($"Cannot convert {source.GetType()} to Bson.");
        }

        public static JToken ToJson(this BsonValue source)
        {
            switch (source.BsonType)
            {
                case BsonType.Document:
                    return source.AsBsonDocument.ToJson();
                case BsonType.Array:
                    return source.AsBsonArray.ToJson();
                case BsonType.Double:
                    return new JValue(source.AsDouble);
                case BsonType.String:
                    return new JValue(source.AsString);
                case BsonType.Boolean:
                    return new JValue(source.AsBoolean);
                case BsonType.DateTime:
                    return new JValue(source.ToUniversalTime());
                case BsonType.Int32:
                    return new JValue(source.AsInt32);
                case BsonType.Int64:
                    return new JValue(source.AsInt64);
                case BsonType.Decimal128:
                    return new JValue(source.AsDecimal);
                case BsonType.Null:
                    return JValue.CreateNull();
            }

            throw new NotSupportedException($"Cannot convert {source.GetType()} to Json.");
        }
    }
}