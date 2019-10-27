// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class ContentsResultGraphType : ObjectGraphType<IResultList<IContentEntity>>
    {
        public ContentsResultGraphType(string schemaType, string schemaName, IGraphType contentType)
        {
            Name = $"{schemaType}ResultDto";

            AddField(new FieldType
            {
                Name = "total",
                Resolver = Resolver(x => x.Total),
                ResolvedType = AllTypes.NonNullInt,
                Description = $"The total number of {schemaName} items."
            });

            AddField(new FieldType
            {
                Name = "items",
                Resolver = Resolver(x => x),
                ResolvedType = new ListGraphType(new NonNullGraphType(contentType)),
                Description = $"The {schemaName} items."
            });

            Description = $"List of {schemaName} items and total count.";
        }

        private static IFieldResolver Resolver(Func<IResultList<IContentEntity>, object> action)
        {
            return new FuncFieldResolver<IResultList<IContentEntity>, object>(c => action(c.Source));
        }
    }
}
