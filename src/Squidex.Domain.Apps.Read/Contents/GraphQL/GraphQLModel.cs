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

// ReSharper disable ConvertClosureToMethodGroup
// ReSharper disable InvertIf
// ReSharper disable ParameterHidesMember

namespace Squidex.Domain.Apps.Read.Contents.GraphQL
{
    public sealed class GraphQLModel : IGraphQLContext
    {
        private readonly Dictionary<Type, Func<Field, (IGraphType ResolveType, IFieldResolver Resolver)>> fieldInfos;
        private readonly Dictionary<Guid, ContentGraphType> schemaTypes = new Dictionary<Guid, ContentGraphType>();
        private readonly Dictionary<Guid, ISchemaEntity> schemas;
        private readonly PartitionResolver partitionResolver;
        private readonly IGraphType assetType = new AssetGraphType();
        private readonly GraphQLSchema graphQLSchema;

        public GraphQLModel(IAppEntity app, IEnumerable<ISchemaEntity> schemas)
        {
            partitionResolver = app.PartitionResolver;
            
            IGraphType assetListType = new ListGraphType(new NonNullGraphType(assetType));

            fieldInfos = new Dictionary<Type, Func<Field, (IGraphType ResolveType, IFieldResolver Resolver)>>
            {
                {
                    typeof(StringField),
                    field => ResolveDefault("String")
                },
                {
                    typeof(BooleanField),
                    field => ResolveDefault("Boolean")
                },
                {
                    typeof(NumberField),
                    field => ResolveDefault("Float")
                },
                {
                    typeof(DateTimeField),
                    field => ResolveDefault("Date")
                },
                {
                    typeof(JsonField),
                    field => ResolveDefault("Json")
                },
                {
                    typeof(GeolocationField),
                    field => ResolveDefault("Geolocation")
                },
                {
                    typeof(AssetsField),
                    field => ResolveAssets(assetListType)
                },
                {
                    typeof(ReferencesField),
                    field => ResolveReferences(field)
                }
            };

            this.schemas = schemas.ToDictionary(x => x.Id);
            
            graphQLSchema = new GraphQLSchema { Query = new ContentQueryGraphType(this, this.schemas.Values) };

            foreach (var schemaType in schemaTypes.Values)
            {
                schemaType.Initialize();
            }
        }

        private static (IGraphType ResolveType, IFieldResolver Resolver) ResolveDefault(string name)
        {
            return (new NoopGraphType(name), new FuncFieldResolver<ContentFieldData, object>(c => c.Source.GetOrDefault(c.FieldName)));
        }

        private static ValueTuple<IGraphType, IFieldResolver> ResolveAssets(IGraphType assetListType)
        {
            var resolver = new FuncFieldResolver<ContentFieldData, object>(c =>
            {
                var context = (QueryContext)c.UserContext;
                var contentIds = c.Source.GetOrDefault(c.FieldName);

                return context.GetReferencedAssetsAsync(contentIds);
            });

            return (assetListType, resolver);
        }

        private ValueTuple<IGraphType, IFieldResolver> ResolveReferences(Field field)
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

                return context.GetReferencedContentsAsync(schemaId, contentIds);
            });

            var schemaFieldType = new ListGraphType(new NonNullGraphType(GetSchemaType(schemaId)));

            return (schemaFieldType, resolver);
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
            var schemaEntity = schemas.GetOrDefault(schemaId);

            return schemaEntity != null ? schemaTypes.GetOrAdd(schemaId, k => new ContentGraphType(schemaEntity.Schema, this)) : null;
        }
    }
}
