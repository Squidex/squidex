// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    internal static class ContentFields
    {
        public static readonly FieldType Id = new FieldType
        {
            Name = "id",
            ResolvedType = AllTypes.NonNullString,
            Resolver = EntityResolvers.Id,
            Description = "The id of the content."
        };

        public static readonly FieldType Version = new FieldType
        {
            Name = "version",
            ResolvedType = AllTypes.NonNullInt,
            Resolver = EntityResolvers.Version,
            Description = "The version of the content."
        };

        public static readonly FieldType Created = new FieldType
        {
            Name = "created",
            ResolvedType = AllTypes.NonNullDateTime,
            Resolver = EntityResolvers.Created,
            Description = "The date and time when the content has been created."
        };

        public static readonly FieldType CreatedBy = new FieldType
        {
            Name = "createdBy",
            ResolvedType = AllTypes.NonNullString,
            Resolver = EntityResolvers.CreatedBy,
            Description = "The user that has created the content."
        };

        public static readonly FieldType CreatedByUser = new FieldType
        {
            Name = "createdByUser",
            ResolvedType = UserGraphType.NonNull,
            Resolver = EntityResolvers.CreatedByUser,
            Description = "The full info of the user that has created the content."
        };

        public static readonly FieldType LastModified = new FieldType
        {
            Name = "lastModified",
            ResolvedType = AllTypes.NonNullDateTime,
            Resolver = EntityResolvers.LastModified,
            Description = "The date and time when the content has been modified last."
        };

        public static readonly FieldType LastModifiedBy = new FieldType
        {
            Name = "lastModifiedBy",
            ResolvedType = AllTypes.NonNullString,
            Resolver = EntityResolvers.LastModifiedBy,
            Description = "The user that has updated the content last."
        };

        public static readonly FieldType LastModifiedByUser = new FieldType
        {
            Name = "lastModifiedByUser",
            ResolvedType = UserGraphType.NonNull,
            Resolver = EntityResolvers.LastModifiedByUser,
            Description = "The full info of the user that has updated the content last."
        };

        public static readonly FieldType Status = new FieldType
        {
            Name = "status",
            ResolvedType = AllTypes.NonNullString,
            Resolver = Resolve(x => x.Status.ToString().ToUpperInvariant()),
            Description = "The status of the content."
        };

        public static readonly FieldType StatusColor = new FieldType
        {
            Name = "statusColor",
            ResolvedType = AllTypes.NonNullString,
            Resolver = Resolve(x => x.StatusColor),
            Description = "The status color of the content."
        };

        public static readonly FieldType NewStatus = new FieldType
        {
            Name = "newStatus",
            ResolvedType = AllTypes.String,
            Resolver = Resolve(x => x.NewStatus?.ToString().ToUpperInvariant()),
            Description = "The new status of the content."
        };

        public static readonly FieldType NewStatusColor = new FieldType
        {
            Name = "newStatusColor",
            ResolvedType = AllTypes.String,
            Resolver = Resolve(x => x.NewStatusColor),
            Description = "The new status color of the content."
        };

        public static readonly FieldType SchemaId = new FieldType
        {
            Name = "schemaId",
            ResolvedType = AllTypes.NonNullString,
            Resolver = Resolve(x => x[Component.Discriminator].ToString()),
            Description = "The id of the schema."
        };

        public static readonly FieldType Url = new FieldType
        {
            Name = "url",
            ResolvedType = AllTypes.NonNullString,
            Resolver = ContentResolvers.Url,
            Description = "The url to the content."
        };

        private static IFieldResolver Resolve<T>(Func<JsonObject, T> resolver)
        {
            return Resolvers.Sync(resolver);
        }

        private static IFieldResolver Resolve<T>(Func<IEnrichedContentEntity, T> resolver)
        {
            return Resolvers.Sync(resolver);
        }
    }
}
