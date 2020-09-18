// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using GraphQL.Types;
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class ContentGraphType : ObjectGraphType<IEnrichedContentEntity>
    {
        private readonly ISchemaEntity schema;
        private readonly string schemaType;
        private readonly string schemaName;

        public ContentGraphType(ISchemaEntity schema)
        {
            this.schema = schema;

            schemaType = schema.TypeName();
            schemaName = schema.DisplayName();

            Name = $"{schemaType}";

            AddField(new FieldType
            {
                Name = "id",
                ResolvedType = AllTypes.NonNullString,
                Resolver = EntityResolvers.Id,
                Description = $"The id of the {schemaName} content."
            });

            AddField(new FieldType
            {
                Name = "version",
                ResolvedType = AllTypes.NonNullInt,
                Resolver = EntityResolvers.Version,
                Description = $"The version of the {schemaName} content."
            });

            AddField(new FieldType
            {
                Name = "created",
                ResolvedType = AllTypes.NonNullDate,
                Resolver = EntityResolvers.Created,
                Description = $"The date and time when the {schemaName} content has been created."
            });

            AddField(new FieldType
            {
                Name = "createdBy",
                ResolvedType = AllTypes.NonNullString,
                Resolver = EntityResolvers.CreatedBy,
                Description = $"The user that has created the {schemaName} content."
            });

            AddField(new FieldType
            {
                Name = "lastModified",
                ResolvedType = AllTypes.NonNullDate,
                Resolver = EntityResolvers.LastModified,
                Description = $"The date and time when the {schemaName} content has been modified last."
            });

            AddField(new FieldType
            {
                Name = "lastModifiedBy",
                ResolvedType = AllTypes.NonNullString,
                Resolver = EntityResolvers.LastModifiedBy,
                Description = $"The user that has updated the {schemaName} content last."
            });

            AddField(new FieldType
            {
                Name = "status",
                ResolvedType = AllTypes.NonNullString,
                Resolver = ContentResolvers.Status,
                Description = $"The the status of the {schemaName} content."
            });

            AddField(new FieldType
            {
                Name = "statusColor",
                ResolvedType = AllTypes.NonNullString,
                Resolver = ContentResolvers.StatusColor,
                Description = $"The color status of the {schemaName} content."
            });

            Interface<ContentInterfaceGraphType>();

            Description = $"The structure of a {schemaName} content type.";

            IsTypeOf = CheckType;
        }

        private bool CheckType(object value)
        {
           return value is IContentEntity content && content.SchemaId?.Id == schema.Id;
        }

        public void Initialize(IGraphModel model)
        {
            AddField(new FieldType
            {
                Name = "url",
                ResolvedType = AllTypes.NonNullString,
                Resolver = ContentResolvers.Url,
                Description = $"The url to the the {schemaName} content."
            });

            var contentDataType = new ContentDataGraphType(schema, schemaName, schemaType, model);

            if (contentDataType.Fields.Any())
            {
                AddField(new FieldType
                {
                    Name = "data",
                    ResolvedType = new NonNullGraphType(contentDataType),
                    Resolver = ContentResolvers.Data,
                    Description = $"The data of the {schemaName} content."
                });
            }

            var contentDataTypeFlat = new ContentDataFlatGraphType(schema, schemaName, schemaType, model);

            if (contentDataTypeFlat.Fields.Any())
            {
                AddField(new FieldType
                {
                    Name = "flatData",
                    ResolvedType = new NonNullGraphType(contentDataTypeFlat),
                    Resolver = ContentResolvers.FlatData,
                    Description = $"The flat data of the {schemaName} content."
                });
            }
        }
    }
}
