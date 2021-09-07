// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
{
    internal sealed class ContentGraphType : ObjectGraphType<IEnrichedContentEntity>
    {
        private readonly DomainId schemaId;

        public ContentGraphType(SchemaInfo schemaInfo)
        {
            // The name is used for equal comparison. Therefore it is important to treat it as readonly.
            Name = schemaInfo.ContentType;

            IsTypeOf = CheckType;

            schemaId = schemaInfo.Schema.Id;
        }

        private bool CheckType(object value)
        {
            return value is IContentEntity content && content.SchemaId?.Id == schemaId;
        }

        public void Initialize(Builder builder, SchemaInfo schemaInfo, IEnumerable<SchemaInfo> allSchemas)
        {
            AddField(ContentFields.Id);
            AddField(ContentFields.Version);
            AddField(ContentFields.Created);
            AddField(ContentFields.CreatedBy);
            AddField(ContentFields.CreatedByUser);
            AddField(ContentFields.LastModified);
            AddField(ContentFields.LastModifiedBy);
            AddField(ContentFields.LastModifiedByUser);
            AddField(ContentFields.Status);
            AddField(ContentFields.StatusColor);
            AddField(ContentFields.NewStatus);
            AddField(ContentFields.NewStatusColor);
            AddField(ContentFields.Url);

            var contentDataType = new DataGraphType(builder, schemaInfo);

            if (contentDataType.Fields.Any())
            {
                AddField(new FieldType
                {
                    Name = "data",
                    ResolvedType = new NonNullGraphType(contentDataType),
                    Resolver = ContentResolvers.Data,
                    Description = FieldDescriptions.ContentData
                });
            }

            var contentDataTypeFlat = new DataFlatGraphType(builder, schemaInfo);

            if (contentDataTypeFlat.Fields.Any())
            {
                AddField(new FieldType
                {
                    Name = "flatData",
                    ResolvedType = new NonNullGraphType(contentDataTypeFlat),
                    Resolver = ContentResolvers.FlatData,
                    Description = FieldDescriptions.ContentFlatData
                });
            }

            foreach (var other in allSchemas.Where(IsReferencingThis))
            {
                AddReferencingQueries(builder, other);
            }

            AddResolvedInterface(builder.SharedTypes.ContentInterface);

            Description = $"The structure of a {schemaInfo.DisplayName} content type.";
        }

        private void AddReferencingQueries(Builder builder, SchemaInfo referencingSchemaInfo)
        {
            var contentType = builder.GetContentType(referencingSchemaInfo);

            AddField(new FieldType
            {
                Name = $"referencing{referencingSchemaInfo.TypeName}Contents",
                Arguments = ContentActions.QueryOrReferencing.Arguments,
                ResolvedType = new ListGraphType(new NonNullGraphType(contentType)),
                Resolver = ContentActions.QueryOrReferencing.Referencing,
                Description = $"Query {referencingSchemaInfo.DisplayName} content items."
            }).WithSchemaId(referencingSchemaInfo);

            var contentResultsTyp = builder.GetContentResultType(referencingSchemaInfo);

            AddField(new FieldType
            {
                Name = $"referencing{referencingSchemaInfo.TypeName}ContentsWithTotal",
                Arguments = ContentActions.QueryOrReferencing.Arguments,
                ResolvedType = contentResultsTyp,
                Resolver = ContentActions.QueryOrReferencing.ReferencingWithTotal,
                Description = $"Query {referencingSchemaInfo.DisplayName} content items with total count."
            }).WithSchemaId(referencingSchemaInfo);
        }

        private bool IsReferencingThis(SchemaInfo other)
        {
            return other.Schema.SchemaDef.Fields.Any(IsReferencingThis);
        }

        private bool IsReferencingThis(IField field)
        {
            switch (field)
            {
                case IField<ReferencesFieldProperties> reference:
                    return
                        reference.Properties.SchemaIds == null ||
                        reference.Properties.SchemaIds.Count == 0 ||
                        reference.Properties.SchemaIds.Contains(schemaId);
                case IArrayField arrayField:
                    return arrayField.Fields.Any(IsReferencingThis);
            }

            return false;
        }
    }
}
