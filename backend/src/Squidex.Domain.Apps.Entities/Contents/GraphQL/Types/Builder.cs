// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using GraphQL;
using GraphQL.Resolvers;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using GraphQLSchema = GraphQL.Types.Schema;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    internal sealed class Builder
    {
        private readonly Dictionary<SchemaInfo, ContentGraphType> contentTypes = new Dictionary<SchemaInfo, ContentGraphType>(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<SchemaInfo, ContentResultGraphType> contentResultTypes = new Dictionary<SchemaInfo, ContentResultGraphType>(ReferenceEqualityComparer.Instance);
        private readonly FieldVisitor fieldVisitor;
        private readonly FieldInputVisitor fieldInputVisitor;
        private readonly PartitionResolver partitionResolver;

        public SharedTypes SharedTypes { get; }

        static Builder()
        {
            ValueConverter.Register<string, DomainId>(DomainId.Create);
            ValueConverter.Register<string, Status>(x => new Status(x));
        }

        public Builder(IAppEntity app, SharedTypes sharedTypes)
        {
            SharedTypes = sharedTypes;

            partitionResolver = app.PartitionResolver();

            fieldVisitor = new FieldVisitor(this);
            fieldInputVisitor = new FieldInputVisitor(this);
        }

        public GraphQLSchema BuildSchema(IEnumerable<ISchemaEntity> schemas)
        {
            var schemaInfos = SchemaInfo.Build(schemas).ToList();

            foreach (var schemaInfo in schemaInfos)
            {
                var contentType = new ContentGraphType(this, schemaInfo);

                contentTypes[schemaInfo] = contentType;
                contentResultTypes[schemaInfo] = new ContentResultGraphType(contentType, schemaInfo);
            }

            var newSchema = new GraphQLSchema
            {
                Query = new AppQueriesGraphType(this, schemaInfos)
            };

            newSchema.RegisterType(SharedTypes.ContentInterface);

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
