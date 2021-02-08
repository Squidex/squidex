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
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.MongoDb.Assets;
using Squidex.Domain.Apps.Entities.MongoDb.Assets.Visitors;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;
using Squidex.Infrastructure.Validation;
using Xunit;
using ClrFilter = Squidex.Infrastructure.Queries.ClrFilter;
using SortBuilder = Squidex.Infrastructure.Queries.SortBuilder;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable IDE1006 // Naming Styles

namespace Squidex.Domain.Apps.Entities.Assets.MongoDb
{
    public class MongoDbQueryTests
    {
        private static readonly IBsonSerializerRegistry Registry = BsonSerializer.SerializerRegistry;
        private static readonly IBsonSerializer<MongoAssetEntity> Serializer = BsonSerializer.SerializerRegistry.GetSerializer<MongoAssetEntity>();

        static MongoDbQueryTests()
        {
            DomainIdSerializer.Register();

            TypeConverterStringSerializer<RefToken>.Register();
            TypeConverterStringSerializer<Status>.Register();

            InstantSerializer.Register();
        }

        [Fact]
        public void Should_throw_exception_for_full_text_search()
        {
            Assert.Throws<ValidationException>(() => _Q(new ClrQuery { FullText = "Full Text" }));
        }

        [Fact]
        public void Should_make_query_with_lastModified()
        {
            var i = _F(ClrFilter.Eq("lastModified", InstantPattern.General.Parse("1988-01-19T12:00:00Z").Value));
            var o = _C("{ 'mt' : ISODate('1988-01-19T12:00:00Z') }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_lastModifiedBy()
        {
            var i = _F(ClrFilter.Eq("lastModifiedBy", "subject:me"));
            var o = _C("{ 'mb' : 'subject:me' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_created()
        {
            var i = _F(ClrFilter.Eq("created", InstantPattern.General.Parse("1988-01-19T12:00:00Z").Value));
            var o = _C("{ 'ct' : ISODate('1988-01-19T12:00:00Z') }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_createdBy()
        {
            var i = _F(ClrFilter.Eq("createdBy", "subject:me"));
            var o = _C("{ 'cb' : 'subject:me' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_version()
        {
            var i = _F(ClrFilter.Eq("version", 0));
            var o = _C("{ 'vs' : NumberLong(0) }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_fileVersion()
        {
            var i = _F(ClrFilter.Eq("fileVersion", 2));
            var o = _C("{ 'fv' : NumberLong(2) }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_tags()
        {
            var i = _F(ClrFilter.Eq("tags", "tag1"));
            var o = _C("{ 'td' : 'tag1' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_fileName()
        {
            var i = _F(ClrFilter.Eq("fileName", "Logo.png"));
            var o = _C("{ 'fn' : 'Logo.png' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_mimeType()
        {
            var i = _F(ClrFilter.Eq("mimeType", "text/json"));
            var o = _C("{ 'mm' : 'text/json' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_fileSize()
        {
            var i = _F(ClrFilter.Eq("fileSize", 1024));
            var o = _C("{ 'fs' : NumberLong(1024) }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_pixelHeight()
        {
            var i = _F(ClrFilter.Eq("metadata.pixelHeight", 600));
            var o = _C("{ 'md.pixelHeight' : 600 }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_pixelWidth()
        {
            var i = _F(ClrFilter.Eq("metadata.pixelWidth", 800));
            var o = _C("{ 'md.pixelWidth' : 800 }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_orderby_with_single_field()
        {
            var i = _S(SortBuilder.Descending("lastModified"));
            var o = _C("{ 'mt' : -1 }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_orderby_with_multiple_fields()
        {
            var i = _S(SortBuilder.Ascending("lastModified"), SortBuilder.Descending("lastModifiedBy"));
            var o = _C("{ 'mt' : 1, 'mb' : -1 }");

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

        private static string _C(string value)
        {
            return value.Replace('\'', '"');
        }

        private static string _F(FilterNode<ClrValue> filter)
        {
            return _Q(new ClrQuery { Filter = filter });
        }

        private static string _S(params SortNode[] sorts)
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

        private static string _Q(ClrQuery query)
        {
            var filter = query.AdjustToModel().BuildFilter<MongoAssetEntity>(false).Filter!;

            var rendered = filter.Render(Serializer, Registry).ToString();

            return rendered;
        }
    }
}