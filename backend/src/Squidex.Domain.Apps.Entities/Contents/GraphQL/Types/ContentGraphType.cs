// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Linq;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ConvertContent;
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
                ResolvedType = AllTypes.NonNullGuid,
                Resolver = Resolve(x => x.Id),
                Description = $"The id of the {schemaName} content."
            });

            AddField(new FieldType
            {
                Name = "version",
                ResolvedType = AllTypes.NonNullInt,
                Resolver = Resolve(x => x.Version),
                Description = $"The version of the {schemaName} content."
            });

            AddField(new FieldType
            {
                Name = "created",
                ResolvedType = AllTypes.NonNullDate,
                Resolver = Resolve(x => x.Created),
                Description = $"The date and time when the {schemaName} content has been created."
            });

            AddField(new FieldType
            {
                Name = "createdBy",
                ResolvedType = AllTypes.NonNullString,
                Resolver = Resolve(x => x.CreatedBy.ToString()),
                Description = $"The user that has created the {schemaName} content."
            });

            AddField(new FieldType
            {
                Name = "lastModified",
                ResolvedType = AllTypes.NonNullDate,
                Resolver = Resolve(x => x.LastModified),
                Description = $"The date and time when the {schemaName} content has been modified last."
            });

            AddField(new FieldType
            {
                Name = "lastModifiedBy",
                ResolvedType = AllTypes.NonNullString,
                Resolver = Resolve(x => x.LastModifiedBy.ToString()),
                Description = $"The user that has updated the {schemaName} content last."
            });

            AddField(new FieldType
            {
                Name = "status",
                ResolvedType = AllTypes.NonNullString,
                Resolver = Resolve(x => x.Status.Name.ToUpperInvariant()),
                Description = $"The the status of the {schemaName} content."
            });

            AddField(new FieldType
            {
                Name = "statusColor",
                ResolvedType = AllTypes.NonNullString,
                Resolver = Resolve(x => x.StatusColor),
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
                Resolver = model.ResolveContentUrl(schema),
                Description = $"The url to the the {schemaName} content."
            });

            var contentDataType = new ContentDataGraphType(schema, schemaName, schemaType, model);

            if (contentDataType.Fields.Any())
            {
                AddField(new FieldType
                {
                    Name = "data",
                    ResolvedType = new NonNullGraphType(contentDataType),
                    Resolver = Resolve(x => x.Data),
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
                    Resolver = ResolveFlat(x => x.Data),
                    Description = $"The flat data of the {schemaName} content."
                });
            }
        }

        private static IFieldResolver Resolve(Func<IEnrichedContentEntity, object?> action)
        {
            return new FuncFieldResolver<IEnrichedContentEntity, object?>(c => action(c.Source));
        }

        private static IFieldResolver ResolveFlat(Func<IEnrichedContentEntity, NamedContentData?> action)
        {
            return new FuncFieldResolver<IEnrichedContentEntity, FlatContentData?>(c =>
            {
                var context = (GraphQLExecutionContext)c.UserContext;

                return action(c.Source)?.ToFlatten(context.Context.App.LanguagesConfig.Master);
            });
        }
    }
}
