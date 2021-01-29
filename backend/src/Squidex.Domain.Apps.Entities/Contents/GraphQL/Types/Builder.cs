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
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Primitives;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using GraphQLSchema = GraphQL.Types.Schema;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    internal sealed class Builder
    {
        private readonly Dictionary<SchemaInfo, ContentGraphType> contentTypes = new Dictionary<SchemaInfo, ContentGraphType>(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<SchemaInfo, ContentResultGraphType> contentResultTypes = new Dictionary<SchemaInfo, ContentResultGraphType>(ReferenceEqualityComparer.Instance);
        private readonly GraphQLTypeFactory typeFactory;
        private readonly GraphQLFieldVisitor fieldVisitor;
        private readonly GraphQLFieldInputVisitor fieldInputVisitor;
        private readonly PartitionResolver partitionResolver;

        public GraphQLTypeFactory TypeFactory
        {
            get { return typeFactory; }
        }

        static Builder()
        {
            ValueConverter.Register<JsonBoolean, bool>(x => x.Value);
            ValueConverter.Register<JsonNumber, double>(x => x.Value);
            ValueConverter.Register<JsonString, string>(x => x.Value);
            ValueConverter.Register<JsonString, DateTimeOffset>(x => DateTimeOffset.Parse(x.Value, CultureInfo.InvariantCulture));
            ValueConverter.Register<string, DomainId>(DomainId.Create);
            ValueConverter.Register<string, Status>(x => new Status(x));
        }

        public Builder(IAppEntity app, GraphQLTypeFactory typeFactory)
        {
            this.typeFactory = typeFactory;

            partitionResolver = app.PartitionResolver();

            fieldVisitor = new GraphQLFieldVisitor(this);
            fieldInputVisitor = new GraphQLFieldInputVisitor(this);
        }

        public GraphQLSchema BuildSchema(IEnumerable<ISchemaEntity> schemas)
        {
            var schemaInfos =
                schemas
                    .Where(x => x.SchemaDef.IsPublished).Select(SchemaInfo.Build)
                    .Where(x => x.Fields.Count > 0)
                    .ToList();

            foreach (var schemaInfo in schemaInfos)
            {
                var contentType = new ContentGraphType(schemaInfo);

                contentTypes[schemaInfo] = contentType;
                contentResultTypes[schemaInfo] = new ContentResultGraphType(contentType, schemaInfo);
            }

            var newSchema = new GraphQLSchema
            {
                Query = new AppQueriesGraphType(this, schemaInfos)
            };

            newSchema.RegisterValueConverter(JsonConverter.Instance);
            newSchema.RegisterValueConverter(InstantConverter.Instance);

            newSchema.RegisterType(ContentInterfaceGraphType.Instance);

            if (schemas.Any())
            {
                newSchema.Mutation = new AppMutationsGraphType(this, schemaInfos);
            }

            foreach (var (schemaInfo, contentType) in contentTypes)
            {
                contentType.Initialize(this, schemaInfo, schemaInfos);
            }

            foreach (var contentType in contentTypes.Values)
            {
                newSchema.RegisterType(contentType);
            }

            newSchema.Initialize();
            newSchema.CleanupMetadata();

            return newSchema;
        }

        public IFieldPartitioning ResolvePartition(Partitioning key)
        {
            return partitionResolver(key);
        }

        public IGraphType? GetInputGraphType(FieldInfo fieldInfo)
        {
            return fieldInfo.Field.Accept(fieldInputVisitor, fieldInfo);
        }

        public (IGraphType?, IFieldResolver?, QueryArguments?) GetGraphType(FieldInfo fieldInfo)
        {
            return fieldInfo.Field.Accept(fieldVisitor, fieldInfo);
        }

        public IObjectGraphType? GetContentType(DomainId schemaId)
        {
            return contentTypes.FirstOrDefault(x => x.Key.Schema.Id == schemaId).Value;
        }

        public IObjectGraphType GetContentType(SchemaInfo schemaId)
        {
            return contentTypes.GetOrDefault(schemaId);
        }

        public IObjectGraphType GetContentResultType(SchemaInfo schemaId)
        {
            return contentResultTypes.GetOrDefault(schemaId);
        }

        public IEnumerable<KeyValuePair<SchemaInfo, ContentGraphType>> GetAllContentTypes()
        {
            return contentTypes;
        }
    }
}
