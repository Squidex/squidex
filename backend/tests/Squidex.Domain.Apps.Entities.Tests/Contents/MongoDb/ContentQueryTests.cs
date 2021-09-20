// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
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
using Squidex.Domain.Apps.Core.Contents;
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

namespace Squidex.Domain.Apps.Entities.Contents.MongoDb
{
    public class ContentQueryTests
    {
        private static readonly IBsonSerializerRegistry Registry = BsonSerializer.SerializerRegistry;
        private static readonly IBsonSerializer<MongoContentEntity> Serializer = BsonSerializer.SerializerRegistry.GetSerializer<MongoContentEntity>();
        private readonly DomainId appId = DomainId.NewGuid();
        private readonly Schema schemaDef;
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.English.Set(Language.DE);

        static ContentQueryTests()
        {
            DomainIdSerializer.Register();

            TypeConverterStringSerializer<RefToken>.Register();
            TypeConverterStringSerializer<Status>.Register();

            InstantSerializer.Register();
        }

        public ContentQueryTests()
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
            A.CallTo(() => schema.Id).Returns(DomainId.NewGuid());
            A.CallTo(() => schema.Version).Returns(3);
            A.CallTo(() => schema.SchemaDef).Returns(schemaDef);

            var app = A.Dummy<IAppEntity>();
            A.CallTo(() => app.Id).Returns(DomainId.NewGuid());
            A.CallTo(() => app.Version).Returns(3);
            A.CallTo(() => app.Languages).Returns(languagesConfig);
        }

        [Fact]
        public void Should_make_query_with_id()
        {
            var id = Guid.NewGuid();

            var filter = ClrFilter.Eq("id", id);

            AssertQuery($"{{ '_id' : '{appId}--{id}' }}", filter);
        }

        [Fact]
        public void Should_make_query_with_id_string()
        {
            var id = DomainId.NewGuid().ToString();

            var filter = ClrFilter.Eq("id", id);

            AssertQuery($"{{ '_id' : '{appId}--{id}' }}", filter);
        }

        [Fact]
        public void Should_make_query_with_id_list()
        {
            var id = Guid.NewGuid();

            var filter = ClrFilter.In("id", new List<Guid> { id });

            AssertQuery($"{{ '_id' : {{ '$in' : ['{appId}--{id}'] }} }}", filter);
        }

        [Fact]
        public void Should_make_query_with_id_string_list()
        {
            var id = DomainId.NewGuid().ToString();

            var filter = ClrFilter.In("id", new List<string> { id });

            AssertQuery($"{{ '_id' : {{ '$in' : ['{appId}--{id}'] }} }}", filter);
        }

        [Fact]
        public void Should_make_query_with_lastModified()
        {
            var time = "1988-01-19T12:00:00Z";

            var filter = ClrFilter.Eq("lastModified", InstantPattern.ExtendedIso.Parse(time).Value);

            AssertQuery("{ 'mt' : ISODate('[value]') }", filter, time);
        }

        [Fact]
        public void Should_make_query_with_lastModifiedBy()
        {
            var filter = ClrFilter.Eq("lastModifiedBy", "me");

            AssertQuery("{ 'mb' : 'me' }", filter);
        }

        [Fact]
        public void Should_make_query_with_created()
        {
            var time = "1988-01-19T12:00:00Z";

            var filter = ClrFilter.Eq("created", InstantPattern.ExtendedIso.Parse(time).Value);

            AssertQuery("{ 'ct' : ISODate('[value]') }", filter, time);
        }

        [Fact]
        public void Should_make_query_with_createdBy()
        {
            var filter = ClrFilter.Eq("createdBy", "subject:me");

            AssertQuery("{ 'cb' : 'subject:me' }", filter);
        }

        [Fact]
        public void Should_make_query_with_version()
        {
            var filter = ClrFilter.Eq("version", 2L);

            AssertQuery("{ 'vs' : NumberLong(2) }", filter);
        }

        [Fact]
        public void Should_make_query_with_datetime_data()
        {
            var time = "1988-01-19T12:00:00Z";

            var filter = ClrFilter.Eq("data/birthday/iv", InstantPattern.General.Parse(time).Value);

            AssertQuery("{ 'do.birthday.iv' : '[value]' }", filter, time);
        }

        [Fact]
        public void Should_make_query_with_underscore_field()
        {
            var filter = ClrFilter.Eq("data/dashed_field/iv", "Value");

            AssertQuery("{ 'do.dashed-field.iv' : 'Value' }", filter);
        }

        [Fact]
        public void Should_make_query_with_references_equals()
        {
            var filter = ClrFilter.Eq("data/friends/iv", "guid");

            AssertQuery("{ 'do.friends.iv' : 'guid' }", filter);
        }

        [Fact]
        public void Should_make_query_with_array_field()
        {
            var filter = ClrFilter.Eq("data/hobbies/iv/name", "PC");

            AssertQuery("{ 'do.hobbies.iv.name' : 'PC' }", filter);
        }

        [Fact]
        public void Should_make_query_with_assets_equals()
        {
            var filter = ClrFilter.Eq("data/pictures/iv", "guid");

            AssertQuery("{ 'do.pictures.iv' : 'guid' }", filter);
        }

        [Fact]
        public void Should_make_orderby_with_single_field()
        {
            var sorting = SortBuilder.Descending("data/age/iv");

            AssertSorting("{ 'do.age.iv' : -1 }", sorting);
        }

        [Fact]
        public void Should_make_orderby_with_multiple_fields()
        {
            var sorting1 = SortBuilder.Ascending("data/age/iv");
            var sorting2 = SortBuilder.Descending("data/firstName/en");

            AssertSorting("{ 'do.age.iv' : 1, 'do.firstName.en' : -1 }", sorting1, sorting2);
        }

        private void AssertQuery(string expected, FilterNode<ClrValue> filter, object? arg = null)
        {
            AssertQuery(new ClrQuery { Filter = filter }, expected, arg);
        }

        private void AssertQuery(ClrQuery query, string expected, object? arg = null)
        {
            var rendered =
                query.AdjustToModel(appId).BuildFilter<MongoContentEntity>().Filter!
                    .Render(Serializer, Registry).ToString();

            var expectation = Cleanup(expected, arg);

            Assert.Equal(expectation, rendered);
        }

        private void AssertSorting(string expected, params SortNode[] sort)
        {
            var cursor = A.Fake<IFindFluent<MongoContentEntity, MongoContentEntity>>();

            var rendered = string.Empty;

            A.CallTo(() => cursor.Sort(A<SortDefinition<MongoContentEntity>>._))
                .Invokes((SortDefinition<MongoContentEntity> sortDefinition) =>
                {
                    rendered = sortDefinition.Render(Serializer, Registry).ToString();
                });

            cursor.QuerySort(new ClrQuery { Sort = sort.ToList() }.AdjustToModel(appId));

            var expectation = Cleanup(expected);

            Assert.Equal(expectation, rendered);
        }

        private static string Cleanup(string filter, object? arg = null)
        {
            return filter.Replace('\'', '"').Replace("[value]", arg?.ToString(), StringComparison.Ordinal);
        }
    }
}
