// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class ContentInterfaceGraphType : InterfaceGraphType
    {
        public ContentInterfaceGraphType()
        {
            Name = $"ContentInfaceDto";

            AddField(new FieldType
            {
                Name = "id",
                ResolvedType = AllTypes.NonNullGuid,
                Description = $"The id of the content."
            });

            AddField(new FieldType
            {
                Name = "version",
                ResolvedType = AllTypes.NonNullInt,
                Description = $"The version of the content."
            });

            AddField(new FieldType
            {
                Name = "created",
                ResolvedType = AllTypes.NonNullDate,
                Description = $"The date and time when the content has been created."
            });

            AddField(new FieldType
            {
                Name = "createdBy",
                ResolvedType = AllTypes.NonNullString,
                Description = $"The user that has created the content."
            });

            AddField(new FieldType
            {
                Name = "lastModified",
                ResolvedType = AllTypes.NonNullDate,
                Description = $"The date and time when the content has been modified last."
            });

            AddField(new FieldType
            {
                Name = "lastModifiedBy",
                ResolvedType = AllTypes.NonNullString,
                Description = $"The user that has updated the content last."
            });

            AddField(new FieldType
            {
                Name = "status",
                ResolvedType = AllTypes.NonNullString,
                Description = $"The the status of the content."
            });

            AddField(new FieldType
            {
                Name = "statusColor",
                ResolvedType = AllTypes.NonNullString,
                Description = $"The color status of the content."
            });

            AddField(new FieldType
            {
                Name = "url",
                ResolvedType = AllTypes.NonNullString,
                Description = $"The url to the the content."
            });

            Description = $"The structure of all content types.";
        }
    }
}
