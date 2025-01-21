// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.MongoDb.TestHelpers;

public sealed class BsonAsDateTimeEntity<T>
{
    [BsonRepresentation(BsonType.DateTime)]
    public T Value { get; set; }
}

public sealed class BsonAsInt64Entity<T>
{
    [BsonRepresentation(BsonType.Int64)]
    public T Value { get; set; }
}

public sealed class BsonAsStringEntity<T>
{
    [BsonRepresentation(BsonType.String)]
    public T Value { get; set; }
}

public sealed class BsonAsDefaultEntity<T>
{
    public T Value { get; set; }
}
