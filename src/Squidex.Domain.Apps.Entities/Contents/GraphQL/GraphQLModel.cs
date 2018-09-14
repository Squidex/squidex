// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Utils;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using GraphQLSchema = GraphQL.Types.Schema;

#pragma warning disable IDE0003

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public sealed class GraphQLModel : IGraphModel
    {
        private readonly Dictionary<ISchemaEntity, ContentGraphType> contentTypes = new Dictionary<ISchemaEntity, ContentGraphType>();
        private readonly Dictionary<ISchemaEntity, ContentDataGraphType> contentDataTypes = new Dictionary<ISchemaEntity, ContentDataGraphType>();
        private readonly Dictionary<Guid, ISchemaEntity> schemasById;
        private readonly PartitionResolver partitionResolver;
        private readonly IAppEntity app;
        private readonly IGraphType assetType;
        private readonly IGraphType assetListType;
        private readonly GraphQLSchema graphQLSchema;

        public bool CanGenerateAssetSourceUrl { get; }

        public GraphQLModel(IAppEntity app, IEnumerable<ISchemaEntity> schemas, IGraphQLUrlGenerator urlGenerator)
        {
            this.app = app;

            partitionResolver = app.PartitionResolver();

            CanGenerateAssetSourceUrl = urlGenerator.CanGenerateAssetSourceUrl;

            assetType = new AssetGraphType(this);
            assetListType = new ListGraphType(new NonNullGraphType(assetType));

            schemasById = schemas.Where(x => x.IsPublished).ToDictionary(x => x.Id);

            graphQLSchema = BuildSchema(this);
            graphQLSchema.RegisterValueConverter(JsonConverter.Instance);

            InitializeContentTypes();
        }

        private static GraphQLSchema BuildSchema(GraphQLModel model)
        {
            var schemas = model.schemasById.Values;

            return new GraphQLSchema { Query = new AppQueriesGraphType(model, schemas) };
        }

        private void InitializeContentTypes()
        {
            foreach (var kvp in contentDataTypes)
            {
                kvp.Value.Initialize(this, kvp.Key);
            }

            foreach (var kvp in contentTypes)
            {
                kvp.Value.Initialize(this, kvp.Key, contentDataTypes[kvp.Key]);
            }
        }

        public IFieldResolver ResolveAssetUrl()
        {
            var resolver = new FuncFieldResolver<IAssetEntity, object>(c =>
            {
                var context = (GraphQLExecutionContext)c.UserContext;

                return context.UrlGenerator.GenerateAssetUrl(app, c.Source);
            });

            return resolver;
        }

        public IFieldResolver ResolveAssetSourceUrl()
        {
            var resolver = new FuncFieldResolver<IAssetEntity, object>(c =>
            {
                var context = (GraphQLExecutionContext)c.UserContext;

                return context.UrlGenerator.GenerateAssetSourceUrl(app, c.Source);
            });

            return resolver;
        }

        public IFieldResolver ResolveAssetThumbnailUrl()
        {
            var resolver = new FuncFieldResolver<IAssetEntity, object>(c =>
            {
                var context = (GraphQLExecutionContext)c.UserContext;

                return context.UrlGenerator.GenerateAssetThumbnailUrl(app, c.Source);
            });

            return resolver;
        }

        public IFieldResolver ResolveContentUrl(ISchemaEntity schema)
        {
            var resolver = new FuncFieldResolver<IContentEntity, object>(c =>
            {
                var context = (GraphQLExecutionContext)c.UserContext;

                return context.UrlGenerator.GenerateContentUrl(app, schema, c.Source);
            });

            return resolver;
        }

        public IFieldPartitioning ResolvePartition(Partitioning key)
        {
            return partitionResolver(key);
        }

        public (IGraphType ResolveType, ValueResolver Resolver) GetGraphType(ISchemaEntity schema, IField field)
        {
            return field.Accept(new QueryGraphTypeVisitor(schema, GetContentType, this, assetListType));
        }

        public IGraphType GetAssetType()
        {
            return assetType;
        }

        public IGraphType GetContentDataType(Guid schemaId)
        {
            var schema = schemasById.GetOrDefault(schemaId);

            if (schema == null)
            {
                return null;
            }

            return contentDataTypes.GetOrAddNew(schema);
        }

        public IGraphType GetContentType(Guid schemaId)
        {
            var schema = schemasById.GetOrDefault(schemaId);

            if (schema == null)
            {
                return null;
            }

            contentDataTypes.GetOrAdd(schema, s => new ContentDataGraphType());

            return contentTypes.GetOrAdd(schema, s => new ContentGraphType());
        }

        public async Task<(object Data, object[] Errors)> ExecuteAsync(GraphQLExecutionContext context, GraphQLQuery query)
        {
            Guard.NotNull(context, nameof(context));

            var inputs = query.Variables?.ToInputs();

            var result = await new DocumentExecuter().ExecuteAsync(options =>
            {
                options.OperationName = query.OperationName;
                options.UserContext = context;
                options.Schema = graphQLSchema;
                options.Inputs = inputs;
                options.Query = query.Query;
            }).ConfigureAwait(false);

            return (result.Data, result.Errors?.Select(x => (object)new { x.Message, x.Locations }).ToArray());
        }
    }
}
