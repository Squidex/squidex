// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Log;
using GraphQLSchema = GraphQL.Types.Schema;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public sealed class GraphQLModel
    {
        private static readonly IDocumentExecuter Executor = new DocumentExecuter();
        private readonly Dictionary<SchemaInfo, ContentGraphType> contentTypes = new Dictionary<SchemaInfo, ContentGraphType>(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<SchemaInfo, ContentResultGraphType> contentResultTypes = new Dictionary<SchemaInfo, ContentResultGraphType>(ReferenceEqualityComparer.Instance);
        private readonly GraphQLSchema schema;
        private readonly GraphQLTypeFactory typeFactory;
        private readonly ISemanticLog log;
#pragma warning disable IDE0044 // Add readonly modifier
        private GraphQLFieldVisitor fieldVisitor;
        private GraphQLFieldInputVisitor fieldInputVisitor;
        private PartitionResolver partitionResolver;
#pragma warning restore IDE0044 // Add readonly modifier

        static GraphQLModel()
        {
            ValueConverter.Register<JsonBoolean, bool>(x => x.Value);
            ValueConverter.Register<JsonNumber, double>(x => x.Value);
            ValueConverter.Register<JsonString, string>(x => x.Value);
            ValueConverter.Register<JsonString, DateTimeOffset>(x => DateTimeOffset.Parse(x.Value, CultureInfo.InvariantCulture));
            ValueConverter.Register<string, DomainId>(DomainId.Create);
        }

        public GraphQLTypeFactory TypeFactory
        {
            get { return typeFactory; }
        }

        public GraphQLModel(IAppEntity app, IEnumerable<ISchemaEntity> schemas, GraphQLTypeFactory typeFactory, ISemanticLog log)
        {
            this.typeFactory = typeFactory;

            this.log = log;

            partitionResolver = app.PartitionResolver();

            fieldVisitor = new GraphQLFieldVisitor(this);
            fieldInputVisitor = new GraphQLFieldInputVisitor(this);

            var allSchemas = schemas.Where(x => x.SchemaDef.IsPublished).Select(SchemaInfo.Build).ToList();

            BuildSchemas(allSchemas);

            schema = BuildSchema(allSchemas);
            schema.RegisterValueConverter(JsonConverter.Instance);
            schema.RegisterValueConverter(InstantConverter.Instance);

            InitializeContentTypes(allSchemas);

            partitionResolver = null!;

            fieldVisitor = null!;
            fieldInputVisitor = null!;
        }

        private void BuildSchemas(List<SchemaInfo> allSchemas)
        {
            foreach (var schemaInfo in allSchemas)
            {
                var contentType = new ContentGraphType(schemaInfo);

                contentTypes[schemaInfo] = contentType;
                contentResultTypes[schemaInfo] = new ContentResultGraphType(contentType, schemaInfo);
            }
        }

        private void InitializeContentTypes(List<SchemaInfo> allSchemas)
        {
            foreach (var (schemaInfo, contentType) in contentTypes)
            {
                contentType.Initialize(this, schemaInfo, allSchemas);
            }

            foreach (var contentType in contentTypes.Values)
            {
                schema.RegisterType(contentType);
            }

            schema.Initialize();
            schema.CleanupMetadata();
        }

        private GraphQLSchema BuildSchema(List<SchemaInfo> schemas)
        {
            var newSchema = new GraphQLSchema
            {
                Query = new AppQueriesGraphType(this, schemas)
            };

            newSchema.RegisterType(ContentInterfaceGraphType.Instance);

            var schemasWithFields = schemas.Where(x => x.Fields.Count > 0);

            if (schemasWithFields.Any())
            {
                newSchema.Mutation = new AppMutationsGraphType(this, schemasWithFields);
            }

            return newSchema;
        }

        internal IFieldPartitioning ResolvePartition(Partitioning key)
        {
            return partitionResolver(key);
        }

        internal IGraphType? GetInputGraphType(FieldInfo fieldInfo)
        {
            return fieldInfo.Field.Accept(fieldInputVisitor, fieldInfo);
        }

        internal (IGraphType?, IFieldResolver?, QueryArguments?) GetGraphType(FieldInfo fieldInfo)
        {
            return fieldInfo.Field.Accept(fieldVisitor, fieldInfo);
        }

        internal IObjectGraphType? GetContentType(DomainId schemaId)
        {
            return contentTypes.FirstOrDefault(x => x.Key.Schema.Id == schemaId).Value;
        }

        internal IObjectGraphType GetContentType(SchemaInfo schemaId)
        {
            return contentTypes.GetOrDefault(schemaId);
        }

        internal IObjectGraphType GetContentResultType(SchemaInfo schemaId)
        {
            return contentResultTypes.GetOrDefault(schemaId);
        }

        internal IEnumerable<KeyValuePair<SchemaInfo, ContentGraphType>> GetAllContentTypes()
        {
            return contentTypes;
        }

        public async Task<(object Data, object[]? Errors)> ExecuteAsync(GraphQLExecutionContext context, GraphQLQuery query)
        {
            Guard.NotNull(context, nameof(context));

            var result = await Executor.ExecuteAsync(execution =>
            {
                context.Setup(execution);

                execution.Schema = schema;
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
