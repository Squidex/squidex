// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Contents;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types.Dynamic;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using GraphQLSchema = GraphQL.Types.Schema;
using Schema = Squidex.Domain.Apps.Core.Schemas.Schema;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;

internal sealed class Builder
{
    private readonly Dictionary<FieldInfo, ComponentUnionGraphType> componentUnionTypes = [];
    private readonly Dictionary<FieldInfo, EmbeddableStringGraphType> embeddableStringTypes = [];
    private readonly Dictionary<FieldInfo, NestedGraphType> nestedTypes = [];
    private readonly Dictionary<FieldInfo, RichTextGraphType> richTextTypes = [];
    private readonly Dictionary<SchemaInfo, ComponentGraphType> componentTypes = [];
    private readonly Dictionary<SchemaInfo, ContentGraphType> contentTypes = [];
    private readonly Dictionary<SchemaInfo, ContentResultGraphType> contentResultTypes = [];
    private readonly Dictionary<string, ContentUnionGraphType> unionTypes = [];
    private readonly Dictionary<string, EnumerationGraphType?> enumTypes = [];
    private readonly Dictionary<string, IGraphType[]> dynamicTypes = [];
    private readonly FieldVisitor fieldVisitor;
    private readonly FieldInputVisitor fieldInputVisitor;
    private readonly PartitionResolver partitionResolver;
    private readonly HashSet<SchemaInfo> allSchemas = [];
    private readonly ReservedNames typeNames = ReservedNames.ForTypes();
    private readonly GraphQLOptions options;

    static Builder()
    {
        ValueConverter.Register<string, DomainId>(DomainId.Create);
        ValueConverter.Register<string, Status>(x => new Status(x));
    }

    public IInterfaceGraphType ContentInterface { get; } = new ContentInterfaceGraphType();

    public IInterfaceGraphType ComponentInterface { get; } = new ComponentInterfaceGraphType();

    public FieldMap FieldMap { get; private set; }

    public Builder(App app, GraphQLOptions options)
    {
        partitionResolver = app.PartitionResolver();

        fieldVisitor = new FieldVisitor(this);
        fieldInputVisitor = new FieldInputVisitor(this);

        this.options = options;
    }

    public GraphQLSchema BuildSchema(IEnumerable<Schema> schemas)
    {
        // Do not add schema without fields.
        allSchemas.AddRange(SchemaInfo.Build(schemas, typeNames).Where(x => x.Fields.Count > 0));

        // Only published normal schemas (not components are used for entities).
        var normalSchemas = allSchemas.Where(IsNormalSchema).ToList();

        foreach (var schemaInfo in normalSchemas)
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
            Query = new ApplicationQueries(this, normalSchemas)
        };

        newSchema.RegisterType(ComponentInterface);
        newSchema.RegisterType(ContentInterface);

        newSchema.Directives.Register(SharedTypes.CacheDirective);
        newSchema.Directives.Register(SharedTypes.OptimizeFieldQueriesDirective);

        FieldMap = new FieldMap(allSchemas);

        if (normalSchemas.Count != 0)
        {
            var mutations = new ApplicationMutations(this, normalSchemas);

            if (mutations.Fields.Count > 0)
            {
                newSchema.Mutation = mutations;
            }
        }

        if (options.EnableSubscriptions)
        {
            newSchema.Subscription = new ApplicationSubscriptions();
        }

        foreach (var (schemaInfo, contentType) in contentTypes)
        {
            contentType.Initialize(this, schemaInfo, normalSchemas);
        }

        foreach (var (schemaInfo, componentType) in componentTypes)
        {
            componentType.Initialize(this, schemaInfo);
        }

        foreach (var contentType in contentTypes.Values)
        {
            newSchema.RegisterType(contentType);
        }

        foreach (var customType in dynamicTypes.SelectMany(x => x.Value))
        {
            newSchema.RegisterType(customType);
        }

        newSchema.Initialize();

        return newSchema;
    }

    private static bool IsNormalSchema(SchemaInfo schema)
    {
        return schema.Schema.IsPublished && schema.Schema.Type != SchemaType.Component;
    }

    public FieldGraphSchema GetGraphType(FieldInfo fieldInfo)
    {
        return fieldInfo.Field.Accept(fieldVisitor, fieldInfo);
    }

    public IFieldPartitioning ResolvePartition(Partitioning key)
    {
        return partitionResolver(key);
    }

    public IGraphType? GetInputGraphType(FieldInfo fieldInfo)
    {
        return fieldInfo.Field.Accept(fieldInputVisitor, fieldInfo);
    }

    public IObjectGraphType? GetContentResultType(SchemaInfo schemaId)
    {
        return contentResultTypes.GetValueOrDefault(schemaId);
    }

    public IObjectGraphType? GetContentType(DomainId schemaId)
    {
        return contentTypes.FirstOrDefault(x => x.Key.Schema.Id == schemaId).Value;
    }

    public IObjectGraphType? GetContentType(SchemaInfo schemaId)
    {
        return contentTypes.GetValueOrDefault(schemaId);
    }

    public IObjectGraphType? GetComponentType(DomainId schemaId)
    {
        var schema = allSchemas.FirstOrDefault(x => x.Schema.Id == schemaId);

        if (schema == null)
        {
            return null;
        }

        return componentTypes.GetValueOrDefault(schema);
    }

    public IGraphType[] GetDynamicTypes(string? schema)
    {
        var graphQLSchema = schema;

        if (string.IsNullOrWhiteSpace(graphQLSchema))
        {
            return Array.Empty<GraphType>();
        }

        return dynamicTypes.GetOrAdd(graphQLSchema, x => DynamicSchemaBuilder.ParseTypes(x, typeNames));
    }

    public EnumerationGraphType? GetEnumeration(string name, IEnumerable<string> values)
    {
        return enumTypes.GetOrAdd(name, x => FieldEnumType.TryCreate(name, values));
    }

    public EmbeddableStringGraphType GetEmbeddableString(FieldInfo fieldInfo, StringFieldProperties properties)
    {
        return embeddableStringTypes.GetOrAdd(fieldInfo, x => new EmbeddableStringGraphType(this, x, properties));
    }

    public RichTextGraphType GetRichText(FieldInfo fieldInfo, RichTextFieldProperties properties)
    {
        return richTextTypes.GetOrAdd(fieldInfo, x => new RichTextGraphType(this, x, properties));
    }

    public ComponentUnionGraphType GetComponentUnion(FieldInfo fieldInfo, ReadonlyList<DomainId>? schemaIds)
    {
        return componentUnionTypes.GetOrAdd(fieldInfo, x => new ComponentUnionGraphType(this, x, schemaIds));
    }

    public ContentUnionGraphType GetContentUnion(string name, ReadonlyList<DomainId>? schemaIds)
    {
        return unionTypes.GetOrAdd(name, x => new ContentUnionGraphType(this, x, schemaIds));
    }

    public NestedGraphType GetNested(FieldInfo fieldInfo)
    {
        return nestedTypes.GetOrAdd(fieldInfo, x => new NestedGraphType(this, x));
    }

    public IEnumerable<KeyValuePair<SchemaInfo, ContentGraphType>> GetAllContentTypes()
    {
        return contentTypes;
    }
}
