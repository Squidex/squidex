// ==========================================================================
//  GraphQLContext.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Contents.GraphQL.Types;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Infrastructure;
using GraphQLSchema = GraphQL.Types.Schema;

// ReSharper disable InvertIf
// ReSharper disable ParameterHidesMember

namespace Squidex.Domain.Apps.Read.Contents.GraphQL
{
    public sealed class GraphQLModel : IGraphQLContext
    {
        private readonly Dictionary<Type, Func<Field, (IGraphType ResolveType, IFieldResolver Resolver)>> fieldInfos;
        private readonly Dictionary<Guid, IGraphType> schemaTypes = new Dictionary<Guid, IGraphType>();
        private readonly Dictionary<Guid, ISchemaEntity> schemas;
        private readonly PartitionResolver partitionResolver;
        private readonly IGraphType assetType = new AssetGraphType();
        private readonly GraphQLSchema graphQLSchema;

        public GraphQLModel(IAppEntity appEntity, IEnumerable<ISchemaEntity> schemas)
        {
            partitionResolver = appEntity.PartitionResolver;

            var defaultResolver =
                new FuncFieldResolver<ContentFieldData, object>(c => c.Source.GetOrDefault(c.FieldName));

            IGraphType assetListType = new ListGraphType(new NonNullGraphType(assetType));

            var stringInfos = 
                (new StringGraphType(), defaultResolver);

            var booleanInfos =
                (new BooleanGraphType(), defaultResolver);

            var numberInfos =
                (new FloatGraphType(), defaultResolver);

            var dateTimeInfos =
                (new DateGraphType(), defaultResolver);

            var jsonInfos =
                (new ObjectGraphType(), defaultResolver);

            var geolocationInfos =
                (new ObjectGraphType(), defaultResolver);

            fieldInfos = new Dictionary<Type, Func<Field, (IGraphType ResolveType, IFieldResolver Resolver)>>
            {
                {
                    typeof(StringField),
                    field => stringInfos
                },
                {
                    typeof(BooleanField),
                    field => booleanInfos
                },
                {
                    typeof(NumberField),
                    field => numberInfos
                },
                {
                    typeof(DateTimeField),
                    field => dateTimeInfos
                },
                {
                    typeof(JsonField),
                    field => jsonInfos
                },
                {
                    typeof(GeolocationField),
                    field => geolocationInfos
                },
                {
                    typeof(AssetsField),
                    field =>
                    {
                        var resolver = new FuncFieldResolver<ContentFieldData, object>(c =>
                        {
                            var context = (QueryContext)c.UserContext;
                            var contentIds = c.Source.GetOrDefault(c.FieldName);

                            return context.GetReferencedAssets(contentIds);
                        });

                        return (assetListType, resolver);
                    }
                },
                {
                    typeof(ReferencesField),
                    field =>
                    {
                        var schemaId = ((ReferencesField)field).Properties.SchemaId;
                        var schemaType = GetSchemaType(schemaId);

                        if (schemaType == null)
                        {
                            return (null, null);
                        }

                        var resolver = new FuncFieldResolver<ContentFieldData, object>(c =>
                        {
                            var context = (QueryContext)c.UserContext;
                            var contentIds = c.Source.GetOrDefault(c.FieldName);

                            return context.GetReferencedContents(schemaId, contentIds);
                        });

                        var schemaFieldType = new ListGraphType(new NonNullGraphType(GetSchemaType(schemaId)));

                        return (schemaFieldType, resolver);
                    }
                }
            };

            this.schemas = schemas.ToDictionary(x => x.Id);
            
            graphQLSchema = new GraphQLSchema { Query = new ContentQueryType(this, this.schemas.Values) };
        }
        
        public async Task<object> ExecuteAsync(QueryContext context, GraphQLQuery query)
        {
            Guard.NotNull(context, nameof(context));

            var result = await new DocumentExecuter().ExecuteAsync(options =>
            {
                options.Query = query.Query;
                options.Schema = graphQLSchema;
                options.Inputs = query.Variables?.ToInputs() ?? new Inputs();
                options.UserContext = context;
                options.OperationName = query.OperationName;
            }).ConfigureAwait(false);

            if (result.Errors != null && result.Errors.Count > 0)
            {
                var errors = result.Errors.Select(x => new ValidationError(x.Message)).ToArray();

                throw new ValidationException("Failed to execute GraphQL query.", errors);
            }

            return result;
        }

        public IFieldPartitioning ResolvePartition(Partitioning key)
        {
            return partitionResolver(key);
        }

        public IGraphType GetAssetType()
        {
            return assetType;
        }

        public (IGraphType ResolveType, IFieldResolver Resolver) GetGraphType(Field field)
        {
            return fieldInfos[field.GetType()](field);
        }

        public IGraphType GetSchemaType(Guid schemaId)
        {
            return schemaTypes.GetOrAdd(schemaId, k =>
            {
                var schemaEntity = schemas.GetOrDefault(k);

                return schemaEntity != null ? new ContentGraphType(schemaEntity.Schema, this) : null;
            });
        }
    }
}
