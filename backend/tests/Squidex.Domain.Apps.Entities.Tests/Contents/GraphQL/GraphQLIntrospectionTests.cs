﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;
using Squidex.Infrastructure;
using GraphQLSchema = GraphQL.Types.Schema;
using Schema = Squidex.Domain.Apps.Core.Schemas.Schema;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL;

public class GraphQLIntrospectionTests : GraphQLTestBase
{
    private Schema schema = new Schema { AppId = TestApp.DefaultId, Id = DomainId.NewGuid(), Name = "my-schema" };

    [Fact]
    public async Task Should_introspect()
    {
        var actual = await ExecuteAsync(new TestQuery
        {
            Query = @"
                query IntrospectionQuery {
                  __schema {
                    queryType {
                      name
                    }
                    mutationType {
                      name
                    }
                    subscriptionType {
                      name
                    }
                    types {
                      ...FullType
                    }
                    directives {
                      name
                      description
                      args {
                        ...InputValue
                      }
                      onOperation
                      onFragment
                      onField
                    }
                  }
                }

                fragment FullType on __Type {
                  kind
                  name
                  description
                  fields(includeDeprecated: true) {
                    name
                    description
                    args {
                      ...InputValue
                    }
                    type {
                      ...TypeRef
                    }
                    isDeprecated
                    deprecationReason
                  }
                  inputFields {
                    ...InputValue
                  }
                  interfaces {
                    ...TypeRef
                  }
                  enumValues(includeDeprecated: true) {
                    name
                    description
                    isDeprecated
                    deprecationReason
                  }
                  possibleTypes {
                    ...TypeRef
                  }
                }

                fragment InputValue on __InputValue {
                  name
                  description
                  type { 
                    ...TypeRef
                  }
                  defaultValue
                }

                fragment TypeRef on __Type {
                  kind
                  name
                  ofType {
                    kind
                    name
                    ofType {
                      kind
                      name
                      ofType {
                        kind
                        name
                      }
                    }
                  }
                }",
            OperationName = "IntrospectionQuery",
        });

        var json = serializer.Serialize(actual);

        Assert.NotEmpty(json);
    }

    [Fact]
    public async Task Should_build_graphql_schema_without_schemas()
    {
        var model = await CreateSut().GetSchemaAsync(TestApp.Default);

        Assert.NotNull(model);
    }

    [Fact]
    public async Task Should_build_graphql_schema_without_published_schema()
    {
        schema = schema.Unpublish();

        var model = await CreateSut(schema).GetSchemaAsync(TestApp.Default);

        Assert.NotNull(model);
    }

    [Fact]
    public async Task Should_build_graphql_schema_on_schema_with_ui_fields_only()
    {
        schema = schema.Publish().AddUI(1, "ui", Partitioning.Invariant);

        var model = await CreateSut(schema).GetSchemaAsync(TestApp.Default);

        Assert.NotNull(model);
    }

    [Fact]
    public async Task Should_build_graphql_schema_on_schema_with_invalid_fields_only()
    {
        schema = schema.Publish().AddComponent(1, "component", Partitioning.Invariant);

        var model = await CreateSut(schema).GetSchemaAsync(TestApp.Default);

        Assert.NotNull(model);
    }

    [Fact]
    public async Task Should_build_graphql_schema_on_unpublished_schema()
    {
        schema = schema.Publish().AddString(1, "myField", Partitioning.Invariant);

        var model = await CreateSut(schema).GetSchemaAsync(TestApp.Default);

        Assert.NotNull(model);
    }

    [Fact]
    public async Task Should_build_graphql_schema_on_reserved_schema_name()
    {
        schema = schema with { Name = "Content" };
        schema = schema.Publish().AddString(1, "myField", Partitioning.Invariant);

        var graphQLSchema = await CreateSut(schema).GetSchemaAsync(TestApp.Default);

        Assert.Contains(graphQLSchema.AllTypes, x => x.Name == "Content2");
    }

    [Fact]
    public async Task Should_build_graphql_schema_with_reserved_field_name()
    {
        schema = schema.Publish().AddString(1, "content", Partitioning.Invariant);

        var graphQLSchema = await CreateSut(schema).GetSchemaAsync(TestApp.Default);

        var type = FindDataType(graphQLSchema, "MySchema");

        Assert.Contains(type?.Fields!, x => x.Name == "content");
    }

    [Fact]
    public async Task Should_build_graphql_schema_on_invalid_field_name()
    {
        schema = schema.Publish().AddString(1, "2-field", Partitioning.Invariant);

        var graphQLSchema = await CreateSut(schema).GetSchemaAsync(TestApp.Default);

        var type = FindDataType(graphQLSchema, "MySchema");

        Assert.Contains(type?.Fields!, x => x.Name == "gql_2Field");
    }

    [Fact]
    public async Task Should_build_graphql_schema_on_duplicate_field_name()
    {
        schema = schema
            .Publish()
            .AddString(1, "my-field", Partitioning.Invariant)
            .AddString(2, "my_field", Partitioning.Invariant);

        var graphQLSchema = await CreateSut(schema).GetSchemaAsync(TestApp.Default);

        var type = FindDataType(graphQLSchema, "MySchema");

        Assert.Contains(type?.Fields!, x => x.Name == "myField");
        Assert.Contains(type?.Fields!, x => x.Name == "myField2");
    }

    [Fact]
    public async Task Should_not_build_grapqhl_schema_on_invalid_component()
    {
        schema = schema
            .Publish()
            .AddComponent(1, "my-component", Partitioning.Invariant,
                    new ComponentFieldProperties())
            .AddString(2, "my-string", Partitioning.Invariant);

        var graphQLSchema = await CreateSut(schema).GetSchemaAsync(TestApp.Default);

        var type = FindDataType(graphQLSchema, "MySchema");

        Assert.DoesNotContain(type?.Fields!, x => x.Name == "myComponent");
    }

    [Fact]
    public async Task Should_not_build_grapqhl_schema_on_invalid_components()
    {
        schema = schema
            .Publish()
            .AddComponents(1, "my-components", Partitioning.Invariant,
                new ComponentsFieldProperties())
            .AddString(2, "my-string", Partitioning.Invariant);

        var graphQLSchema = await CreateSut(schema).GetSchemaAsync(TestApp.Default);

        var type = FindDataType(graphQLSchema, "MySchema");

        Assert.DoesNotContain(type?.Fields!, x => x.Name == "myComponents");
    }

    [Fact]
    public async Task Should_not_build_grapqhl_schema_on_invalid_references()
    {
        schema = schema
            .Publish()
            .AddReferences(1, "my-references", Partitioning.Invariant,
                new ReferencesFieldProperties { SchemaId = DomainId.NewGuid() })
            .AddString(2, "my-string", Partitioning.Invariant);

        var graphQLSchema = await CreateSut(schema).GetSchemaAsync(TestApp.Default);

        var type = FindDataType(graphQLSchema, "MySchema");

        Assert.DoesNotContain(type?.Fields!, x => x.Name == "myReferences");
    }

    private static IObjectGraphType? FindDataType(GraphQLSchema graphQLSchema, string schema)
    {
        var type = (IObjectGraphType)graphQLSchema.AllTypes.Single(x => x.Name == schema);

        return (IObjectGraphType?)type.GetField("flatData")?.ResolvedType?.InnerType();
    }
}
