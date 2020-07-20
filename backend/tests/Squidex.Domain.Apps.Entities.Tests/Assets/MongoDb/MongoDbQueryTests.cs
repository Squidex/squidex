// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Linq;
using FakeItEasy;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using NodaTime.Text;
using Squidex.Domain.Apps.Entities.MongoDb.Assets;
using Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Validation;
using Xunit;
using ClrFilter = Squidex.Infrastructure.Queries.ClrFilter;
using SortBuilder = Squidex.Infrastructure.Queries.SortBuilder;

namespace Squidex.Domain.Apps.Entities.Assets.MongoDb
{
    public class MongoDbQueryTests
    {
        private static readonly IBsonSerializerRegistry Registry = BsonSerializer.SerializerRegistry;
        private static readonly IBsonSerializer<MongoAssetEntity> Serializer = BsonSerializer.SerializerRegistry.GetSerializer<MongoAssetEntity>();

        static MongoDbQueryTests()
        {
            InstantSerializer.Register();

            DomainIdSerializer.Register();
        }

        [Fact]
        public void Should_throw_exception_for_full_text_search()
        {
            Assert.Throws<ValidationException>(() => Q(new ClrQuery { FullText = "Full Text" }));
        }

        [Fact]
        public void Should_make_query_with_lastModified()
        {
            var i = F(ClrFilter.Eq("lastModified", InstantPattern.General.Parse("1988-01-19T12:00:00Z").Value));
            var o = C("{ 'mt' : ISODate('1988-01-19T12:00:00Z') }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_lastModifiedBy()
        {
            var i = F(ClrFilter.Eq("lastModifiedBy", "Me"));
            var o = C("{ 'mb' : 'Me' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_created()
        {
            var i = F(ClrFilter.Eq("created", InstantPattern.General.Parse("1988-01-19T12:00:00Z").Value));
            var o = C("{ 'ct' : ISODate('1988-01-19T12:00:00Z') }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_createdBy()
        {
            var i = F(ClrFilter.Eq("createdBy", "Me"));
            var o = C("{ 'cb' : 'Me' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_version()
        {
            var i = F(ClrFilter.Eq("version", 0));
            var o = C("{ 'vs' : NumberLong(0) }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_fileVersion()
        {
            var i = F(ClrFilter.Eq("fileVersion", 2));
            var o = C("{ 'fv' : NumberLong(2) }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_tags()
        {
            var i = F(ClrFilter.Eq("tags", "tag1"));
            var o = C("{ 'td' : 'tag1' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_fileName()
        {
            var i = F(ClrFilter.Eq("fileName", "Logo.png"));
            var o = C("{ 'fn' : 'Logo.png' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_mimeType()
        {
            var i = F(ClrFilter.Eq("mimeType", "text/json"));
            var o = C("{ 'mm' : 'text/json' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_fileSize()
        {
            var i = F(ClrFilter.Eq("fileSize", 1024));
            var o = C("{ 'fs' : NumberLong(1024) }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_pixelHeight()
        {
            var i = F(ClrFilter.Eq("metadata.pixelHeight", 600));
            var o = C("{ 'md.pixelHeight' : 600 }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_pixelWidth()
        {
            var i = F(ClrFilter.Eq("metadata.pixelWidth", 800));
            var o = C("{ 'md.pixelWidth' : 800 }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_orderby_with_single_field()
        {
            var i = S(SortBuilder.Descending("lastModified"));
            var o = C("{ 'mt' : -1 }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_orderby_with_multiple_fields()
        {
            var i = S(SortBuilder.Ascending("lastModified"), SortBuilder.Descending("lastModifiedBy"));
            var o = C("{ 'mt' : 1, 'mb' : -1 }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_take_statement()
        {
            var query = new ClrQuery { Take = 3 };
            var cursor = A.Fake<IFindFluent<MongoAssetEntity, MongoAssetEntity>>();

            cursor.QueryLimit(query.AdjustToModel());

            A.CallTo(() => cursor.Limit(3))
                .MustHaveHappened();
        }

        [Fact]
        public void Should_make_skip_statement()
        {
            var query = new ClrQuery { Skip = 3 };
            var cursor = A.Fake<IFindFluent<MongoAssetEntity, MongoAssetEntity>>();

            cursor.QuerySkip(query.AdjustToModel());

            A.CallTo(() => cursor.Skip(3))
                .MustHaveHappened();
        }

        private static string C(string value)
        {
            return value.Replace('\'', '"');
        }

        private static string F(FilterNode<ClrValue> filter)
        {
            return Q(new ClrQuery { Filter = filter });
        }

        private static string S(params SortNode[] sorts)
        {
            var cursor = A.Fake<IFindFluent<MongoAssetEntity, MongoAssetEntity>>();

            var i = string.Empty;

            A.CallTo(() => cursor.Sort(A<SortDefinition<MongoAssetEntity>>._))
                .Invokes((SortDefinition<MongoAssetEntity> sortDefinition) =>
                {
                    i = sortDefinition.Render(Serializer, Registry).ToString();
                });

            cursor.QuerySort(new ClrQuery { Sort = sorts.ToList() }.AdjustToModel());

            return i;
        }

        private static string Q(ClrQuery query)
        {
            var rendered =
                query.AdjustToModel().BuildFilter<MongoAssetEntity>(false).Filter!
                    .Render(Serializer, Registry).ToString();

            return rendered;
        }
    }
}