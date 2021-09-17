// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
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
            Description = FieldDescriptions.EntityId
        };

        public static readonly FieldType Version = new FieldType
        {
            Name = "version",
            ResolvedType = AllTypes.NonNullInt,
            Resolver = EntityResolvers.Version,
            Description = FieldDescriptions.EntityVersion
        };

        public static readonly FieldType Created = new FieldType
        {
            Name = "created",
            ResolvedType = AllTypes.NonNullDateTime,
            Resolver = EntityResolvers.Created,
            Description = FieldDescriptions.EntityCreated
        };

        public static readonly FieldType CreatedBy = new FieldType
        {
            Name = "createdBy",
            ResolvedType = AllTypes.NonNullString,
            Resolver = EntityResolvers.CreatedBy,
            Description = FieldDescriptions.EntityCreatedBy
        };

        public static readonly FieldType CreatedByUser = new FieldType
        {
            Name = "createdByUser",
            ResolvedType = UserGraphType.NonNull,
            Resolver = EntityResolvers.CreatedByUser,
            Description = FieldDescriptions.EntityCreatedBy
        };

        public static readonly FieldType LastModified = new FieldType
        {
            Name = "lastModified",
            ResolvedType = AllTypes.NonNullDateTime,
            Resolver = EntityResolvers.LastModified,
            Description = FieldDescriptions.EntityLastModified
        };

        public static readonly FieldType LastModifiedBy = new FieldType
        {
            Name = "lastModifiedBy",
            ResolvedType = AllTypes.NonNullString,
            Resolver = EntityResolvers.LastModifiedBy,
            Description = FieldDescriptions.EntityLastModifiedBy
        };

        public static readonly FieldType LastModifiedByUser = new FieldType
        {
            Name = "lastModifiedByUser",
            ResolvedType = UserGraphType.NonNull,
            Resolver = EntityResolvers.LastModifiedByUser,
            Description = FieldDescriptions.EntityLastModifiedBy
        };

        public static readonly FieldType Status = new FieldType
        {
            Name = "status",
            ResolvedType = AllTypes.NonNullString,
            Resolver = Resolve(x => x.Status.ToString().ToUpperInvariant()),
            Description = FieldDescriptions.ContentStatus
        };

        public static readonly FieldType StatusColor = new FieldType
        {
            Name = "statusColor",
            ResolvedType = AllTypes.NonNullString,
            Resolver = Resolve(x => x.StatusColor),
            Description = FieldDescriptions.ContentStatusColor
        };

        public static readonly FieldType NewStatus = new FieldType
        {
            Name = "newStatus",
            ResolvedType = AllTypes.String,
            Resolver = Resolve(x => x.NewStatus?.ToString().ToUpperInvariant()),
            Description = FieldDescriptions.ContentNewStatus
        };

        public static readonly FieldType NewStatusColor = new FieldType
        {
            Name = "newStatusColor",
            ResolvedType = AllTypes.String,
            Resolver = Resolve(x => x.NewStatusColor),
            Description = FieldDescriptions.ContentStatusColor
        };

        public static readonly FieldType SchemaId = new FieldType
        {
            Name = "schemaId",
            ResolvedType = AllTypes.NonNullString,
            Resolver = Resolve(x => x[Component.Discriminator].ToString()),
            Description = FieldDescriptions.ContentSchemaId
        };

        public static readonly FieldType Url = new FieldType
        {
            Name = "url",
            ResolvedType = AllTypes.NonNullString,
            Resolver = ContentResolvers.Url,
            Description = FieldDescriptions.ContentUrl
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
