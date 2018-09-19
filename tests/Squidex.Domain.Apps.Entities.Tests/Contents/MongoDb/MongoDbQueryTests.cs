// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Immutable;
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
using Squidex.Domain.Apps.Entities.MongoDb.Contents.Visitors;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.MongoDb.OData;
using Squidex.Infrastructure.Queries;
using Xunit;
using FilterBuilder = Squidex.Infrastructure.Queries.FilterBuilder;
using SortBuilder = Squidex.Infrastructure.Queries.SortBuilder;

namespace Squidex.Domain.Apps.Entities.Contents.MongoDb
{
    public class MongoDbQueryTests
    {
        private static readonly IBsonSerializerRegistry Registry = BsonSerializer.SerializerRegistry;
        private static readonly IBsonSerializer<MongoContentEntity> Serializer = BsonSerializer.SerializerRegistry.GetSerializer<MongoContentEntity>();
        private readonly Schema schemaDef;
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.Build(Language.EN, Language.DE);

        static MongoDbQueryTests()
        {
            InstantSerializer.Register();
        }

        public MongoDbQueryTests()
        {
            schemaDef =
                new Schema("user")
                    .AddString(1, "firstName", Partitioning.Language,
                        new StringFieldProperties { Label = "FirstName", IsRequired = true, AllowedValues = ImmutableList.Create("1", "2") })
                    .AddString(2, "lastName", Partitioning.Language,
                        new StringFieldProperties { Hints = "Last Name", Editor = StringFieldEditor.Input })
                    .AddBoolean(3, "isAdmin", Partitioning.Invariant,
                        new BooleanFieldProperties())
                    .AddNumber(4, "age", Partitioning.Invariant,
                        new NumberFieldProperties { MinValue = 1, MaxValue = 10 })
                    .AddDateTime(5, "birthday", Partitioning.Invariant,
                        new DateTimeFieldProperties())
                    .AddAssets(6, "pictures", Partitioning.Invariant,
                        new AssetsFieldProperties())
                    .AddReferences(7, "friends", Partitioning.Invariant,
                        new ReferencesFieldProperties())
                    .AddString(8, "dashed-field", Partitioning.Invariant,
                        new StringFieldProperties())
                    .Update(new SchemaProperties { Hints = "The User" });

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
            Assert.Throws<NotSupportedException>(() => F(FilterBuilder.Eq("data/invalid/iv", "Me")));
        }

        [Fact]
        public void Should_make_query_with_lastModified()
        {
            var i = F(FilterBuilder.Eq("lastModified", InstantPattern.General.Parse("1988-01-19T12:00:00Z").Value));
            var o = C("{ 'mt' : '1988-01-19T12:00:00Z' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_lastModifiedBy()
        {
            var i = F(FilterBuilder.Eq("lastModifiedBy", "Me"));
            var o = C("{ 'mb' : 'Me' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_created()
        {
            var i = F(FilterBuilder.Eq("created", InstantPattern.General.Parse("1988-01-19T12:00:00Z").Value));
            var o = C("{ 'ct' : '1988-01-19T12:00:00Z' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_createdBy()
        {
            var i = F(FilterBuilder.Eq("createdBy", "Me"));
            var o = C("{ 'cb' : 'Me' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_version()
        {
            var i = F(FilterBuilder.Eq("version", 0L));
            var o = C("{ 'vs' : NumberLong(0) }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_version_and_list()
        {
            var i = F(FilterBuilder.In("version", 0L, 2L, 5L));
            var o = C("{ 'vs' : { '$in' : [NumberLong(0), NumberLong(2), NumberLong(5)] } }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_from_draft()
        {
            var i = F(FilterBuilder.Eq("data/dashed_field/iv", "Value"), true);
            var o = C("{ 'dd.8.iv' : 'Value' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_date_field_created()
        {
            var i = F(FilterBuilder.Eq("data/birthday/iv", InstantPattern.General.Parse("1988-01-19T12:00:00Z").Value));
            var o = C("{ 'do.5.iv' : '1988-01-19T12:00:00Z' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_underscore_field()
        {
            var i = F(FilterBuilder.Eq("data/dashed_field/iv", "Value"));
            var o = C("{ 'do.8.iv' : 'Value' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_references_equals()
        {
            var i = F(FilterBuilder.Eq("data/friends/iv", "guid"));
            var o = C("{ 'do.7.iv' : 'guid' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_assets_equals()
        {
            var i = F(FilterBuilder.Eq("data/pictures/iv", "guid"));
            var o = C("{ 'do.6.iv' : 'guid' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_full_text()
        {
            var i = Q(new Query { FullText = "Hello my World" });
            var o = C("{ '$text' : { '$search' : 'Hello my World' } }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_orderby_with_single_field()
        {
            var i = S(SortBuilder.Descending("data/age/iv"));
            var o = C("{ 'do.4.iv' : -1 }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_orderby_with_multiple_fields()
        {
            var i = S(SortBuilder.Ascending("data/age/iv"), SortBuilder.Descending("data/firstName/en"));
            var o = C("{ 'do.4.iv' : 1, 'do.1.en' : -1 }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_take_statement()
        {
            var query = new Query { Take = 3 };
            var cursor = A.Fake<IFindFluent<MongoContentEntity, MongoContentEntity>>();

            cursor.ContentTake(query.AdjustToModel(schemaDef, false));

            A.CallTo(() => cursor.Limit(3)).MustHaveHappened();
        }

        [Fact]
        public void Should_make_skip_statement()
        {
            var query = new Query { Skip = 3 };
            var cursor = A.Fake<IFindFluent<MongoContentEntity, MongoContentEntity>>();

            cursor.ContentSkip(query.AdjustToModel(schemaDef, false));

            A.CallTo(() => cursor.Skip(3)).MustHaveHappened();
        }

        private static string C(string value)
        {
            return value.Replace('\'', '"');
        }

        private string F(FilterNode filter, bool useDraft = false)
        {
            return Q(new Query { Filter = filter }, useDraft);
        }

        private string S(params SortNode[] sorts)
        {
            var cursor = A.Fake<IFindFluent<MongoContentEntity, MongoContentEntity>>();

            var i = string.Empty;

            A.CallTo(() => cursor.Sort(A<SortDefinition<MongoContentEntity>>.Ignored))
                .Invokes((SortDefinition<MongoContentEntity> sortDefinition) =>
                {
                    i = sortDefinition.Render(Serializer, Registry).ToString();
                });

            cursor.ContentSort(new Query { Sort = sorts.ToList() }.AdjustToModel(schemaDef, false));

            return i;
        }

        private string Q(Query query, bool useDraft = false)
        {
            var rendered =
                query.AdjustToModel(schemaDef, useDraft).BuildFilter<MongoContentEntity>().Filter
                    .Render(Serializer, Registry).ToString();

            return rendered;
        }
    }
}