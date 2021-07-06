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
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using GraphQLSchema = GraphQL.Types.Schema;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types
{
    internal sealed class Builder
    {
        private readonly Dictionary<SchemaInfo, ComponentGraphType> componentTypes = new Dictionary<SchemaInfo, ComponentGraphType>(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<SchemaInfo, ContentGraphType> contentTypes = new Dictionary<SchemaInfo, ContentGraphType>(ReferenceEqualityComparer.Instance);
        private readonly Dictionary<SchemaInfo, ContentResultGraphType> contentResultTypes = new Dictionary<SchemaInfo, ContentResultGraphType>(ReferenceEqualityComparer.Instance);
        private readonly FieldVisitor fieldVisitor;
        private readonly FieldInputVisitor fieldInputVisitor;
        private readonly PartitionResolver partitionResolver;
        private readonly List<SchemaInfo> allSchemas = new List<SchemaInfo>();

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
            // Do not add schema without fields.
            allSchemas.AddRange(SchemaInfo.Build(schemas).Where(x => x.Fields.Count > 0));

            // Only published normal schemas (not components are used for entities).
            var schemaInfos = allSchemas.Where(x => x.Schema.SchemaDef.IsPublished && x.Schema.SchemaDef.Type != SchemaType.Component).ToList();

            foreach (var schemaInfo in schemaInfos)
            {
                var contentType = new ContentGraphType(schemaInfo);

                contentTypes[schemaInfo] = contentType;
                contentResultTypes[schemaInfo] = new ContentResultGraphType(contentType, schemaInfo);
            }

            foreach (var schemaInfo in allSchemas)
            {
                var componentType = new ComponentGraphType(schemaInfo);

                componentTypes[schemaInfo] = componentType;
            }

            var newSchema = new GraphQLSchema
            {
                Query = new AppQueriesGraphType(this, schemaInfos)
            };

            newSchema.RegisterType(SharedTypes.ComponentInterface);
            newSchema.RegisterType(SharedTypes.ContentInterface);

            if (schemaInfos.Any())
            {
                var mutations = new AppMutationsGraphType(this, schemaInfos);

                if (mutations.Fields.Count > 0)
                {
                    newSchema.Mutation = mutations;
                }
            }

            foreach (var (schemaInfo, contentType) in contentTypes)
            {
                contentType.Initialize(this, schemaInfo, schemaInfos);
            }

            foreach (var (schemaInfo, componentType) in componentTypes)
            {
                componentType.Initialize(this, schemaInfo);
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

        public IObjectGraphType GetContentResultType(SchemaInfo schemaId)
        {
            return contentResultTypes.GetOrDefault(schemaId);
        }

        public IObjectGraphType? GetContentType(DomainId schemaId)
        {
            return contentTypes.FirstOrDefault(x => x.Key.Schema.Id == schemaId).Value;
        }

        public IObjectGraphType GetContentType(SchemaInfo schemaId)
        {
            return contentTypes.GetOrDefault(schemaId);
        }

        public IObjectGraphType? GetComponentType(DomainId schemaId)
        {
            var schema = allSchemas.Find(x => x.Schema.Id == schemaId);

            if (schema == null)
            {
                return null;
            }

            return componentTypes.GetOrDefault(schema);
        }

        public IEnumerable<KeyValuePair<SchemaInfo, ContentGraphType>> GetAllContentTypes()
        {
            return contentTypes;
        }
    }
}
