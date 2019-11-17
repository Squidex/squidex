// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.OData.Edm;
using Squidex.Infrastructure.Queries.OData;
using Xunit;

namespace Squidex.Infrastructure.Queries
{
    public class QueryODataConversionTests
    {
        private static readonly IEdmModel EdmModel;

        static QueryODataConversionTests()
        {
            var entityType = new EdmEntityType("Squidex", "Users");

            entityType.AddStructuralProperty("id", EdmPrimitiveTypeKind.Guid, false);
            entityType.AddStructuralProperty("idNullable", EdmPrimitiveTypeKind.Guid, true);
            entityType.AddStructuralProperty("created", EdmPrimitiveTypeKind.DateTimeOffset, false);
            entityType.AddStructuralProperty("createdNullable", EdmPrimitiveTypeKind.DateTimeOffset, true);
            entityType.AddStructuralProperty("isComicFigure", EdmPrimitiveTypeKind.Boolean, false);
            entityType.AddStructuralProperty("isComicFigureNullable", EdmPrimitiveTypeKind.Boolean, true);
            entityType.AddStructuralProperty("firstName", EdmPrimitiveTypeKind.String, true);
            entityType.AddStructuralProperty("firstNameNullable", EdmPrimitiveTypeKind.String, false);
            entityType.AddStructuralProperty("lastName", EdmPrimitiveTypeKind.String, true);
            entityType.AddStructuralProperty("birthday", EdmPrimitiveTypeKind.Date, false);
            entityType.AddStructuralProperty("birthdayNullable", EdmPrimitiveTypeKind.Date, true);
            entityType.AddStructuralProperty("incomeCents", EdmPrimitiveTypeKind.Int64, false);
            entityType.AddStructuralProperty("incomeCentsNullable", EdmPrimitiveTypeKind.Int64, true);
            entityType.AddStructuralProperty("incomeMio", EdmPrimitiveTypeKind.Double, false);
            entityType.AddStructuralProperty("incomeMioNullable", EdmPrimitiveTypeKind.Double, true);
            entityType.AddStructuralProperty("age", EdmPrimitiveTypeKind.Int32, false);
            entityType.AddStructuralProperty("ageNullable", EdmPrimitiveTypeKind.Int32, true);

            var container = new EdmEntityContainer("Squidex", "Container");

            container.AddEntitySet("UserSet", entityType);

            var model = new EdmModel();

            model.AddElement(container);
            model.AddElement(entityType);

            EdmModel = model;
        }

        [Fact]
        public void Should_parse_query()
        {
            var parser = EdmModel.ParseQuery("$filter=firstName eq 'Dagobert'");

            Assert.NotNull(parser);
        }

        [Theory]
        [InlineData("created")]
        [InlineData("createdNullable")]
        public void Should_parse_filter_when_type_is_datetime(string field)
        {
            var i = Q($"$filter={field} eq 1988-01-19T12:00:00Z");
            var o = C($"Filter: {field} == 1988-01-19T12:00:00Z");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_when_type_is_datetime_list()
        {
            var i = Q("$filter=created in ('1988-01-19T12:00:00Z')");
            var o = C("Filter: created in [1988-01-19T12:00:00Z]");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_when_type_is_datetime_and_and_value_is_date()
        {
            var i = Q("$filter=created eq 1988-01-19");
            var o = C("Filter: created == 1988-01-19T00:00:00Z");

            Assert.Equal(o, i);
        }

        [Theory]
        [InlineData("birthday")]
        [InlineData("birthdayNullable")]
        public void Should_parse_filter_when_type_is_date(string field)
        {
            var i = Q($"$filter={field} eq 1988-01-19");
            var o = C($"Filter: {field} == 1988-01-19T00:00:00Z");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_when_type_is_date_list()
        {
            var i = Q("$filter=birthday in ('1988-01-19')");
            var o = C("Filter: birthday in [1988-01-19T00:00:00Z]");

            Assert.Equal(o, i);
        }

        [Theory]
        [InlineData("id")]
        [InlineData("idNullable")]
        public void Should_parse_filter_when_type_is_guid(string field)
        {
            var i = Q($"$filter={field} eq B5FE25E3-B262-4B17-91EF-B3772A6B62BB");
            var o = C($"Filter: {field} == b5fe25e3-b262-4b17-91ef-b3772a6b62bb");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_when_type_is_guid_list()
        {
            var i = Q("$filter=id in ('B5FE25E3-B262-4B17-91EF-B3772A6B62BB')");
            var o = C("Filter: id in [b5fe25e3-b262-4b17-91ef-b3772a6b62bb]");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_when_type_is_null()
        {
            var i = Q("$filter=firstName eq null");
            var o = C("Filter: firstName == null");

            Assert.Equal(o, i);
        }

        [Theory]
        [InlineData("firstName")]
        [InlineData("firstNameNullable")]
        public void Should_parse_filter_when_type_is_string(string field)
        {
            var i = Q($"$filter={field} eq 'Dagobert'");
            var o = C($"Filter: {field} == 'Dagobert'");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_when_type_is_string_list()
        {
            var i = Q("$filter=firstName in ('Dagobert')");
            var o = C("Filter: firstName in ['Dagobert']");

            Assert.Equal(o, i);
        }

        [Theory]
        [InlineData("isComicFigure")]
        [InlineData("isComicFigureNullable")]
        public void Should_parse_filter_when_type_is_boolean(string field)
        {
            var i = Q($"$filter={field} eq true");
            var o = C($"Filter: {field} == True");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_when_type_is_boolean_list()
        {
            var i = Q("$filter=isComicFigure in (true)");
            var o = C("Filter: isComicFigure in [True]");

            Assert.Equal(o, i);
        }

        [Theory]
        [InlineData("age")]
        [InlineData("ageNullable")]
        public void Should_parse_filter_when_type_is_int32(string field)
        {
            var i = Q($"$filter={field} eq 60");
            var o = C($"Filter: {field} == 60");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_when_type_is_int32_list()
        {
            var i = Q("$filter=age in (60)");
            var o = C("Filter: age in [60]");

            Assert.Equal(o, i);
        }

        [Theory]
        [InlineData("incomeCents")]
        [InlineData("incomeCentsNullable")]
        public void Should_parse_filter_when_type_is_int64(string field)
        {
            var i = Q($"$filter={field} eq 31543143513456789");
            var o = C($"Filter: {field} == 31543143513456789");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_when_type_is_int64_list()
        {
            var i = Q("$filter=incomeCents in (31543143513456789)");
            var o = C("Filter: incomeCents in [31543143513456789]");

            Assert.Equal(o, i);
        }

        [Theory]
        [InlineData("incomeMio")]
        [InlineData("incomeMioNullable")]
        public void Should_parse_filter_when_type_is_double(string field)
        {
            var i = Q($"$filter={field} eq 5634474356.1233");
            var o = C($"Filter: {field} == 5634474356.1233");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_when_type_is_double_list()
        {
            var i = Q("$filter=incomeMio in (5634474356.1233)");
            var o = C("Filter: incomeMio in [5634474356.1233]");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_negation()
        {
            var i = Q("$filter=not endswith(lastName, 'Duck')");
            var o = C("Filter: !(endsWith(lastName, 'Duck'))");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_startswith()
        {
            var i = Q("$filter=startswith(lastName, 'Duck')");
            var o = C("Filter: startsWith(lastName, 'Duck')");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_endswith()
        {
            var i = Q("$filter=endswith(lastName, 'Duck')");
            var o = C("Filter: endsWith(lastName, 'Duck')");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_empty()
        {
            var i = Q("$filter=empty(lastName)");
            var o = C("Filter: empty(lastName)");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_empty_to_true()
        {
            var i = Q("$filter=empty(lastName) eq true");
            var o = C("Filter: empty(lastName)");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_contains()
        {
            var i = Q("$filter=contains(lastName, 'Duck')");
            var o = C("Filter: contains(lastName, 'Duck')");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_contains_to_true()
        {
            var i = Q("$filter=contains(lastName, 'Duck') eq true");
            var o = C("Filter: contains(lastName, 'Duck')");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_contains_to_false()
        {
            var i = Q("$filter=contains(lastName, 'Duck') eq false");
            var o = C("Filter: !(contains(lastName, 'Duck'))");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_equals()
        {
            var i = Q("$filter=age eq 1");
            var o = C("Filter: age == 1");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_notequals()
        {
            var i = Q("$filter=age ne 1");
            var o = C("Filter: age != 1");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_lessthan()
        {
            var i = Q("$filter=age lt 1");
            var o = C("Filter: age < 1");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_lessthanorequal()
        {
            var i = Q("$filter=age le 1");
            var o = C("Filter: age <= 1");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_greaterthan()
        {
            var i = Q("$filter=age gt 1");
            var o = C("Filter: age > 1");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_greaterthanorequal()
        {
            var i = Q("$filter=age ge 1");
            var o = C("Filter: age >= 1");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_conjunction_and_contains()
        {
            var i = Q("$filter=contains(firstName, 'Sebastian') eq false and isComicFigure eq true");
            var o = C("Filter: (!(contains(firstName, 'Sebastian')) && isComicFigure == True)");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_conjunction()
        {
            var i = Q("$filter=age eq 1 and age eq 2");
            var o = C("Filter: (age == 1 && age == 2)");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_disjunction()
        {
            var i = Q("$filter=age eq 1 or age eq 2");
            var o = C("Filter: (age == 1 || age == 2)");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_full_text_numbers()
        {
            var i = Q("$search=\"33k\"");
            var o = C("FullText: '33k'");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_full_text()
        {
            var i = Q("$search=Duck");
            var o = C("FullText: 'Duck'");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_full_text_and_multiple_terms()
        {
            var i = Q("$search=Dagobert or Donald");
            var o = C("FullText: 'Dagobert or Donald'");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_orderby_with_single_field()
        {
            var i = Q("$orderby=age desc");
            var o = C("Sort: age Descending");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_orderby_with_multiple_field()
        {
            var i = Q("$orderby=age, incomeMio desc");
            var o = C("Sort: age Ascending, incomeMio Descending");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_and_take()
        {
            var i = Q("$top=3&$skip=4");
            var o = C("Skip: 4; Take: 3");

            Assert.Equal(o, i);
        }

        private static string C(string value)
        {
            return value;
        }

        private static string? Q(string value)
        {
            var parser = EdmModel.ParseQuery(value);

            return parser?.ToQuery().ToString();
        }
    }
}