﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using MongoDB.Bson.Serialization;

namespace Squidex.Domain.Apps.Entities.MongoDb.Contents
{
    internal static class Fields
    {
        private static readonly Lazy<string> IdField = new Lazy<string>(GetIdField);
        private static readonly Lazy<string> SchemaIdField = new Lazy<string>(GetSchemaIdField);

        public static string Id => IdField.Value;

        public static string SchemaId => SchemaIdField.Value;

        private static string GetIdField()
        {
            return BsonClassMap.LookupClassMap(typeof(MongoContentEntity)).GetMemberMap(nameof(MongoContentEntity.Id)).ElementName;
        }

        private static string GetSchemaIdField()
        {
            return BsonClassMap.LookupClassMap(typeof(MongoContentEntity)).GetMemberMap(nameof(MongoContentEntity.IndexedSchemaId)).ElementName;
        }
    }
}
