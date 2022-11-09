// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Infrastructure.MongoDb;

public static class Entities
{
    public sealed class DateTimeEntity<T>
    {
        [BsonRepresentation(BsonType.DateTime)]
        public T Value { get; set; }
    }

    public sealed class Int64Entity<T>
    {
        [BsonRepresentation(BsonType.Int64)]
        public T Value { get; set; }
    }

    public sealed class Int32Entity<T>
    {
        [BsonRepresentation(BsonType.Int32)]
        public T Value { get; set; }
    }

    public sealed class StringEntity<T>
    {
        [BsonRepresentation(BsonType.String)]
        public T Value { get; set; }
    }

    public sealed class BinaryEntity<T>
    {
        [BsonRepresentation(BsonType.Binary)]
        public T Value { get; set; }
    }

    public sealed class DefaultEntity<T>
    {
        public T Value { get; set; }
    }
}
