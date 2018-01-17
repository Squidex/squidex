// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using GraphQL.Resolvers;
using GraphQL.Types;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    public sealed class ContentDataChangedResultGraphType : ObjectGraphType<ContentDataChangedResult>
    {
        public ContentDataChangedResultGraphType(string schemaType, string schemaName, IComplexGraphType contentDataType)
        {
            Name = $"{schemaName}DataChangedResultDto";

            AddField(new FieldType
            {
                Name = "version",
                ResolvedType = AllTypes.Int,
                Resolver = Resolve(x => x.Version),
                Description = $"The new version of the {schemaName} content."
            });

            AddField(new FieldType
            {
                Name = "data",
                ResolvedType = new NonNullGraphType(contentDataType),
                Resolver = Resolve(x => x.Data),
                Description = $"The new data of the {schemaName} content."
            });

            Description = $"The result of the {schemaName} mutation";
        }

        private static IFieldResolver Resolve(Func<ContentDataChangedResult, object> action)
        {
            return new FuncFieldResolver<ContentDataChangedResult, object>(c => action(c.Source));
        }
    }
}
