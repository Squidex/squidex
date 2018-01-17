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
using Squidex.Domain.Apps.Entities.Schemas;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class ContentGraphType : ObjectGraphType<IContentEntity>
    {
        public void Initialize(IGraphModel model, ISchemaEntity schema, IComplexGraphType contentDataType)
        {
            var schemaType = schema.TypeName();
            var schemaName = schema.DisplayName();

            Name = $"{schemaType}Dto";

            AddField(new FieldType
            {
                Name = "id",
                ResolvedType = new NonNullGraphType(new StringGraphType()),
                Resolver = Resolve(x => x.Id.ToString()),
                Description = $"The id of the {schemaName} content."
            });

            AddField(new FieldType
            {
                Name = "version",
                ResolvedType = new NonNullGraphType(new IntGraphType()),
                Resolver = Resolve(x => x.Version),
                Description = $"The version of the {schemaName} content."
            });

            AddField(new FieldType
            {
                Name = "created",
                ResolvedType = new NonNullGraphType(new DateGraphType()),
                Resolver = Resolve(x => x.Created.ToDateTimeUtc()),
                Description = $"The date and time when the {schemaName} content has been created."
            });

            AddField(new FieldType
            {
                Name = "createdBy",
                ResolvedType = new NonNullGraphType(new StringGraphType()),
                Resolver = Resolve(x => x.CreatedBy.ToString()),
                Description = $"The user that has created the {schemaName} content."
            });

            AddField(new FieldType
            {
                Name = "lastModified",
                ResolvedType = new NonNullGraphType(new DateGraphType()),
                Resolver = Resolve(x => x.LastModified.ToDateTimeUtc()),
                Description = $"The date and time when the {schemaName} content has been modified last."
            });

            AddField(new FieldType
            {
                Name = "lastModifiedBy",
                ResolvedType = new NonNullGraphType(new StringGraphType()),
                Resolver = Resolve(x => x.LastModifiedBy.ToString()),
                Description = $"The user that has updated the {schemaName} content last."
            });

            AddField(new FieldType
            {
                Name = "url",
                ResolvedType = new NonNullGraphType(new StringGraphType()),
                Resolver = model.ResolveContentUrl(schema),
                Description = $"The url to the the {schemaName} content."
            });

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

            Description = $"The structure of a {schemaName} content type.";
        }

        private static IFieldResolver Resolve(Func<IContentEntity, object> action)
        {
            return new FuncFieldResolver<IContentEntity, object>(c => action(c.Source));
        }
    }
}
