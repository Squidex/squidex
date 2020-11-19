// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using FakeItEasy;
using GraphQL.DataLoader;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Contents.TestData;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Squidex.Infrastructure.Json;
using Squidex.Log;
using Xunit;

#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public class GraphQLTestBase
    {
        protected readonly IAppEntity app;
        protected readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        protected readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        protected readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        protected readonly IJsonSerializer serializer = TestUtils.CreateSerializer(TypeNameHandling.None);
        protected readonly ISchemaEntity schema;
        protected readonly ISchemaEntity schemaRef1;
        protected readonly ISchemaEntity schemaRef2;
        protected readonly ISchemaEntity schemaInvalidName;
        protected readonly Context requestContext;
        protected readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        protected readonly NamedId<DomainId> schemaId = NamedId.Of(DomainId.NewGuid(), "my-schema");
        protected readonly NamedId<DomainId> schemaRefId1 = NamedId.Of(DomainId.NewGuid(), "my-ref-schema1");
        protected readonly NamedId<DomainId> schemaRefId2 = NamedId.Of(DomainId.NewGuid(), "my-ref-schema2");
        protected readonly NamedId<DomainId> schemaInvalidNameId = NamedId.Of(DomainId.NewGuid(), "content");
        protected readonly IGraphQLService sut;

        public GraphQLTestBase()
        {
            app = Mocks.App(appId, Language.DE, Language.GermanGermany);

            var schemaDef =
                new Schema(schemaId.Name)
                    .Publish()
                    .AddJson(1, "my-json", Partitioning.Invariant,
                        new JsonFieldProperties())
                    .AddString(2, "my-string", Partitioning.Language,
                        new StringFieldProperties())
                    .AddNumber(3, "my-number", Partitioning.Invariant,
                        new NumberFieldProperties())
                    .AddNumber(4, "my_number", Partitioning.Invariant,
                        new NumberFieldProperties())
                    .AddAssets(5, "my-assets", Partitioning.Invariant,
                        new AssetsFieldProperties())
                    .AddBoolean(6, "my-boolean", Partitioning.Invariant,
                        new BooleanFieldProperties())
                    .AddDateTime(7, "my-datetime", Partitioning.Invariant,
                        new DateTimeFieldProperties())
                    .AddReferences(8, "my-references", Partitioning.Invariant,
                        new ReferencesFieldProperties { SchemaId = schemaRefId1.Id })
                    .AddReferences(81, "my-union", Partitioning.Invariant,
                        new ReferencesFieldProperties())
                    .AddReferences(9, "my-invalid", Partitioning.Invariant,
                        new ReferencesFieldProperties { SchemaId = DomainId.NewGuid() })
                    .AddGeolocation(10, "my-geolocation", Partitioning.Invariant,
                        new GeolocationFieldProperties())
                    .AddTags(11, "my-tags", Partitioning.Invariant,
                        new TagsFieldProperties())
                    .AddString(12, "my-localized", Partitioning.Language,
                        new StringFieldProperties())
                    .AddArray(13, "my-array", Partitioning.Invariant, f => f
                        .AddBoolean(121, "nested-boolean")
                        .AddNumber(122, "nested-number")
                        .AddNumber(123, "nested_number"))
                    .AddNumber(14, "2_numbers", Partitioning.Invariant,
                        new NumberFieldProperties())
                    .AddNumber(15, "2-numbers", Partitioning.Invariant,
                        new NumberFieldProperties())
                    .SetScripts(new SchemaScripts { Query = "<query-script>" });

            schema = Mocks.Schema(appId, schemaId, schemaDef);

            var schemaRef1Def =
                new Schema(schemaRefId1.Name)
                    .Publish()
                    .AddString(1, "ref1-field", Partitioning.Invariant);

            schemaRef1 = Mocks.Schema(appId, schemaRefId1, schemaRef1Def);

            var schemaRef2Def =
                new Schema(schemaRefId2.Name)
                    .Publish()
                    .AddString(1, "ref2-field", Partitioning.Invariant);

            schemaRef2 = Mocks.Schema(appId, schemaRefId2, schemaRef2Def);

            var schemaInvalidNameDef =
                new Schema(schemaInvalidNameId.Name)
                    .Publish()
                    .AddString(1, "my-field", Partitioning.Invariant);

            schemaInvalidName = Mocks.Schema(appId, schemaInvalidNameId, schemaInvalidNameDef);

            requestContext = new Context(Mocks.FrontendUser(), app);

            sut = CreateSut();
        }

        protected void AssertResult(object expected, (bool HasErrors, object Response) result, bool checkErrors = true)
        {
            if (checkErrors && result.HasErrors)
            {
                throw new InvalidOperationException(Serialize(result));
            }

            var resultJson = serializer.Serialize(result.Response, true);
            var expectJson = serializer.Serialize(expected, true);

            Assert.Equal(expectJson, resultJson);
        }

        private string Serialize((bool HasErrors, object Response) result)
        {
            return serializer.Serialize(result);
        }

        public sealed class TestServiceProvider : IServiceProvider
        {
            private readonly Dictionary<Type, object> services;

            public TestServiceProvider(GraphQLTestBase testBase)
            {
                var appProvider = A.Fake<IAppProvider>();

                A.CallTo(() => appProvider.GetSchemasAsync(testBase.appId.Id))
                    .Returns(new List<ISchemaEntity>
                    {
                        testBase.schema,
                        testBase.schemaRef1,
                        testBase.schemaRef2,
                        testBase.schemaInvalidName
                    });

                var dataLoaderContext = new DataLoaderContextAccessor();

                services = new Dictionary<Type, object>
                {
                    [typeof(IAppProvider)] = appProvider,
                    [typeof(IAssetQueryService)] = testBase.assetQuery,
                    [typeof(ICommandBus)] = testBase.commandBus,
                    [typeof(IContentQueryService)] = testBase.contentQuery,
                    [typeof(IDataLoaderContextAccessor)] = dataLoaderContext,
                    [typeof(IOptions<AssetOptions>)] = Options.Create(new AssetOptions()),
                    [typeof(IOptions<ContentOptions>)] = Options.Create(new ContentOptions()),
                    [typeof(ISemanticLog)] = A.Fake<ISemanticLog>(),
                    [typeof(IUrlGenerator)] = new FakeUrlGenerator(),
                    [typeof(DataLoaderDocumentListener)] = new DataLoaderDocumentListener(dataLoaderContext)
                };
            }

            public object GetService(Type serviceType)
            {
                return services.GetOrDefault(serviceType);
            }
        }

        private CachingGraphQLService CreateSut()
        {
            var cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));

            return new CachingGraphQLService(cache, new TestServiceProvider(this));
        }
    }
}
