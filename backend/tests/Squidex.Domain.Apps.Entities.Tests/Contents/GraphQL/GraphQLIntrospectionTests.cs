// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Contents.GraphQL.Types;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Xunit;
using Schema = Squidex.Domain.Apps.Core.Schemas.Schema;

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public class GraphQLIntrospectionTests : GraphQLTestBase
    {
        [Fact]
        public async Task Should_introspect()
        {
            const string query = @"
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
                }";

            var result = await ExecuteAsync(new ExecutionOptions { Query = query, OperationName = "IntrospectionQuery" });

            var json = serializer.Serialize(result, true);

            Assert.NotEmpty(json);
        }

        [Fact]
        public async Task Should_create_empty_schema()
        {
            var model = await CreateSut().GetModelAsync(TestApp.Default);

            Assert.NotNull(model);
        }

        [Fact]
        public async Task Should_create_empty_schema_with_empty_schema()
        {
            var schema =
                Mocks.Schema(TestApp.DefaultId,
                    NamedId.Of(DomainId.NewGuid(), "content"),
                    new Schema("content").Publish());

            var model = await CreateSut(schema).GetModelAsync(TestApp.Default);

            Assert.NotNull(model);
        }

        [Fact]
        public async Task Should_create_empty_schema_with_empty_schema_because_ui_field()
        {
            var schema =
                Mocks.Schema(TestApp.DefaultId,
                    NamedId.Of(DomainId.NewGuid(), "content"),
                    new Schema("content").Publish()
                        .AddUI(1, "ui", Partitioning.Invariant));

            var model = await CreateSut(schema).GetModelAsync(TestApp.Default);

            Assert.NotNull(model);
        }

        [Fact]
        public async Task Should_create_empty_schema_with_empty_schema_because_invalid_field()
        {
            var schema =
                Mocks.Schema(TestApp.DefaultId,
                    NamedId.Of(DomainId.NewGuid(), "content"),
                    new Schema("content").Publish()
                        .AddComponent(1, "component", Partitioning.Invariant));

            var model = await CreateSut(schema).GetModelAsync(TestApp.Default);

            Assert.NotNull(model);
        }

        [Fact]
        public async Task Should_create_empty_schema_with_unpublished_schema()
        {
            var schema =
                Mocks.Schema(TestApp.DefaultId,
                    NamedId.Of(DomainId.NewGuid(), "content"),
                    new Schema("content")
                        .AddString(1, "myField", Partitioning.Invariant));

            var model = await CreateSut(schema).GetModelAsync(TestApp.Default);

            Assert.NotNull(model);
        }

        [Fact]
        public async Task Should_create_schema_with_reserved_schema_name()
        {
            var schema =
                Mocks.Schema(TestApp.DefaultId,
                    NamedId.Of(DomainId.NewGuid(), "content"),
                    new Schema("content").Publish()
                        .AddString(1, "myField", Partitioning.Invariant));

            var model = await CreateSut(schema).GetModelAsync(TestApp.Default);

            Assert.Contains(model.Schema.AllTypes, x => x.Name == "ContentEntity");
        }

        [Fact]
        public async Task Should_create_schema_with_reserved_field_name()
        {
            var schema =
                Mocks.Schema(TestApp.DefaultId,
                    NamedId.Of(DomainId.NewGuid(), "my-schema"),
                    new Schema("my-schema").Publish()
                        .AddString(1, "content", Partitioning.Invariant));

            var model = await CreateSut(schema).GetModelAsync(TestApp.Default);

            var type = FindDataType(model, "MySchema");

            Assert.Contains(type.Fields, x => x.Name == "content");
        }

        [Fact]
        public async Task Should_create_schema_with_invalid_field_name()
        {
            var schema =
                Mocks.Schema(TestApp.DefaultId,
                    NamedId.Of(DomainId.NewGuid(), "my-schema"),
                    new Schema("my-schema").Publish()
                        .AddString(1, "2-field", Partitioning.Invariant));

            var model = await CreateSut(schema).GetModelAsync(TestApp.Default);

            var type = FindDataType(model, "MySchema");

            Assert.Contains(type.Fields, x => x.Name == "gql_2Field");
        }

        [Fact]
        public async Task Should_create_schema_with_duplicate_field_names()
        {
            var schema =
                Mocks.Schema(TestApp.DefaultId,
                    NamedId.Of(DomainId.NewGuid(), "my-schema"),
                    new Schema("my-schema").Publish()
                        .AddString(1, "my-field", Partitioning.Invariant)
                        .AddString(2, "my_field", Partitioning.Invariant));

            var model = await CreateSut(schema).GetModelAsync(TestApp.Default);

            var type = FindDataType(model, "MySchema");

            Assert.Contains(type.Fields, x => x.Name == "myField");
            Assert.Contains(type.Fields, x => x.Name == "myField2");
        }

        [Fact]
        public async Task Should_not_create_schema_With_invalid_component()
        {
            var schema =
                Mocks.Schema(TestApp.DefaultId,
                    NamedId.Of(DomainId.NewGuid(), "my-schema"),
                    new Schema("my-schema").Publish()
                        .AddComponent(1, "my-component", Partitioning.Invariant,
                            new ComponentFieldProperties())
                        .AddString(2, "my-string", Partitioning.Invariant));

            var model = await CreateSut(schema).GetModelAsync(TestApp.Default);

            var type = FindDataType(model, "MySchema");

            Assert.DoesNotContain(type.Fields, x => x.Name == "myComponent");
        }

        [Fact]
        public async Task Should_not_create_schema_With_invalid_components()
        {
            var schema =
                Mocks.Schema(TestApp.DefaultId,
                    NamedId.Of(DomainId.NewGuid(), "my-schema"),
                    new Schema("my-schema").Publish()
                        .AddComponents(1, "my-components", Partitioning.Invariant,
                            new ComponentsFieldProperties())
                        .AddString(2, "my-string", Partitioning.Invariant));

            var model = await CreateSut(schema).GetModelAsync(TestApp.Default);

            var type = FindDataType(model, "MySchema");

            Assert.DoesNotContain(type.Fields, x => x.Name == "myComponents");
        }

        [Fact]
        public async Task Should_not_create_schema_With_invalid_references()
        {
            var schema =
                Mocks.Schema(TestApp.DefaultId,
                    NamedId.Of(DomainId.NewGuid(), "my-schema"),
                    new Schema("my-schema").Publish()
                        .AddReferences(1, "my-references", Partitioning.Invariant,
                            new ReferencesFieldProperties { SchemaId = DomainId.NewGuid() })
                        .AddString(2, "my-string", Partitioning.Invariant));

            var model = await CreateSut(schema).GetModelAsync(TestApp.Default);

            var type = FindDataType(model, "MySchema");

            Assert.DoesNotContain(type.Fields, x => x.Name == "myReferences");
        }

        private static IObjectGraphType FindDataType(GraphQLModel model, string schema)
        {
            var type = (IObjectGraphType)model.Schema.AllTypes.Single(x => x.Name == schema);

            return (IObjectGraphType)type.GetField("flatData").ResolvedType.Flatten();
        }
    }
}
