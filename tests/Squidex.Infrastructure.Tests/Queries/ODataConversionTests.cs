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
    public class ODataConversionTests
    {
        private static readonly IEdmModel EdmModel;

        static ODataConversionTests()
        {
            var entityType = new EdmEntityType("Squidex", "Users");

            entityType.AddStructuralProperty("id", EdmPrimitiveTypeKind.Guid);
            entityType.AddStructuralProperty("created", EdmPrimitiveTypeKind.DateTimeOffset);
            entityType.AddStructuralProperty("isComicFigure", EdmPrimitiveTypeKind.Boolean);
            entityType.AddStructuralProperty("firstName", EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty("lastName", EdmPrimitiveTypeKind.String);
            entityType.AddStructuralProperty("birthday", EdmPrimitiveTypeKind.Date);
            entityType.AddStructuralProperty("incomeCents", EdmPrimitiveTypeKind.Int64);
            entityType.AddStructuralProperty("incomeMio", EdmPrimitiveTypeKind.Double);
            entityType.AddStructuralProperty("age", EdmPrimitiveTypeKind.Int32);

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

        [Fact]
        public void Should_parse_filter_when_type_is_datetime()
        {
            var i = Q("$filter=created eq 1988-01-19T12:00:00Z");
            var o = C("Filter: created == 1988-01-19T12:00:00Z");

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
        public void Should_parse_filter_when_type_is_date()
        {
            var i = Q("$filter=created eq 1988-01-19");
            var o = C("Filter: created == 1988-01-19T00:00:00Z");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_when_type_is_date_list()
        {
            var i = Q("$filter=created in ('1988-01-19')");
            var o = C("Filter: created in [1988-01-19T00:00:00Z]");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_when_type_is_guid()
        {
            var i = Q("$filter=id eq B5FE25E3-B262-4B17-91EF-B3772A6B62BB");
            var o = C("Filter: id == b5fe25e3-b262-4b17-91ef-b3772a6b62bb");

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

        [Fact]
        public void Should_parse_filter_when_type_is_string()
        {
            var i = Q("$filter=firstName eq 'Dagobert'");
            var o = C("Filter: firstName == 'Dagobert'");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_when_type_is_string_list()
        {
            var i = Q("$filter=firstName in ('Dagobert')");
            var o = C("Filter: firstName in ['Dagobert']");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_when_type_is_boolean()
        {
            var i = Q("$filter=isComicFigure eq true");
            var o = C("Filter: isComicFigure == True");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_when_type_is_boolean_list()
        {
            var i = Q("$filter=isComicFigure in (true)");
            var o = C("Filter: isComicFigure in [True]");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_when_type_is_int32()
        {
            var i = Q("$filter=age eq 60");
            var o = C("Filter: age == 60");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_when_type_is_int32_list()
        {
            var i = Q("$filter=age in (60)");
            var o = C("Filter: age in [60]");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_when_type_is_int64()
        {
            var i = Q("$filter=incomeCents eq 31543143513456789");
            var o = C("Filter: incomeCents == 31543143513456789");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_when_type_is_int64_list()
        {
            var i = Q("$filter=incomeCents in (31543143513456789)");
            var o = C("Filter: incomeCents in [31543143513456789]");

            Assert.Equal(o, i);
        }

        [Fact]
        public void Should_parse_filter_when_type_is_double()
        {
            var i = Q("$filter=incomeMio eq 5634474356.1233");
            var o = C("Filter: incomeMio == 5634474356.1233");

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

        private static string Q(string value)
        {
            var parser = EdmModel.ParseQuery(value);

            return parser.ToQuery().ToString();
        }
    }
}