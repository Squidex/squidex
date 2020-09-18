// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class ContentInterfaceGraphType : InterfaceGraphType<IEnrichedContentEntity>
    {
        public ContentInterfaceGraphType()
        {
            Name = "Content";

            AddField(new FieldType
            {
                Name = "id",
                ResolvedType = AllTypes.NonNullString,
                Resolver = EntityResolvers.Id,
                Description = "The id of the content."
            });

            AddField(new FieldType
            {
                Name = "version",
                ResolvedType = AllTypes.NonNullInt,
                Resolver = EntityResolvers.Version,
                Description = "The version of the content."
            });

            AddField(new FieldType
            {
                Name = "created",
                ResolvedType = AllTypes.NonNullDate,
                Resolver = EntityResolvers.Created,
                Description = "The date and time when the content has been created."
            });

            AddField(new FieldType
            {
                Name = "createdBy",
                ResolvedType = AllTypes.NonNullString,
                Resolver = EntityResolvers.CreatedBy,
                Description = "The user that has created the content."
            });

            AddField(new FieldType
            {
                Name = "lastModified",
                ResolvedType = AllTypes.NonNullDate,
                Resolver = EntityResolvers.LastModified,
                Description = "The date and time when the content has been modified last."
            });

            AddField(new FieldType
            {
                Name = "lastModifiedBy",
                ResolvedType = AllTypes.NonNullString,
                Resolver = EntityResolvers.LastModifiedBy,
                Description = "The user that has updated the content last."
            });

            AddField(new FieldType
            {
                Name = "status",
                ResolvedType = AllTypes.NonNullString,
                Resolver = ContentResolvers.Status,
                Description = "The the status of the content."
            });

            AddField(new FieldType
            {
                Name = "statusColor",
                ResolvedType = AllTypes.NonNullString,
                Resolver = ContentResolvers.StatusColor,
                Description = "The color status of the content."
            });

            Description = "The structure of all content types.";
        }
    }
}
