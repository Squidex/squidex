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
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;
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
        private readonly GraphQLSchema graphQLSchema;
        private readonly GraphQLTypeFactory graphQLTypeFactory;
        private readonly ISemanticLog log;
#pragma warning disable IDE0044 // Add readonly modifier
        private GraphQLTypeVisitor typeVisitor;
        private PartitionResolver partitionResolver;
#pragma warning restore IDE0044 // Add readonly modifier

        static GraphQLModel()
        {
            ValueConverter.Register<string, DomainId>(DomainId.Create);
        }

        public GraphQLTypeFactory TypeFactory
        {
            get { return graphQLTypeFactory; }
        }

        public GraphQLModel(IAppEntity app, IEnumerable<ISchemaEntity> schemas, GraphQLTypeFactory typeFactory, ISemanticLog log)
        {
            graphQLTypeFactory = typeFactory;

            this.log = log;

            partitionResolver = app.PartitionResolver();

            typeVisitor = new GraphQLTypeVisitor(contentTypes, this);

            var allSchemas = schemas.Where(x => x.SchemaDef.IsPublished).ToList();

            BuildSchemas(allSchemas);

            graphQLSchema = BuildSchema(this, allSchemas);
            graphQLSchema.RegisterValueConverter(JsonConverter.Instance);
            graphQLSchema.RegisterValueConverter(InstantConverter.Instance);

            InitializeContentTypes(allSchemas);

            partitionResolver = null!;

            typeVisitor = null!;
        }

        private void BuildSchemas(List<ISchemaEntity> allSchemas)
        {
            foreach (var schema in allSchemas)
            {
                contentTypes[schema.Id] = new ContentGraphType(schema);
            }
        }

        private void InitializeContentTypes(List<ISchemaEntity> allSchemas)
        {
            var i = 0;

            foreach (var contentType in contentTypes.Values)
            {
                var schema = allSchemas[i];

                contentType.Initialize(this, schema, allSchemas);

                i++;
            }

            foreach (var contentType in contentTypes.Values)
            {
                graphQLSchema.RegisterType(contentType);
            }
        }

        private static GraphQLSchema BuildSchema(GraphQLModel model, List<ISchemaEntity> schemas)
        {
            var schema = new GraphQLSchema
            {
                Query = new AppQueriesGraphType(model, schemas)
            };

            schema.RegisterType(ContentInterfaceGraphType.Instance);

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
            return InputFieldVisitor.Build(field, this, schema, fieldName);
        }

        public (IGraphType?, ValueResolver?, QueryArguments?) GetGraphType(ISchemaEntity schema, IField field, string fieldName)
        {
            return field.Accept(typeVisitor, new GraphQLTypeVisitor.Args(schema, fieldName));
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
