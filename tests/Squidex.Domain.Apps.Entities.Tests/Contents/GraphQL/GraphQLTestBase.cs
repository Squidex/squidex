// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Claims;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NodaTime.Extensions;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Assets;
using Squidex.Domain.Apps.Entities.Assets.Repositories;
using Squidex.Domain.Apps.Entities.Contents.TestData;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;
using Xunit;

#pragma warning disable SA1311 // Static readonly fields must begin with upper-case letter
#pragma warning disable SA1401 // Fields must be private

namespace Squidex.Domain.Apps.Entities.Contents.GraphQL
{
    public class GraphQLTestBase
    {
        protected static readonly Guid schemaId = Guid.NewGuid();
        protected static readonly Guid appId = Guid.NewGuid();
        protected static readonly string appName = "my-app";
        protected readonly Schema schemaDef;
        protected readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        protected readonly ICommandBus commandBus = A.Fake<ICommandBus>();
        protected readonly IAssetRepository assetRepository = A.Fake<IAssetRepository>();
        protected readonly ISchemaEntity schema = A.Fake<ISchemaEntity>();
        protected readonly IMemoryCache cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        protected readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        protected readonly IAppEntity app = A.Dummy<IAppEntity>();
        protected readonly ClaimsPrincipal user = new ClaimsPrincipal();
        protected readonly IGraphQLService sut;

        public GraphQLTestBase()
        {
            schemaDef =
                new Schema("my-schema")
                    .AddField(new JsonField(1, "my-json", Partitioning.Invariant,
                        new JsonFieldProperties()))
                    .AddField(new StringField(2, "my-string", Partitioning.Language,
                        new StringFieldProperties()))
                    .AddField(new NumberField(3, "my-number", Partitioning.Invariant,
                        new NumberFieldProperties()))
                    .AddField(new AssetsField(4, "my-assets", Partitioning.Invariant,
                        new AssetsFieldProperties()))
                    .AddField(new BooleanField(5, "my-boolean", Partitioning.Invariant,
                        new BooleanFieldProperties()))
                    .AddField(new DateTimeField(6, "my-datetime", Partitioning.Invariant,
                        new DateTimeFieldProperties()))
                    .AddField(new ReferencesField(7, "my-references", Partitioning.Invariant,
                        new ReferencesFieldProperties { SchemaId = schemaId }))
                    .AddField(new ReferencesField(9, "my-invalid", Partitioning.Invariant,
                        new ReferencesFieldProperties { SchemaId = Guid.NewGuid() }))
                    .AddField(new GeolocationField(10, "my-geolocation", Partitioning.Invariant,
                        new GeolocationFieldProperties()))
                    .AddField(new TagsField(11, "my-tags", Partitioning.Invariant,
                        new TagsFieldProperties()));

            A.CallTo(() => app.Id).Returns(appId);
            A.CallTo(() => app.Name).Returns(appName);
            A.CallTo(() => app.LanguagesConfig).Returns(LanguagesConfig.Build(Language.DE));

            A.CallTo(() => schema.Id).Returns(schemaId);
            A.CallTo(() => schema.Name).Returns(schemaDef.Name);
            A.CallTo(() => schema.SchemaDef).Returns(schemaDef);
            A.CallTo(() => schema.IsPublished).Returns(true);
            A.CallTo(() => schema.ScriptQuery).Returns("<script-query>");

            var allSchemas = new List<ISchemaEntity> { schema };

            A.CallTo(() => appProvider.GetSchemasAsync(appId)).Returns(allSchemas);

            sut = new CachingGraphQLService(cache, appProvider, assetRepository, commandBus, contentQuery, new FakeUrlGenerator());
        }

        protected static IContentEntity CreateContent(Guid id, Guid refId, Guid assetId, NamedContentData data = null, bool noJson = false)
        {
            var now = DateTime.UtcNow.ToInstant();

            data = data ??
                new NamedContentData()
                    .AddField("my-string",
                        new ContentFieldData().AddValue("de", "value"))
                    .AddField("my-assets",
                        new ContentFieldData().AddValue("iv", JToken.FromObject(new[] { assetId })))
                    .AddField("my-number",
                        new ContentFieldData().AddValue("iv", 1))
                    .AddField("my-boolean",
                        new ContentFieldData().AddValue("iv", true))
                    .AddField("my-datetime",
                        new ContentFieldData().AddValue("iv", now.ToDateTimeUtc()))
                    .AddField("my-tags",
                        new ContentFieldData().AddValue("iv", JToken.FromObject(new[] { "tag1", "tag2" })))
                    .AddField("my-references",
                        new ContentFieldData().AddValue("iv", JToken.FromObject(new[] { refId })))
                    .AddField("my-geolocation",
                        new ContentFieldData().AddValue("iv", JToken.FromObject(new { latitude = 10, longitude = 20 })));

            if (!noJson)
            {
                data.AddField("my-json",
                    new ContentFieldData().AddValue("iv", JToken.FromObject(new { value = 1 })));
            }

            var content = new ContentEntity
            {
                Id = id,
                Version = 1,
                Created = now,
                CreatedBy = new RefToken("subject", "user1"),
                LastModified = now,
                LastModifiedBy = new RefToken("subject", "user2"),
                Data = data
            };

            return content;
        }

        protected static IAssetEntity CreateAsset(Guid id)
        {
            var now = DateTime.UtcNow.ToInstant();

            var asset = new FakeAssetEntity
            {
                Id = id,
                Version = 1,
                Created = now,
                CreatedBy = new RefToken("subject", "user1"),
                LastModified = now,
                LastModifiedBy = new RefToken("subject", "user2"),
                FileName = "MyFile.png",
                FileSize = 1024,
                FileVersion = 123,
                MimeType = "image/png",
                IsImage = true,
                PixelWidth = 800,
                PixelHeight = 600
            };

            return asset;
        }

        protected static void AssertResult(object expected, (object Data, object[] Errors) result, bool checkErrors = true)
        {
            if (checkErrors && (result.Errors != null && result.Errors.Length > 0))
            {
                throw new InvalidOperationException(result.Errors[0]?.ToString());
            }

            var resultJson = JsonConvert.SerializeObject(new { data = result.Data }, Formatting.Indented);
            var expectJson = JsonConvert.SerializeObject(expected, Formatting.Indented);

            Assert.Equal(expectJson, resultJson);
        }
    }
}
