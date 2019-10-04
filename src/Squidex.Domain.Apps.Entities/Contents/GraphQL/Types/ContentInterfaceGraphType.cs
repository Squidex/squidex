// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class ContentInterfaceGraphType : InterfaceGraphType<IContentEntity>
    {
        public ContentInterfaceGraphType()
        {
            Name = $"Content";

            AddField(new FieldType
            {
                Name = "id",
                ResolvedType = AllTypes.NonNullGuid,
                Resolver = Resolve(x => x.Id),
                Description = $"The id of the content."
            });

            AddField(new FieldType
            {
                Name = "version",
                ResolvedType = AllTypes.NonNullInt,
                Resolver = Resolve(x => x.Version),
                Description = $"The version of the content."
            });

            AddField(new FieldType
            {
                Name = "created",
                ResolvedType = AllTypes.NonNullDate,
                Resolver = Resolve(x => x.Created),
                Description = $"The date and time when the content has been created."
            });

            AddField(new FieldType
            {
                Name = "createdBy",
                ResolvedType = AllTypes.NonNullString,
                Resolver = Resolve(x => x.CreatedBy.ToString()),
                Description = $"The user that has created the content."
            });

            AddField(new FieldType
            {
                Name = "lastModified",
                ResolvedType = AllTypes.NonNullDate,
                Resolver = Resolve(x => x.LastModified),
                Description = $"The date and time when the content has been modified last."
            });

            AddField(new FieldType
            {
                Name = "lastModifiedBy",
                ResolvedType = AllTypes.NonNullString,
                Resolver = Resolve(x => x.LastModifiedBy.ToString()),
                Description = $"The user that has updated the content last."
            });

            AddField(new FieldType
            {
                Name = "status",
                ResolvedType = AllTypes.NonNullString,
                Resolver = Resolve(x => x.Status.Name.ToUpperInvariant()),
                Description = $"The the status of the content."
            });

            AddField(new FieldType
            {
                Name = "statusColor",
                ResolvedType = AllTypes.NonNullString,
                Resolver = Resolve(x => x.StatusColor),
                Description = $"The color status of the content."
            });

            Description = $"The structure of all content types.";
        }

        private static IFieldResolver Resolve(Func<IEnrichedContentEntity, object> action)
        {
            return new FuncFieldResolver<IEnrichedContentEntity, object>(c => action(c.Source));
        }
    }
}
