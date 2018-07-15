// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using FakeItEasy;
using Microsoft.OData.Edm;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.Assets.Edm;
using Squidex.Domain.Apps.Entities.MongoDb.Assets;
using Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors;
using Squidex.Domain.Apps.Entities.Tags;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.MongoDb.OData;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets.OData
{
    public class ODataQueryTests
    {
        private readonly ITagService tagService = A.Fake<ITagService>();
        private readonly IBsonSerializerRegistry registry = BsonSerializer.SerializerRegistry;
        private readonly IBsonSerializer<MongoAssetEntity> serializer = BsonSerializer.SerializerRegistry.GetSerializer<MongoAssetEntity>();
        private readonly IEdmModel edmModel = EdmAssetModel.Edm;
        private readonly Guid appId = Guid.NewGuid();
        private readonly ConvertValue valueConverter;

        static ODataQueryTests()
        {
            InstantSerializer.Register();
        }

        public ODataQueryTests()
        {
            A.CallTo(() => tagService.GetTagIdsAsync(appId, TagGroups.Assets, A<HashSet<string>>.That.Contains("tag1")))
                .Returns(HashSet.Of("normalized1"));

            valueConverter = FindExtensions.CreateValueConverter(appId, tagService);
        }

        [Fact]
        public void Should_parse_query()
        {
            var parser = edmModel.ParseQuery("$filter=lastModifiedBy eq 'Sebastian'");

            Assert.NotNull(parser);
        }

        [Fact]
        public void Should_make_query_with_lastModified()
        {
            var i = F("$filter=lastModified eq 1988-01-19T12:00:00Z");
            var o = C("{ 'LastModified' : ISODate('1988-01-19T12:00:00Z') }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_lastModifiedBy()
        {
            var i = F("$filter=lastModifiedBy eq 'Me'");
            var o = C("{ 'LastModifiedBy' : 'Me' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_created()
        {
            var i = F("$filter=created eq 1988-01-19T12:00:00Z");
            var o = C("{ 'Created' : ISODate('1988-01-19T12:00:00Z') }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_createdBy()
        {
            var i = F("$filter=createdBy eq 'Me'");
            var o = C("{ 'CreatedBy' : 'Me' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_version()
        {
            var i = F("$filter=version eq 0");
            var o = C("{ 'Version' : NumberLong(0) }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_normalized_tags()
        {
            var i = F("$filter=tags eq 'tag1'");
            var o = C("{ 'Tags' : 'normalized1' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_tags()
        {
            var i = F("$filter=tags eq 'tag2'");
            var o = C("{ 'Tags' : 'tag2' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_fileName()
        {
            var i = F("$filter=fileName eq 'Logo.png'");
            var o = C("{ 'FileName' : 'Logo.png' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_fileSize()
        {
            var i = F("$filter=fileSize eq 1024");
            var o = C("{ 'FileSize' : NumberLong(1024) }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_fileVersion()
        {
            var i = F("$filter=fileVersion eq 2");
            var o = C("{ 'FileVersion' : NumberLong(2) }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_isImage()
        {
            var i = F("$filter=isImage eq true");
            var o = C("{ 'IsImage' : true }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_mimeType()
        {
            var i = F("$filter=mimeType eq 'text/json'");
            var o = C("{ 'MimeType' : 'text/json' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_pixelHeight()
        {
            var i = F("$filter=pixelHeight eq 600");
            var o = C("{ 'PixelHeight' : 600 }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_pixelWidth()
        {
            var i = F("$filter=pixelWidth eq 600");
            var o = C("{ 'PixelWidth' : 600 }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_orderby_with_single_field()
        {
            var i = S("$orderby=lastModified desc");
            var o = C("{ 'LastModified' : -1 }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_orderby_with_multiple_field()
        {
            var i = S("$orderby=lastModified, lastModifiedBy desc");
            var o = C("{ 'LastModified' : 1, 'LastModifiedBy' : -1 }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_top_statement()
        {
            var parser = edmModel.ParseQuery("$top=3");
            var cursor = A.Fake<IFindFluent<MongoAssetEntity, MongoAssetEntity>>();

            cursor.AssetTake(parser);

            A.CallTo(() => cursor.Limit(3)).MustHaveHappened();
        }

        [Fact]
        public void Should_make_top_statement_with_limit()
        {
            var parser = edmModel.ParseQuery("$top=300");
            var cursor = A.Fake<IFindFluent<MongoAssetEntity, MongoAssetEntity>>();

            cursor.AssetTake(parser);

            A.CallTo(() => cursor.Limit(200)).MustHaveHappened();
        }

        [Fact]
        public void Should_make_top_statement_with_default_value()
        {
            var parser = edmModel.ParseQuery(string.Empty);
            var cursor = A.Fake<IFindFluent<MongoAssetEntity, MongoAssetEntity>>();

            cursor.AssetTake(parser);

            A.CallTo(() => cursor.Limit(20)).MustHaveHappened();
        }

        [Fact]
        public void Should_make_skip_statement()
        {
            var parser = edmModel.ParseQuery("$skip=3");
            var cursor = A.Fake<IFindFluent<MongoAssetEntity, MongoAssetEntity>>();

            cursor.AssetSkip(parser);

            A.CallTo(() => cursor.Skip(3)).MustHaveHappened();
        }

        [Fact]
        public void Should_make_skip_statement_with_default_value()
        {
            var parser = edmModel.ParseQuery(string.Empty);
            var cursor = A.Fake<IFindFluent<MongoAssetEntity, MongoAssetEntity>>();

            cursor.AssetSkip(parser);

            A.CallTo(() => cursor.Skip(A<int>.Ignored)).MustNotHaveHappened();
        }

        private static string C(string value)
        {
            return value.Replace('\'', '"');
        }

        private string S(string value)
        {
            var parser = edmModel.ParseQuery(value);
            var cursor = A.Fake<IFindFluent<MongoAssetEntity, MongoAssetEntity>>();

            var i = string.Empty;

            A.CallTo(() => cursor.Sort(A<SortDefinition<MongoAssetEntity>>.Ignored))
                .Invokes((SortDefinition<MongoAssetEntity> sortDefinition) =>
                {
                    i = sortDefinition.Render(serializer, registry).ToString();
                });

            cursor.AssetSort(parser);

            return i;
        }

        private string F(string value)
        {
            var parser = edmModel.ParseQuery(value);

            var query =
                parser.BuildFilter<MongoAssetEntity>(convertValue: valueConverter)
                    .Filter.Render(serializer, registry).ToString();

            return query;
        }
    }
}