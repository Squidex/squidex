// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

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
using Squidex.Log;
using GraphQLSchema = GraphQL.Types.Schema;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public sealed class GraphQLModel : IGraphModel
    {
        private static readonly IDocumentExecuter Executor = new DocumentExecuter();
        private readonly Dictionary<DomainId, ContentGraphType> contentTypes = new Dictionary<DomainId, ContentGraphType>();
        private readonly PartitionResolver partitionResolver;
        private readonly IObjectGraphType assetType;
        private readonly IGraphType assetListType;
        private readonly GraphQLSchema graphQLSchema;
        private readonly ISemanticLog log;

        public bool CanGenerateAssetSourceUrl { get; }

        static GraphQLModel()
        {
            ValueConverter.Register<string, DomainId>(DomainId.Create);
        }

        public GraphQLModel(IAppEntity app,
            IEnumerable<ISchemaEntity> schemas,
            int pageSizeContents,
            int pageSizeAssets,
            IUrlGenerator urlGenerator, ISemanticLog log)
        {
            this.log = log;

            partitionResolver = app.PartitionResolver();

            CanGenerateAssetSourceUrl = urlGenerator.CanGenerateAssetSourceUrl;

            assetType = new AssetGraphType(this);
            assetListType = new ListGraphType(new NonNullGraphType(assetType));

            var allSchemas = schemas.Where(x => x.SchemaDef.IsPublished).ToList();

            BuildSchemas(allSchemas);

            graphQLSchema = BuildSchema(this, pageSizeContents, pageSizeAssets, allSchemas);
            graphQLSchema.RegisterValueConverter(JsonConverter.Instance);
            graphQLSchema.RegisterValueConverter(InstantConverter.Instance);

            InitializeContentTypes(allSchemas, pageSizeContents);
        }

        private void BuildSchemas(List<ISchemaEntity> allSchemas)
        {
            foreach (var schema in allSchemas)
            {
                contentTypes[schema.Id] = new ContentGraphType(schema);
            }
        }

        private void InitializeContentTypes(List<ISchemaEntity> allSchemas, int pageSize)
        {
            var i = 0;

            foreach (var contentType in contentTypes.Values)
            {
                var schema = allSchemas[i];

                contentType.Initialize(this, schema, allSchemas, pageSize);

                i++;
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
                Query = new AppQueriesGraphType(
                    model,
                    pageSizeContents,
                    pageSizeAssets,
                    schemas)
            };

            var schemasWithFields = schemas.Where(x => x.SchemaDef.Fields.Count > 0);

            if (schemasWithFields.Any())
            {
                schema.Mutation = new AppMutationsGraphType(model, schemasWithFields);
            }

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

        public IGraphType GetContentType(DomainId schemaId)
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
            });

            if (result.Errors != null && result.Errors.Any())
            {
                log.LogWarning(w => w
                    .WriteProperty("action", "GraphQL")
                    .WriteProperty("status", "Failed")
                    .WriteArray("errors", a =>
                    {
                        foreach (var error in result.Errors)
                        {
                            a.WriteObject(error, (error, e) => e.WriteException(error));
                        }
                    }));
            }

            var errors = result.Errors?.Select(x => (object)new { x.Message, x.Locations }).ToArray();

            return (result.Data, errors);
        }
    }
}
