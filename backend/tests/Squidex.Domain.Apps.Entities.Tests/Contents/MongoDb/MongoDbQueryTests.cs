﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using NodaTime.Text;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.MongoDb.Contents;
using Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.MongoDb.Queries;
using Squidex.Infrastructure.Queries;
using Xunit;
using ClrFilter = Squidex.Infrastructure.Queries.ClrFilter;
using SortBuilder = Squidex.Infrastructure.Queries.SortBuilder;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable IDE1006 // Naming Styles

namespace Squidex.Domain.Apps.Entities.Contents.MongoDb
{
    public class MongoDbQueryTests
    {
        private static readonly IBsonSerializerRegistry Registry = BsonSerializer.SerializerRegistry;
        private static readonly IBsonSerializer<MongoContentEntity> Serializer = BsonSerializer.SerializerRegistry.GetSerializer<MongoContentEntity>();
        private readonly Schema schemaDef;
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.English.Set(Language.DE);

        static MongoDbQueryTests()
        {
            InstantSerializer.Register();
        }

        public MongoDbQueryTests()
        {
            schemaDef =
                new Schema("user")
                    .AddString(1, "firstName", Partitioning.Language,
                        new StringFieldProperties())
                    .AddString(2, "lastName", Partitioning.Language,
                        new StringFieldProperties())
                    .AddBoolean(3, "isAdmin", Partitioning.Invariant,
                        new BooleanFieldProperties())
                    .AddNumber(4, "age", Partitioning.Invariant,
                        new NumberFieldProperties())
                    .AddDateTime(5, "birthday", Partitioning.Invariant,
                        new DateTimeFieldProperties())
                    .AddAssets(6, "pictures", Partitioning.Invariant,
                        new AssetsFieldProperties())
                    .AddReferences(7, "friends", Partitioning.Invariant,
                        new ReferencesFieldProperties())
                    .AddString(8, "dashed-field", Partitioning.Invariant,
                        new StringFieldProperties())
                    .AddArray(9, "hobbies", Partitioning.Invariant, a => a
                        .AddString(91, "name"))
                    .Update(new SchemaProperties());

            var schema = A.Dummy<ISchemaEntity>();
            A.CallTo(() => schema.Id).Returns(Guid.NewGuid());
            A.CallTo(() => schema.Version).Returns(3);
            A.CallTo(() => schema.SchemaDef).Returns(schemaDef);

            var app = A.Dummy<IAppEntity>();
            A.CallTo(() => app.Id).Returns(Guid.NewGuid());
            A.CallTo(() => app.Version).Returns(3);
            A.CallTo(() => app.LanguagesConfig).Returns(languagesConfig);
        }

        [Fact]
        public void Should_throw_exception_for_invalid_field()
        {
            Assert.Throws<KeyNotFoundException>(() => _F(ClrFilter.Eq("data/invalid/iv", "Me")));
        }

        [Fact]
        public void Should_make_query_with_id()
        {
            var id = Guid.NewGuid();

            var i = _F(ClrFilter.Eq("id", id));
            var o = _C($"{{ '_id' : '{id}' }}");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_id_string()
        {
            var id = Guid.NewGuid().ToString();

            var i = _F(ClrFilter.Eq("id", id));
            var o = _C($"{{ '_id' : '{id}' }}");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_id_list()
        {
            var id = Guid.NewGuid();

            var i = _F(ClrFilter.In("id", new List<Guid> { id }));
            var o = _C($"{{ '_id' : {{ '$in' : ['{id}'] }} }}");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_id_string_list()
        {
            var id = Guid.NewGuid().ToString();

            var i = _F(ClrFilter.In("id", new List<string> { id }));
            var o = _C($"{{ '_id' : {{ '$in' : ['{id}'] }} }}");

            Assert.Equal(o, i);
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
            var i = _F(ClrFilter.Eq("lastModifiedBy", "Me"));
            var o = _C("{ 'mb' : 'Me' }");

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
            var i = _F(ClrFilter.Eq("createdBy", "Me"));
            var o = _C("{ 'cb' : 'Me' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_version()
        {
            var i = _F(ClrFilter.Eq("version", 0L));
            var o = _C("{ 'vs' : NumberLong(0) }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_version_and_list()
        {
            var i = _F(ClrFilter.In("version", new List<long> { 0L, 2L, 5L }));
            var o = _C("{ 'vs' : { '$in' : [NumberLong(0), NumberLong(2), NumberLong(5)] } }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_null_regex()
        {
            var i = _F(ClrFilter.Contains("createdBy", null!));
            var o = _C("{ 'cb' : /null/i }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_empty_test()
        {
            var i = _F(ClrFilter.Empty("data/firstName/iv"));
            var o = _C("{ '$or' : [{ 'do.1.iv' : { '$exists' : false } }, { 'do.1.iv' : null }, { 'do.1.iv' : '' }, { 'do.1.iv' : [] }] }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_datetime_data()
        {
            var i = _F(ClrFilter.Eq("data/birthday/iv", InstantPattern.General.Parse("1988-01-19T12:00:00Z").Value));
            var o = _C("{ 'do.5.iv' : '1988-01-19T12:00:00Z' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_underscore_field()
        {
            var i = _F(ClrFilter.Eq("data/dashed_field/iv", "Value"));
            var o = _C("{ 'do.8.iv' : 'Value' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_references_equals()
        {
            var i = _F(ClrFilter.Eq("data/friends/iv", "guid"));
            var o = _C("{ 'do.7.iv' : 'guid' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_array_field()
        {
            var i = _F(ClrFilter.Eq("data/hobbies/iv/name", "PC"));
            var o = _C("{ 'do.9.iv.91' : 'PC' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_assets_equals()
        {
            var i = _F(ClrFilter.Eq("data/pictures/iv", "guid"));
            var o = _C("{ 'do.6.iv' : 'guid' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_full_text()
        {
            var i = _Q(new ClrQuery { FullText = "Hello my World" });
            var o = _C("{ '$text' : { '$search' : 'Hello my World' } }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_orderby_with_single_field()
        {
            var i = _S(SortBuilder.Descending("data/age/iv"));
            var o = _C("{ 'do.4.iv' : -1 }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_orderby_with_multiple_fields()
        {
            var i = _S(SortBuilder.Ascending("data/age/iv"), SortBuilder.Descending("data/firstName/en"));
            var o = _C("{ 'do.4.iv' : 1, 'do.1.en' : -1 }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_take_statement()
        {
            var query = new ClrQuery { Take = 3 };
            var cursor = A.Fake<IFindFluent<MongoContentEntity, MongoContentEntity>>();

            cursor.QueryLimit(query);

            A.CallTo(() => cursor.Limit(3))
                .MustHaveHappened();
        }

        [Fact]
        public void Should_make_skip_statement()
        {
            var query = new ClrQuery { Skip = 3 };
            var cursor = A.Fake<IFindFluent<MongoContentEntity, MongoContentEntity>>();

            cursor.QuerySkip(query);

            A.CallTo(() => cursor.Skip(3))
                .MustHaveHappened();
        }

        private static string _C(string value)
        {
            return value.Replace('\'', '"');
        }

        private string _F(FilterNode<ClrValue> filter)
        {
            return _Q(new ClrQuery { Filter = filter });
        }

        private string _S(params SortNode[] sorts)
        {
            var cursor = A.Fake<IFindFluent<MongoContentEntity, MongoContentEntity>>();

            var i = string.Empty;

            A.CallTo(() => cursor.Sort(A<SortDefinition<MongoContentEntity>>._))
                .Invokes((SortDefinition<MongoContentEntity> sortDefinition) =>
                {
                    i = sortDefinition.Render(Serializer, Registry).ToString();
                });

            cursor.QuerySort(new ClrQuery { Sort = sorts.ToList() }.AdjustToModel(schemaDef));

            return i;
        }

        private string _Q(ClrQuery query)
        {
            var rendered =
                query.AdjustToModel(schemaDef).BuildFilter<MongoContentEntity>().Filter!
                    .Render(Serializer, Registry).ToString();

            return rendered;
        }
    }
}