// ==========================================================================
//  GraphQLContext.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Contents;
using Squidex.Infrastructure;
using Schema = Squidex.Domain.Apps.Core.Schemas.Schema;

namespace Squidex.Domain.Apps.Read.GraphQl
{
    public sealed class GraphQLContext : IGraphQLContext
    {
        private readonly PartitionResolver partitionResolver;
        private readonly Dictionary<Type, (IGraphType ResolveType, IFieldResolver Resolver)> fieldInfos;

        public GraphQLContext(PartitionResolver partitionResolver)
        {
            Guard.NotNull(partitionResolver, nameof(partitionResolver));

            this.partitionResolver = partitionResolver;

            var defaultResolver =
                new FuncFieldResolver<ContentFieldData, object>(c => c.Source.GetOrDefault(c.FieldName));

            fieldInfos = new Dictionary<Type, (IGraphType ResolveType, IFieldResolver Resolver)>
            {
                {
                    typeof(StringField),
                    (new StringGraphType(), defaultResolver)
                },
                {
                    typeof(BooleanField),
                    (new BooleanGraphType(), defaultResolver)
                },
                {
                    typeof(NumberField),
                    (new FloatGraphType(), defaultResolver)
                },
                {
                    typeof(DateTimeField),
                    (new FloatGraphType(), defaultResolver)
                },
                {
                    typeof(JsonField),
                    (new ObjectGraphType(), defaultResolver)
                },
                {
                    typeof(GeolocationField),
                    (new ObjectGraphType(), defaultResolver)
                },
                {
                    typeof(AssetsField),
                    (new ListGraphType<StringGraphType>(), defaultResolver)
                },
                {
                    typeof(ReferencesField),
                    (new ListGraphType<StringGraphType>(), defaultResolver)
                }
            };
        }

        public IGraphType GetSchemaListType(Schema schema)
        {
            throw new NotImplementedException();
        }

        public IGraphType GetSchemaListType(Guid schemaId)
        {
            throw new NotImplementedException();
        }

        public IFieldPartitioning ResolvePartition(Partitioning key)
        {
            return partitionResolver(key);
        }

        public (IGraphType ResolveType, IFieldResolver Resolver) GetGraphType(Field field)
        {
            return fieldInfos[field.GetType()];
        }
    }
}
