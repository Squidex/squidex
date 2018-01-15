// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class ContentResultGraphType : ObjectGraphType<IResultList<IContentEntity>>
    {
        public ContentResultGraphType(IGraphQLContext ctx, ISchemaEntity schema, string schemaName)
        {
            Name = $"{schema.Name.ToPascalCase()}ResultDto";

            var schemaType = ctx.GetSchemaType(schema.Id);

            AddField(new FieldType
            {
                Name = "total",
                Resolver = Resolver(x => x.Total),
                ResolvedType = new NonNullGraphType(new IntGraphType()),
                Description = $"The total number of {schemaName} items."
            });

            AddField(new FieldType
            {
                Name = "items",
                Resolver = Resolver(x => x),
                ResolvedType = new ListGraphType(new NonNullGraphType(schemaType)),
                Description = $"The {schemaName} items."
            });
        }

        private static IFieldResolver Resolver(Func<IResultList<IContentEntity>, object> action)
        {
            return new FuncFieldResolver<IResultList<IContentEntity>, object>(c => action(c.Source));
        }
    }
}
