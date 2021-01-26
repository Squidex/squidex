﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    public static class ContentFields
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
            ResolvedType = AllTypes.NonNullDate,
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

        public static readonly FieldType LastModified = new FieldType
        {
            Name = "lastModified",
            ResolvedType = AllTypes.NonNullDate,
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

        public static readonly FieldType Status = new FieldType
        {
            Name = "status",
            ResolvedType = AllTypes.NonNullString,
            Resolver = Resolve(x => x.Status.ToString().ToUpperInvariant()),
            Description = "The the status of the content."
        };

        public static readonly FieldType StatusColor = new FieldType
        {
            Name = "statusColor",
            ResolvedType = AllTypes.NonNullString,
            Resolver = Resolve(x => x.StatusColor),
            Description = "The color status of the content."
        };

        private static IFieldResolver Resolve<T>(Func<IEnrichedContentEntity, T> resolver)
        {
            return Resolvers.Sync(resolver);
        }
    }
}
