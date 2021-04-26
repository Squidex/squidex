// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.OData.Edm;
using Squidex.Infrastructure.Queries.OData;
using Xunit;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable IDE1006 // Naming Styles

namespace Squidex.Infrastructure.Queries
{
    public class QueryFromODataTests
    {
        private static readonly IEdmModel EdmModel;

        static QueryFromODataTests()
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
            entityType.AddStructuralProperty("geo", EdmPrimitiveTypeKind.GeographyPoint, false);
            entityType.AddStructuralProperty("geoNullable", EdmPrimitiveTypeKind.GeographyPoint, true);
            entityType.AddStructuralProperty("age", EdmPrimitiveTypeKind.Int32, false);
            entityType.AddStructuralProperty("ageNullable", EdmPrimitiveTypeKind.Int32, true);
            entityType.AddStructuralProperty("properties", new EdmComplexTypeReference(new EdmComplexType("Squidex", "Properties", null, false, true), true));

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
        [InlineData("properties/datetime")]
        [InlineData("properties/nested/datetime")]
        public void Should_parse_filter_if_type_is_datetime(string field)
        {
            var i = _Q($"$filter={field} eq 1988-01-19T12:00:00Z");
            var o = _C($"Filter: {field} == 1988-01-19T12:00:00Z");

            Assert.Equal(o, i);
        }

        [Theory]
        [InlineData("created")]
        [InlineData("createdNullable")]
        [InlineData("properties/datetime")]
        [InlineData("properties/nested/datetime")]
        public void Should_parse_filter_if_type_is_datetime_and_value_is_null(string field)
        {
            var i = _Q($"$filter={field} eq null");
            var o = _C($"Filter: {field} == null");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_if_type_is_datetime_list()
        {
            var i = _Q("$filter=created in ('1988-01-19T12:00:00Z')");
            var o = _C("Filter: created in [1988-01-19T12:00:00Z]");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_if_type_is_datetime_and_and_value_is_date()
        {
            var i = _Q("$filter=created eq 1988-01-19");
            var o = _C("Filter: created == 1988-01-19T00:00:00Z");

            Assert.Equal(o, i);
        }

        [Theory]
        [InlineData("birthday")]
        [InlineData("birthdayNullable")]
        [InlineData("properties/date")]
        [InlineData("properties/nested/date")]
        public void Should_parse_filter_if_type_is_date(string field)
        {
            var i = _Q($"$filter={field} eq 1988-01-19");
            var o = _C($"Filter: {field} == 1988-01-19T00:00:00Z");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_if_type_is_date_list()
        {
            var i = _Q("$filter=birthday in ('1988-01-19')");
            var o = _C("Filter: birthday in [1988-01-19T00:00:00Z]");

            Assert.Equal(o, i);
        }

        [Theory]
        [InlineData("id")]
        [InlineData("idNullable")]
        [InlineData("properties/uid")]
        [InlineData("properties/nested/guid")]
        public void Should_parse_filter_if_type_is_guid(string field)
        {
            var i = _Q($"$filter={field} eq B5FE25E3-B262-4B17-91EF-B3772A6B62BB");
            var o = _C($"Filter: {field} == b5fe25e3-b262-4b17-91ef-b3772a6b62bb");

            Assert.Equal(o, i);
        }

        [Theory]
        [InlineData("id")]
        [InlineData("idNullable")]
        [InlineData("properties/uid")]
        [InlineData("properties/nested/guid")]
        public void Should_parse_filter_if_type_is_guid_and_value_is_null(string field)
        {
            var i = _Q($"$filter={field} eq null");
            var o = _C($"Filter: {field} == null");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_if_type_is_guid_list()
        {
            var i = _Q("$filter=id in ('B5FE25E3-B262-4B17-91EF-B3772A6B62BB')");
            var o = _C("Filter: id in [b5fe25e3-b262-4b17-91ef-b3772a6b62bb]");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_if_type_is_null()
        {
            var i = _Q("$filter=firstName eq null");
            var o = _C("Filter: firstName == null");

            Assert.Equal(o, i);
        }

        [Theory]
        [InlineData("firstName")]
        [InlineData("firstNameNullable")]
        [InlineData("properties/string")]
        [InlineData("properties/nested/string")]
        public void Should_parse_filter_if_type_is_string(string field)
        {
            var i = _Q($"$filter={field} eq 'Dagobert'");
            var o = _C($"Filter: {field} == 'Dagobert'");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_if_type_is_string_list()
        {
            var i = _Q("$filter=firstName in ('Dagobert')");
            var o = _C("Filter: firstName in ['Dagobert']");

            Assert.Equal(o, i);
        }

        [Theory]
        [InlineData("isComicFigure")]
        [InlineData("isComicFigureNullable")]
        [InlineData("properties/boolean")]
        [InlineData("properties/nested/boolean")]
        public void Should_parse_filter_if_type_is_boolean(string field)
        {
            var i = _Q($"$filter={field} eq true");
            var o = _C($"Filter: {field} == True");

            Assert.Equal(o, i);
        }

        [Theory]
        [InlineData("isComicFigure")]
        [InlineData("isComicFigureNullable")]
        [InlineData("properties/boolean")]
        [InlineData("properties/nested/boolean")]
        public void Should_parse_filter_if_type_is_boolean_and_value_is_null(string field)
        {
            var i = _Q($"$filter={field} eq null");
            var o = _C($"Filter: {field} == null");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_if_type_is_boolean_list()
        {
            var i = _Q("$filter=isComicFigure in (true)");
            var o = _C("Filter: isComicFigure in [True]");

            Assert.Equal(o, i);
        }

        [Theory]
        [InlineData("age")]
        [InlineData("ageNullable")]
        [InlineData("properties/int")]
        [InlineData("properties/nested/int")]
        public void Should_parse_filter_if_type_is_int32(string field)
        {
            var i = _Q($"$filter={field} eq 60");
            var o = _C($"Filter: {field} == 60");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_if_type_is_int32_list()
        {
            var i = _Q("$filter=age in (60)");
            var o = _C("Filter: age in [60]");

            Assert.Equal(o, i);
        }

        [Theory]
        [InlineData("incomeCents")]
        [InlineData("incomeCentsNullable")]
        [InlineData("properties/long")]
        [InlineData("properties/nested/long")]
        public void Should_parse_filter_if_type_is_int64(string field)
        {
            var i = _Q($"$filter={field} eq 31543143513456789");
            var o = _C($"Filter: {field} == 31543143513456789");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_if_type_is_int64_list()
        {
            var i = _Q("$filter=incomeCents in (31543143513456789)");
            var o = _C("Filter: incomeCents in [31543143513456789]");

            Assert.Equal(o, i);
        }

        [Theory]
        [InlineData("incomeMio")]
        [InlineData("incomeMioNullable")]
        [InlineData("properties/double")]
        [InlineData("properties/nested/double")]
        public void Should_parse_filter_if_type_is_double(string field)
        {
            var i = _Q($"$filter={field} eq 5634474356.1233");
            var o = _C($"Filter: {field} == 5634474356.1233");

            Assert.Equal(o, i);
        }

        [Theory]
        [InlineData("geo")]
        [InlineData("geoNullable")]
        [InlineData("properties/geo")]
        [InlineData("properties/nested/geo")]
        public void Should_parse_filter_if_type_is_geograph(string field)
        {
            var i = _Q($"$filter=geo.distance({field}, geography'POINT(10 20)') lt 30.0");
            var o = _C($"Filter: {field} < Radius(10, 20, 30)");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_if_type_is_double_list()
        {
            var i = _Q("$filter=incomeMio in (5634474356.1233)");
            var o = _C("Filter: incomeMio in [5634474356.1233]");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_negation()
        {
            var i = _Q("$filter=not endswith(lastName, 'Duck')");
            var o = _C("Filter: !(endsWith(lastName, 'Duck'))");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_startswith()
        {
            var i = _Q("$filter=startswith(lastName, 'Duck')");
            var o = _C("Filter: startsWith(lastName, 'Duck')");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_endswith()
        {
            var i = _Q("$filter=endswith(lastName, 'Duck')");
            var o = _C("Filter: endsWith(lastName, 'Duck')");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_matchs()
        {
            var i = _Q("$filter=matchs(lastName, 'Duck')");
            var o = _C("Filter: matchs(lastName, 'Duck')");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_empty()
        {
            var i = _Q("$filter=empty(lastName)");
            var o = _C("Filter: empty(lastName)");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_empty_to_true()
        {
            var i = _Q("$filter=empty(lastName) eq true");
            var o = _C("Filter: empty(lastName)");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_exists()
        {
            var i = _Q("$filter=exists(lastName)");
            var o = _C("Filter: exists(lastName)");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_exists_to_true()
        {
            var i = _Q("$filter=exists(lastName) eq true");
            var o = _C("Filter: exists(lastName)");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_exists_to_false()
        {
            var i = _Q("$filter=exists(lastName) eq false");
            var o = _C("Filter: !(exists(lastName))");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_contains()
        {
            var i = _Q("$filter=contains(lastName, 'Duck')");
            var o = _C("Filter: contains(lastName, 'Duck')");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_contains_to_true()
        {
            var i = _Q("$filter=contains(lastName, 'Duck') eq true");
            var o = _C("Filter: contains(lastName, 'Duck')");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_contains_to_false()
        {
            var i = _Q("$filter=contains(lastName, 'Duck') eq false");
            var o = _C("Filter: !(contains(lastName, 'Duck'))");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_equals()
        {
            var i = _Q("$filter=age eq 1");
            var o = _C("Filter: age == 1");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_notequals()
        {
            var i = _Q("$filter=age ne 1");
            var o = _C("Filter: age != 1");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_lessthan()
        {
            var i = _Q("$filter=age lt 1");
            var o = _C("Filter: age < 1");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_lessthanorequal()
        {
            var i = _Q("$filter=age le 1");
            var o = _C("Filter: age <= 1");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_greaterthan()
        {
            var i = _Q("$filter=age gt 1");
            var o = _C("Filter: age > 1");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_greaterthanorequal()
        {
            var i = _Q("$filter=age ge 1");
            var o = _C("Filter: age >= 1");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_conjunction_and_contains()
        {
            var i = _Q("$filter=contains(firstName, 'Sebastian') eq false and isComicFigure eq true");
            var o = _C("Filter: (!(contains(firstName, 'Sebastian')) && isComicFigure == True)");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_conjunction()
        {
            var i = _Q("$filter=age eq 1 and age eq 2");
            var o = _C("Filter: (age == 1 && age == 2)");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_disjunction()
        {
            var i = _Q("$filter=age eq 1 or age eq 2");
            var o = _C("Filter: (age == 1 || age == 2)");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_full_text_numbers()
        {
            var i = _Q("$search=\"33k\"");
            var o = _C("FullText: '33k'");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_full_text()
        {
            var i = _Q("$search=Duck");
            var o = _C("FullText: 'Duck'");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_with_full_text_and_multiple_terms()
        {
            var i = _Q("$search=Dagobert or Donald");
            var o = _C("FullText: 'Dagobert or Donald'");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_orderby_with_single_field()
        {
            var i = _Q("$orderby=age desc");
            var o = _C("Sort: age Descending");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_make_orderby_with_multiple_field()
        {
            var i = _Q("$orderby=age, incomeMio desc");
            var o = _C("Sort: age Ascending, incomeMio Descending");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_and_take()
        {
            var i = _Q("$top=3&$skip=4");
            var o = _C("Skip: 4; Take: 3");

            Assert.Equal(o, i);
        }

        private static string _C(string value)
        {
            return value.Replace('/', '.');
        }

        private static string? _Q(string value)
        {
            var parser = EdmModel.ParseQuery(value);

            return parser?.ToQuery().ToString();
        }
    }
}