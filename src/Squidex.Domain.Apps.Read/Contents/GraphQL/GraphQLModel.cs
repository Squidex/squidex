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
using Squidex.Domain.Apps.Read.Assets;
using Squidex.Domain.Apps.Read.Contents.CustomQueries;
using Squidex.Domain.Apps.Read.Contents.GraphQL.Types;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Infrastructure;
using GraphQLSchema = GraphQL.Types.Schema;

namespace Squidex.Domain.Apps.Read.Contents.GraphQL
{
    public sealed class GraphQLModel : IGraphQLContext
    {
        private readonly Dictionary<Type, Func<Field, (IGraphType ResolveType, IFieldResolver Resolver)>> fieldInfos;
        private readonly Dictionary<Guid, ContentGraphType> schemaTypes = new Dictionary<Guid, ContentGraphType>();
        private readonly Dictionary<Guid, ISchemaEntity> schemas;
        private readonly PartitionResolver partitionResolver;
        private readonly IAppEntity app;
        private readonly IGraphType assetType;
        private readonly IGraphType assetListType;
        private readonly GraphQLSchema graphQLSchema;

        public string AppName => this.app.Name;

        public bool CanGenerateAssetSourceUrl { get; }

        public GraphQLModel(IAppEntity app, IEnumerable<ISchemaEntity> schemas, IGraphQLUrlGenerator urlGenerator, IQueryModulesService queryModulesService)
        {
            this.app = app;

            CanGenerateAssetSourceUrl = urlGenerator.CanGenerateAssetSourceUrl;

            partitionResolver = app.PartitionResolver;

            assetType = new AssetGraphType(this);
            assetListType = new ListGraphType(new NonNullGraphType(assetType));

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

            graphQLSchema = new GraphQLSchema { Query = new ContentQueryGraphType(this, this.schemas.Values, queryModulesService) };

            foreach (var schemaType in schemaTypes.Values)
            {
                schemaType.Initialize();
            }
        }

        private static (IGraphType ResolveType, IFieldResolver Resolver) ResolveDefault(string name)
        {
            return (new NoopGraphType(name), new FuncFieldResolver<ContentFieldData, object>(c => c.Source.GetOrDefault(c.FieldName)));
        }

        public IFieldResolver ResolveAssetUrl()
        {
            var resolver = new FuncFieldResolver<IAssetEntity, object>(c =>
            {
                var context = (GraphQLQueryContext)c.UserContext;

                return context.UrlGenerator.GenerateAssetUrl(app, c.Source);
            });

            return resolver;
        }

        public IFieldResolver ResolveAssetSourceUrl()
        {
            var resolver = new FuncFieldResolver<IAssetEntity, object>(c =>
            {
                var context = (GraphQLQueryContext)c.UserContext;

                return context.UrlGenerator.GenerateAssetSourceUrl(app, c.Source);
            });

            return resolver;
        }

        public IFieldResolver ResolveAssetThumbnailUrl()
        {
            var resolver = new FuncFieldResolver<IAssetEntity, object>(c =>
            {
                var context = (GraphQLQueryContext)c.UserContext;

                return context.UrlGenerator.GenerateAssetThumbnailUrl(app, c.Source);
            });

            return resolver;
        }

        public IFieldResolver ResolveContentUrl(ISchemaEntity schema)
        {
            var resolver = new FuncFieldResolver<IContentEntity, object>(c =>
            {
                var context = (GraphQLQueryContext)c.UserContext;

                return context.UrlGenerator.GenerateContentUrl(app, schema, c.Source);
            });

            return resolver;
        }

        private static ValueTuple<IGraphType, IFieldResolver> ResolveAssets(IGraphType assetListType)
        {
            var resolver = new FuncFieldResolver<ContentFieldData, object>(c =>
            {
                var context = (GraphQLQueryContext)c.UserContext;
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
                var context = (GraphQLQueryContext)c.UserContext;
                var contentIds = c.Source.GetOrDefault(c.FieldName);

                return context.GetReferencedContentsAsync(schemaId, contentIds);
            });

            var schemaFieldType = new ListGraphType(new NonNullGraphType(GetSchemaType(schemaId)));

            return (schemaFieldType, resolver);
        }

        public async Task<(object Data, object[] Errors)> ExecuteAsync(GraphQLQueryContext context, GraphQLQuery query)
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

            return (result.Data, result.Errors?.Select(x => (object)new { x.Message, x.Locations }).ToArray());
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
            var schema = schemas.GetOrDefault(schemaId);

            return schema != null ? schemaTypes.GetOrAdd(schemaId, k => new ContentGraphType(schema, this)) : null;
        }
    }
}
