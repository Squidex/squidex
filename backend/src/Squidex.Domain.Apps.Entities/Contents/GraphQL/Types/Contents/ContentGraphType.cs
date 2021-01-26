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

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents
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

            AddField(ContentFields.Id);
            AddField(ContentFields.Version);
            AddField(ContentFields.Created);
            AddField(ContentFields.CreatedBy);
            AddField(ContentFields.LastModified);
            AddField(ContentFields.LastModifiedBy);
            AddField(ContentFields.Status);
            AddField(ContentFields.StatusColor);

            AddResolvedInterface(ContentInterfaceGraphType.Instance);

            Description = $"The structure of a {schemaName} content type.";

            IsTypeOf = CheckType;
        }

        private bool CheckType(object value)
        {
           return value is IContentEntity content && content.SchemaId?.Id == schemaId;
        }

        public void Initialize(IGraphModel model, ISchemaEntity schema, IEnumerable<ISchemaEntity> all)
        {
            var schemaType = schema.TypeName();
            var schemaName = schema.DisplayName();

            AddField(new FieldType
            {
                Name = "url",
                ResolvedType = AllTypes.NonNullString,
                Resolver = ContentResolvers.Url,
                Description = $"The url to the content."
            });

            var contentDataType = new ContentDataGraphType(schema, schemaName, schemaType, model);

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

            var contentDataTypeFlat = new ContentDataFlatGraphType(schema, schemaName, schemaType, model);

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

            foreach (var other in all.Where(x => References(x, schema)))
            {
                var referencingId = other.Id;
                var referencingType = other.TypeName();
                var referencingName = other.DisplayName();

                var contentType = model.GetContentType(referencingId);

                AddReferencingQueries(referencingId, referencingType, referencingName, contentType);
            }
        }

        private void AddReferencingQueries(DomainId referencingId, string referencingType, string referencingName, IGraphType contentType)
        {
            var resolver = ContentActions.QueryOrReferencing.Referencing(referencingId);

            AddField(new FieldType
            {
                Name = $"referencing{referencingType}Contents",
                Arguments = ContentActions.QueryOrReferencing.Arguments,
                ResolvedType = new ListGraphType(new NonNullGraphType(contentType)),
                Resolver = resolver,
                Description = $"Query {referencingName} content items."
            });

            AddField(new FieldType
            {
                Name = $"referencing{referencingType}ContentsWithTotal",
                Arguments = ContentActions.QueryOrReferencing.Arguments,
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
