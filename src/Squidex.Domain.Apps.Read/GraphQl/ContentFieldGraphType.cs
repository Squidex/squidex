// ==========================================================================
//  SchemaGraphType.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Read.GraphQl
{

    public sealed class ContentFieldGraphType : ObjectGraphType<ContentFieldData>
    {
        public ContentFieldGraphType(Field field, IGraphQLContext context)
        {
            var partition = context.ResolvePartition(field.Paritioning);

            foreach (var partitionItem in partition)
            {
                var fieldInfo = context.GetGraphType(field);

                AddField(new FieldType
                {
                    Name = partitionItem.Key,
                    Resolver = fieldInfo.Resolver,
                    ResolvedType = fieldInfo.ResolveType,
                    Description = field.RawProperties.Hints
                });
            }
        }
    }
}
