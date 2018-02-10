// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Immutable;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.OData.Edm;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents.Edm;
using Squidex.Domain.Apps.Entities.MongoDb.Contents;
using Squidex.Domain.Apps.Entities.MongoDb.Contents.Visitors;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.MongoDb.OData;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.OData
{
    public class ODataQueryTests
    {
        private readonly Schema schemaDef;
        private readonly IBsonSerializerRegistry registry = BsonSerializer.SerializerRegistry;
        private readonly IBsonSerializer<MongoContentEntity> serializer = BsonSerializer.SerializerRegistry.GetSerializer<MongoContentEntity>();
        private readonly IEdmModel edmModel;
        private readonly LanguagesConfig languagesConfig = LanguagesConfig.Build(Language.EN, Language.DE);

        static ODataQueryTests()
        {
            InstantSerializer.Register();
        }

        public ODataQueryTests()
        {
            schemaDef =
                new Schema("user")
                    .AddField(new StringField(1, "firstName", Partitioning.Language,
                        new StringFieldProperties { Label = "FirstName", IsRequired = true, AllowedValues = ImmutableList.Create("1", "2") }))
                    .AddField(new StringField(2, "lastName", Partitioning.Language,
                        new StringFieldProperties { Hints = "Last Name", Editor = StringFieldEditor.Input }))
                    .AddField(new BooleanField(3, "isAdmin", Partitioning.Invariant,
                        new BooleanFieldProperties()))
                    .AddField(new NumberField(4, "age", Partitioning.Invariant,
                        new NumberFieldProperties { MinValue = 1, MaxValue = 10 }))
                    .AddField(new DateTimeField(5, "birthday", Partitioning.Invariant,
                        new DateTimeFieldProperties()))
                    .AddField(new AssetsField(6, "pictures", Partitioning.Invariant,
                        new AssetsFieldProperties()))
                    .AddField(new ReferencesField(7, "friends", Partitioning.Invariant,
                        new ReferencesFieldProperties()))
                    .AddField(new StringField(8, "dashed-field", Partitioning.Invariant,
                        new StringFieldProperties()))
                    .Update(new SchemaProperties { Hints = "The User" });

            var builder = new EdmModelBuilder(new MemoryCache(Options.Create(new MemoryCacheOptions())));

            var schema = A.Dummy<ISchemaEntity>();
            A.CallTo(() => schema.Id).Returns(Guid.NewGuid());
            A.CallTo(() => schema.Version).Returns(3);
            A.CallTo(() => schema.SchemaDef).Returns(schemaDef);

            var app = A.Dummy<IAppEntity>();
            A.CallTo(() => app.Id).Returns(Guid.NewGuid());
            A.CallTo(() => app.Version).Returns(3);
            A.CallTo(() => app.LanguagesConfig).Returns(languagesConfig);

            edmModel = builder.BuildEdmModel(schema, app);
        }

        [Fact]
        public void Should_parse_query()
        {
            var parser = edmModel.ParseQuery("$filter=data/firstName/de eq 'Sebastian'");

            Assert.NotNull(parser);
        }

        [Fact]
        public void Should_make_query_with_created()
        {
            var i = F("$filter=created eq 1988-01-19T12:00:00Z");
            var o = C("{ 'ct' : ISODate('1988-01-19T12:00:00Z') }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_createdBy()
        {
            var i = F("$filter=createdBy eq 'Me'");
            var o = C("{ 'cb' : 'Me' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_lastModified()
        {
            var i = F("$filter=lastModified eq 1988-01-19T12:00:00Z");
            var o = C("{ 'mt' : ISODate('1988-01-19T12:00:00Z') }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_lastModifiedBy()
        {
            var i = F("$filter=lastModifiedBy eq 'Me'");
            var o = C("{ 'mb' : 'Me' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_version()
        {
            var i = F("$filter=version eq 0");
            var o = C("{ 'vs' : 0 }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_underscore_field()
        {
            var i = F("$filter=data/dashed_field/iv eq 'Value'");
            var o = C("{ 'do.8.iv' : 'Value' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_not()
        {
            var i = F("$filter=not endswith(data/firstName/de, 'Sebastian')");
            var o = C("{ 'do.1.de' : { '$not' : /Sebastian$/i } }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_startswith()
        {
            var i = F("$filter=startswith(data/firstName/de, 'Sebastian')");
            var o = C("{ 'do.1.de' : /^Sebastian/i }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_endswith()
        {
            var i = F("$filter=endswith(data/firstName/de, 'Sebastian')");
            var o = C("{ 'do.1.de' : /Sebastian$/i }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_cointains()
        {
            var i = F("$filter=contains(data/firstName/de, 'Sebastian')");
            var o = C("{ 'do.1.de' : /Sebastian/i }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_equals()
        {
            var i = F("$filter=contains(data/firstName/de, 'Sebastian') eq true");
            var o = C("{ 'do.1.de' : /Sebastian/i }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_wih_equals_to_false()
        {
            var i = F("$filter=contains(data/firstName/de, 'Sebastian') eq false");
            var o = C("{ 'do.1.de' : { '$not' : /Sebastian/i } }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_conjunction_and_contains()
        {
            var i = F("$filter=contains(data/firstName/de, 'Sebastian') eq false and data/isAdmin/iv eq true");
            var o = C("{ 'do.1.de' : { '$not' : /Sebastian/i }, 'do.3.iv' : true }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_string_equals()
        {
            var i = F("$filter=data/firstName/de eq 'Sebastian'");
            var o = C("{ 'do.1.de' : 'Sebastian' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_datetime_equals()
        {
            var i = F("$filter=data/birthday/iv eq 1988-01-19T12:00:00Z");
            var o = C("{ 'do.5.iv' : ISODate('1988-01-19T12:00:00Z') }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_boolean_equals()
        {
            var i = F("$filter=data/isAdmin/iv eq true");
            var o = C("{ 'do.3.iv' : true }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_notequals()
        {
            var i = F("$filter=data/firstName/de ne 'Sebastian'");
            var o = C("{ '$or' : [{ 'do.1.de' : { '$exists' : false } }, { 'do.1.de' : { '$ne' : 'Sebastian' } }] }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_lessthan()
        {
            var i = F("$filter=data/age/iv lt 1");
            var o = C("{ 'do.4.iv' : { '$lt' : 1.0 } }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_lessequals()
        {
            var i = F("$filter=data/age/iv le 1");
            var o = C("{ 'do.4.iv' : { '$lte' : 1.0 } }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_greaterthan()
        {
            var i = F("$filter=data/age/iv gt 1");
            var o = C("{ 'do.4.iv' : { '$gt' : 1.0 } }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_greaterequals()
        {
            var i = F("$filter=data/age/iv ge 1");
            var o = C("{ 'do.4.iv' : { '$gte' : 1.0 } }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_references_equals()
        {
            var i = F("$filter=data/pictures/iv eq 'guid'");
            var o = C("{ 'do.6.iv' : 'guid' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_assets_equals()
        {
            var i = F("$filter=data/friends/iv eq 'guid'");
            var o = C("{ 'do.7.iv' : 'guid' }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_conjunction()
        {
            var i = F("$filter=data/age/iv eq 1 and data/age/iv eq 2");
            var o = C("{ '$and' : [{ 'do.4.iv' : 1.0 }, { 'do.4.iv' : 2.0 }] }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_disjunction()
        {
            var i = F("$filter=data/age/iv eq 1 or data/age/iv eq 2");
            var o = C("{ '$or' : [{ 'do.4.iv' : 1.0 }, { 'do.4.iv' : 2.0 }] }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_full_text()
        {
            var i = F("$search=Hello my World");
            var o = C("{ '$text' : { '$search' : 'Hello my World' } }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_query_with_full_text_and_multiple_terms()
        {
            var i = F("$search=A and B");
            var o = C("{ '$text' : { '$search' : 'A and B' } }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_orderby_with_single_field()
        {
            var i = S("$orderby=data/age/iv desc");
            var o = C("{ 'do.4.iv' : -1 }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_orderby_with_multiple_field()
        {
            var i = S("$orderby=data/age/iv, data/firstName/en desc");
            var o = C("{ 'do.4.iv' : 1, 'do.1.en' : -1 }");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_top_statement()
        {
            var parser = edmModel.ParseQuery("$top=3");
            var cursor = A.Fake<IFindFluent<MongoContentEntity, MongoContentEntity>>();

            cursor.ContentTake(parser);

            A.CallTo(() => cursor.Limit(3)).MustHaveHappened();
        }

        [Fact]
        public void Should_make_top_statement_with_limit()
        {
            var parser = edmModel.ParseQuery("$top=300");
            var cursor = A.Fake<IFindFluent<MongoContentEntity, MongoContentEntity>>();

            cursor.ContentTake(parser);

            A.CallTo(() => cursor.Limit(200)).MustHaveHappened();
        }

        [Fact]
        public void Should_make_top_statement_with_default_value()
        {
            var parser = edmModel.ParseQuery(string.Empty);
            var cursor = A.Fake<IFindFluent<MongoContentEntity, MongoContentEntity>>();

            cursor.ContentTake(parser);

            A.CallTo(() => cursor.Limit(20)).MustHaveHappened();
        }

        [Fact]
        public void Should_make_skip_statement()
        {
            var parser = edmModel.ParseQuery("$skip=3");
            var cursor = A.Fake<IFindFluent<MongoContentEntity, MongoContentEntity>>();

            cursor.ContentSkip(parser);

            A.CallTo(() => cursor.Skip(3)).MustHaveHappened();
        }

        [Fact]
        public void Should_make_skip_statement_with_default_value()
        {
            var parser = edmModel.ParseQuery(string.Empty);
            var cursor = A.Fake<IFindFluent<MongoContentEntity, MongoContentEntity>>();

            cursor.ContentSkip(parser);

            A.CallTo(() => cursor.Skip(A<int>.Ignored)).MustNotHaveHappened();
        }

        private static string C(string value)
        {
            return value.Replace('\'', '"');
        }

        private string S(string value)
        {
            var parser = edmModel.ParseQuery(value);
            var cursor = A.Fake<IFindFluent<MongoContentEntity, MongoContentEntity>>();

            var i = string.Empty;

            A.CallTo(() => cursor.Sort(A<SortDefinition<MongoContentEntity>>.Ignored))
                .Invokes((SortDefinition<MongoContentEntity> sortDefinition) =>
                {
                    i = sortDefinition.Render(serializer, registry).ToString();
                });

            cursor.ContentSort(parser, FindExtensions.CreatePropertyCalculator(schemaDef));

            return i;
        }

        private string F(string value)
        {
            var parser = edmModel.ParseQuery(value);

            var query =
                parser.BuildFilter<MongoContentEntity>(FindExtensions.CreatePropertyCalculator(schemaDef))
                    .Filter.Render(serializer, registry).ToString();

            return query;
        }
    }
}