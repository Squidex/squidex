// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class ContentGraphType : ObjectGraphType<IEnrichedContentEntity>
    {
        private readonly DomainId schemaId;

        public ContentGraphType(ISchemaEntity schema)
        {
            schemaId = schema.Id;

            var schemaType = schema.TypeName();
            var schemaName = schema.DisplayName();

            Name = schemaType.SafeTypeName();

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
           return value is IContentEntity content && content.SchemaId?.Id == schemaId;
        }

        public void Initialize(IGraphModel model, ISchemaEntity schema, IEnumerable<ISchemaEntity> all, int pageSize)
        {
            var schemaType = schema.TypeName();
            var schemaName = schema.DisplayName();

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

            foreach (var other in all.Where(x => References(x, schema)))
            {
                var referencingId = other.Id;
                var referencingType = other.TypeName();
                var referencingName = other.DisplayName();

                var contentType = model.GetContentType(referencingId);

                AddReferencingQueries(referencingId, referencingType, referencingName, contentType, pageSize);
            }
        }

        private void AddReferencingQueries(DomainId referencingId, string referencingType, string referencingName, IGraphType contentType, int pageSize)
        {
            var resolver = ContentActions.QueryOrReferencing.Referencing(referencingId);

            AddField(new FieldType
            {
                Name = $"referencing{referencingType}Contents",
                Arguments = ContentActions.QueryOrReferencing.Arguments(pageSize),
                ResolvedType = new ListGraphType(new NonNullGraphType(contentType)),
                Resolver = resolver,
                Description = $"Query {referencingName} content items."
            });

            AddField(new FieldType
            {
                Name = $"referencing{referencingType}ContentsWithTotal",
                Arguments = ContentActions.QueryOrReferencing.Arguments(pageSize),
                ResolvedType = new ContentsResultGraphType(referencingType, referencingName, contentType),
                Resolver = resolver,
                Description = $"Query {referencingName} content items with total count."
            });
        }

        private static bool References(ISchemaEntity other, ISchemaEntity schema)
        {
            var id = schema.Id;

            return other.SchemaDef.Fields.Any(x => References(x, id));
        }

        private static bool References(IField field, DomainId id)
        {
            switch (field)
            {
                case IField<ReferencesFieldProperties> reference:
                    return
                        reference.Properties.SchemaIds == null ||
                        reference.Properties.SchemaIds.Count == 0 ||
                        reference.Properties.SchemaIds.Contains(id);
                case IArrayField arrayField:
                    return arrayField.Fields.Any(x => References(x, id));
            }

            return false;
        }
    }
}
