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
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    internal sealed class ContentGraphType : ObjectGraphType<IEnrichedContentEntity>
    {
        private readonly DomainId schemaId;

        public ContentGraphType(SchemaInfo schemaInfo)
        {
            schemaId = schemaInfo.Schema.Id;

            Name = schemaInfo.ContentType;

            AddField(ContentFields.Id);
            AddField(ContentFields.Version);
            AddField(ContentFields.Created);
            AddField(ContentFields.CreatedBy);
            AddField(ContentFields.LastModified);
            AddField(ContentFields.LastModifiedBy);
            AddField(ContentFields.Status);
            AddField(ContentFields.StatusColor);

            AddResolvedInterface(ContentInterfaceGraphType.Instance);

            Description = $"The structure of a {schemaInfo.DisplayName} content type.";

            IsTypeOf = CheckType;
        }

        private bool CheckType(object value)
        {
           return value is IContentEntity content && content.SchemaId?.Id == schemaId;
        }

        public void Initialize(GraphQLModel model, SchemaInfo schemaInfo, IEnumerable<SchemaInfo> all)
        {
            AddField(new FieldType
            {
                Name = "url",
                ResolvedType = AllTypes.NonNullString,
                Resolver = ContentResolvers.Url,
                Description = $"The url to the content."
            });

            var contentDataType = new DataGraphType(model, schemaInfo);

            if (contentDataType.Fields.Any())
            {
                AddField(new FieldType
                {
                    Name = "data",
                    ResolvedType = new NonNullGraphType(contentDataType),
                    Resolver = ContentResolvers.Data,
                    Description = $"The data of the content."
                });
            }

            var contentDataTypeFlat = new DataFlatGraphType(model, schemaInfo);

            if (contentDataTypeFlat.Fields.Any())
            {
                AddField(new FieldType
                {
                    Name = "flatData",
                    ResolvedType = new NonNullGraphType(contentDataTypeFlat),
                    Resolver = ContentResolvers.FlatData,
                    Description = $"The flat data of the content."
                });
            }

            foreach (var other in all.Where(x => References(x, schemaInfo)))
            {
                AddReferencingQueries(model, other);
            }
        }

        private void AddReferencingQueries(GraphQLModel model, SchemaInfo referencingSchemaInfo)
        {
            var contentType = model.GetContentType(referencingSchemaInfo);

            AddField(new FieldType
            {
                Name = $"referencing{referencingSchemaInfo.TypeName}Contents",
                Arguments = ContentActions.QueryOrReferencing.Arguments,
                ResolvedType = new ListGraphType(new NonNullGraphType(contentType)),
                Resolver = ContentActions.QueryOrReferencing.Referencing,
                Description = $"Query {referencingSchemaInfo.DisplayName} content items."
            }).WithSchemaId(referencingSchemaInfo);

            var contentResultsTyp = model.GetContentResultType(referencingSchemaInfo);

            AddField(new FieldType
            {
                Name = $"referencing{referencingSchemaInfo.TypeName}ContentsWithTotal",
                Arguments = ContentActions.QueryOrReferencing.Arguments,
                ResolvedType = contentResultsTyp,
                Resolver = ContentActions.QueryOrReferencing.Referencing,
                Description = $"Query {referencingSchemaInfo.DisplayName} content items with total count."
            }).WithSchemaId(referencingSchemaInfo);
        }

        private static bool References(SchemaInfo other, SchemaInfo schema)
        {
            var id = schema.Schema.Id;

            return other.Schema.SchemaDef.Fields.Any(x => References(x, id));
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
