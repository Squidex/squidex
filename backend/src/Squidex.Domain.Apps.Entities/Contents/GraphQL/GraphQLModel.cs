﻿// ==========================================================================
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
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
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
        private static readonly IDocumentExecuter Executor = new DocumentExecuter();
        private readonly Dictionary<Guid, ContentGraphType> contentTypes = new Dictionary<Guid, ContentGraphType>();
        private readonly PartitionResolver partitionResolver;
        private readonly IGraphType assetType;
        private readonly IGraphType assetListType;
        private readonly GraphQLSchema graphQLSchema;

        public bool CanGenerateAssetSourceUrl { get; }

        public GraphQLModel(IAppEntity app,
            IEnumerable<ISchemaEntity> schemas,
            int pageSizeContents,
            int pageSizeAssets,
            IUrlGenerator urlGenerator)
        {
            partitionResolver = app.PartitionResolver();

            CanGenerateAssetSourceUrl = urlGenerator.CanGenerateAssetSourceUrl;

            assetType = new AssetGraphType(this);
            assetListType = new ListGraphType(new NonNullGraphType(assetType));

            var allSchemas = schemas.Where(x => x.SchemaDef.IsPublished).ToList();

            BuildSchemas(allSchemas);

            graphQLSchema = BuildSchema(this, pageSizeContents, pageSizeAssets, allSchemas);
            graphQLSchema.RegisterValueConverter(JsonConverter.Instance);
            graphQLSchema.RegisterValueConverter(InstantConverter.Instance);

            InitializeContentTypes();
        }

        private void BuildSchemas(List<ISchemaEntity> allSchemas)
        {
            foreach (var schema in allSchemas)
            {
                contentTypes[schema.Id] = new ContentGraphType(schema);
            }
        }

        private void InitializeContentTypes()
        {
            foreach (var contentType in contentTypes.Values)
            {
                contentType.Initialize(this);
            }

            foreach (var contentType in contentTypes.Values)
            {
                graphQLSchema.RegisterType(contentType);
            }
        }

        private static GraphQLSchema BuildSchema(GraphQLModel model, int pageSizeContents, int pageSizeAssets, List<ISchemaEntity> schemas)
        {
            var schema = new GraphQLSchema
            {
                Query =
                    new AppQueriesGraphType(
                        model,
                        pageSizeContents,
                        pageSizeAssets,
                        schemas
                    ),
                Mutation = new AppMutationsGraphType(model, schemas)
            };

            return schema;
        }

        public IFieldPartitioning ResolvePartition(Partitioning key)
        {
            return partitionResolver(key);
        }

        public IGraphType? GetInputGraphType(ISchemaEntity schema, IField field, string fieldName)
        {
            return field.Accept(new InputFieldVisitor(schema, this, fieldName));
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) GetGraphType(ISchemaEntity schema, IField field, string fieldName)
        {
            return field.Accept(new QueryGraphTypeVisitor(schema, contentTypes, this, assetListType, fieldName));
        }

        public IGraphType GetAssetType()
        {
            return assetType;
        }

        public IGraphType GetContentType(Guid schemaId)
        {
            return contentTypes.GetOrDefault(schemaId);
        }

        public async Task<(object Data, object[]? Errors)> ExecuteAsync(GraphQLExecutionContext context, GraphQLQuery query)
        {
            Guard.NotNull(context, nameof(context));

            var result = await Executor.ExecuteAsync(execution =>
            {
                context.Setup(execution);

                execution.Schema = graphQLSchema;
                execution.Inputs = query.Inputs;
                execution.Query = query.Query;
            }).ConfigureAwait(false);

            var errors = result.Errors?.Select(x => (object)new { x.Message, x.Locations }).ToArray();

            return (result.Data, errors);
        }
    }
}
