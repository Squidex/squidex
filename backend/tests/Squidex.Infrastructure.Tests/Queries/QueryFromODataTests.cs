// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.OData.Edm;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Queries.OData;

#pragma warning disable SA1300 // Element should begin with upper-case letter
#pragma warning disable IDE1006 // Naming Styles

namespace Squidex.Infrastructure.Queries;

public class QueryFromODataTests
{
    private static readonly IEdmModel EdmModel;

    static QueryFromODataTests()
    {
        var fields = new List<FilterField>
        {
            new FilterField(FilterSchema.Guid, "id"),
            new FilterField(FilterSchema.Guid, "idNullable", IsNullable: true),
            new FilterField(FilterSchema.DateTime, "created"),
            new FilterField(FilterSchema.DateTime, "createdNullable", IsNullable: true),
            new FilterField(FilterSchema.Boolean, "isComicFigure"),
            new FilterField(FilterSchema.Boolean, "isComicFigureNullable", IsNullable: true),
            new FilterField(FilterSchema.String, "firstName"),
            new FilterField(FilterSchema.String, "firstNameNullable", IsNullable: true),
            new FilterField(FilterSchema.String, "lastName"),
            new FilterField(FilterSchema.String, "lastNameNullable", IsNullable: true),
            new FilterField(FilterSchema.Number, "age"),
            new FilterField(FilterSchema.Number, "ageNullable", IsNullable: true),
            new FilterField(FilterSchema.Number, "incomeMio"),
            new FilterField(FilterSchema.Number, "incomeMioNullable", IsNullable: true),
            new FilterField(FilterSchema.GeoObject, "geo"),
            new FilterField(FilterSchema.GeoObject, "geoNullable", IsNullable: true),
            new FilterField(FilterSchema.Any, "properties"),
        };

        var queryModel = new QueryModel
        {
            Schema = new FilterSchema(FilterSchemaType.Object)
            {
                Fields = fields.ToReadonlyList(),
            },
        };

        EdmModel = queryModel.ConvertToEdm("Squidex", "Content");
    }

    [Fact]
    public void Should_parse_query()
    {
        var parser = EdmModel.ParseQuery("$filter=firstName eq 'Dagobert'");

        Assert.NotNull(parser);
    }

    [Fact]
    public void Should_escape_field_name()
    {
        Assert.Equal("field_name", "field-name".EscapeEdmField());
    }

    [Fact]
    public void Should_unescape_field_name()
    {
        Assert.Equal("field-name", "field_name".UnescapeEdmField());
    }

    [Theory]
    [InlineData("created")]
    [InlineData("createdNullable")]
    [InlineData("properties/datetime")]
    [InlineData("properties/nested/datetime")]
    public void Should_parse_filter_if_type_is_datetime(string field)
    {
        var i = _o($"$filter={field} eq 1988-01-19T12:00:00Z");
        var o = _q($"Filter: {field} == 1988-01-19T12:00:00Z");

        Assert.Equal(o, i);
    }

    [Theory]
    [InlineData("created")]
    [InlineData("createdNullable")]
    [InlineData("properties/datetime")]
    [InlineData("properties/nested/datetime")]
    public void Should_parse_filter_if_type_is_datetime_and_value_is_null(string field)
    {
        var i = _o($"$filter={field} eq null");
        var o = _q($"Filter: {field} == null");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_if_type_is_datetime_list()
    {
        var i = _o("$filter=created in ('1988-01-19T12:00:00Z')");
        var o = _q("Filter: created in [1988-01-19T12:00:00Z]");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_if_type_is_datetime_and_and_value_is_date()
    {
        var i = _o("$filter=created eq 1988-01-19");
        var o = _q("Filter: created == 1988-01-19T00:00:00Z");

        Assert.Equal(o, i);
    }

    [Theory]
    [InlineData("id")]
    [InlineData("idNullable")]
    [InlineData("properties/uid")]
    [InlineData("properties/nested/guid")]
    public void Should_parse_filter_if_type_is_guid(string field)
    {
        var i = _o($"$filter={field} eq B5FE25E3-B262-4B17-91EF-B3772A6B62BB");
        var o = _q($"Filter: {field} == b5fe25e3-b262-4b17-91ef-b3772a6b62bb");

        Assert.Equal(o, i);
    }

    [Theory]
    [InlineData("id")]
    [InlineData("idNullable")]
    [InlineData("properties/uid")]
    [InlineData("properties/nested/guid")]
    public void Should_parse_filter_if_type_is_guid_and_value_is_null(string field)
    {
        var i = _o($"$filter={field} eq null");
        var o = _q($"Filter: {field} == null");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_if_type_is_guid_list()
    {
        var i = _o("$filter=id in ('B5FE25E3-B262-4B17-91EF-B3772A6B62BB')");
        var o = _q("Filter: id in [b5fe25e3-b262-4b17-91ef-b3772a6b62bb]");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_if_type_is_null()
    {
        var i = _o("$filter=firstName eq null");
        var o = _q("Filter: firstName == null");

        Assert.Equal(o, i);
    }

    [Theory]
    [InlineData("firstName")]
    [InlineData("firstNameNullable")]
    [InlineData("properties/string")]
    [InlineData("properties/nested/string")]
    public void Should_parse_filter_if_type_is_string(string field)
    {
        var i = _o($"$filter={field} eq 'Dagobert'");
        var o = _q($"Filter: {field} == 'Dagobert'");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_if_type_is_string_list()
    {
        var i = _o("$filter=firstName in ('Dagobert')");
        var o = _q("Filter: firstName in ['Dagobert']");

        Assert.Equal(o, i);
    }

    [Theory]
    [InlineData("isComicFigure")]
    [InlineData("isComicFigureNullable")]
    [InlineData("properties/boolean")]
    [InlineData("properties/nested/boolean")]
    public void Should_parse_filter_if_type_is_boolean(string field)
    {
        var i = _o($"$filter={field} eq true");
        var o = _q($"Filter: {field} == True");

        Assert.Equal(o, i);
    }

    [Theory]
    [InlineData("isComicFigure")]
    [InlineData("isComicFigureNullable")]
    [InlineData("properties/boolean")]
    [InlineData("properties/nested/boolean")]
    public void Should_parse_filter_if_type_is_boolean_and_value_is_null(string field)
    {
        var i = _o($"$filter={field} eq null");
        var o = _q($"Filter: {field} == null");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_if_type_is_boolean_list()
    {
        var i = _o("$filter=isComicFigure in (true)");
        var o = _q("Filter: isComicFigure in [True]");

        Assert.Equal(o, i);
    }

    [Theory]
    [InlineData("incomeMio")]
    [InlineData("incomeMioNullable")]
    [InlineData("properties/double")]
    [InlineData("properties/nested/double")]
    public void Should_parse_filter_if_type_is_double(string field)
    {
        var i = _o($"$filter={field} eq 5634474356.1233");
        var o = _q($"Filter: {field} == 5634474356.1233");

        Assert.Equal(o, i);
    }

    [Theory]
    [InlineData("geo")]
    [InlineData("geoNullable")]
    [InlineData("properties/geo")]
    [InlineData("properties/nested/geo")]
    public void Should_parse_filter_if_type_is_geograph(string field)
    {
        var i = _o($"$filter=geo.distance({field}, geography'POINT(10 20)') lt 30.0");
        var o = _q($"Filter: {field} < Radius(10, 20, 30)");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_if_type_is_double_list()
    {
        var i = _o("$filter=incomeMio in (5634474356.1233)");
        var o = _q("Filter: incomeMio in [5634474356.1233]");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_with_negation()
    {
        var i = _o("$filter=not endswith(lastName, 'Duck')");
        var o = _q("Filter: !(endsWith(lastName, 'Duck'))");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_with_startswith()
    {
        var i = _o("$filter=startswith(lastName, 'Duck')");
        var o = _q("Filter: startsWith(lastName, 'Duck')");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_with_endswith()
    {
        var i = _o("$filter=endswith(lastName, 'Duck')");
        var o = _q("Filter: endsWith(lastName, 'Duck')");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_with_matchs()
    {
        var i = _o("$filter=matchs(lastName, 'Duck')");
        var o = _q("Filter: matchs(lastName, 'Duck')");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_with_empty()
    {
        var i = _o("$filter=empty(lastName)");
        var o = _q("Filter: empty(lastName)");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_with_empty_to_true()
    {
        var i = _o("$filter=empty(lastName) eq true");
        var o = _q("Filter: empty(lastName)");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_with_exists()
    {
        var i = _o("$filter=exists(lastName)");
        var o = _q("Filter: exists(lastName)");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_with_exists_to_true()
    {
        var i = _o("$filter=exists(lastName) eq true");
        var o = _q("Filter: exists(lastName)");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_with_exists_to_false()
    {
        var i = _o("$filter=exists(lastName) eq false");
        var o = _q("Filter: !(exists(lastName))");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_with_contains()
    {
        var i = _o("$filter=contains(lastName, 'Duck')");
        var o = _q("Filter: contains(lastName, 'Duck')");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_with_contains_to_true()
    {
        var i = _o("$filter=contains(lastName, 'Duck') eq true");
        var o = _q("Filter: contains(lastName, 'Duck')");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_with_contains_to_false()
    {
        var i = _o("$filter=contains(lastName, 'Duck') eq false");
        var o = _q("Filter: !(contains(lastName, 'Duck'))");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_with_equals()
    {
        var i = _o("$filter=age eq 1");
        var o = _q("Filter: age == 1");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_with_notequals()
    {
        var i = _o("$filter=age ne 1");
        var o = _q("Filter: age != 1");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_with_lessthan()
    {
        var i = _o("$filter=age lt 1");
        var o = _q("Filter: age < 1");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_with_lessthanorequal()
    {
        var i = _o("$filter=age le 1");
        var o = _q("Filter: age <= 1");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_with_greaterthan()
    {
        var i = _o("$filter=age gt 1");
        var o = _q("Filter: age > 1");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_with_greaterthanorequal()
    {
        var i = _o("$filter=age ge 1");
        var o = _q("Filter: age >= 1");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_with_conjunction_and_contains()
    {
        var i = _o("$filter=contains(firstName, 'Sebastian') eq false and isComicFigure eq true");
        var o = _q("Filter: (!(contains(firstName, 'Sebastian')) && isComicFigure == True)");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_with_conjunction()
    {
        var i = _o("$filter=age eq 1 and age eq 2");
        var o = _q("Filter: (age == 1 && age == 2)");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_filter_with_disjunction()
    {
        var i = _o("$filter=age eq 1 or age eq 2");
        var o = _q("Filter: (age == 1 || age == 2)");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_compare_constant_to_lowercase()
    {
        var i = _o("$filter=firstName eq tolower('DONALD')");
        var o = _q("Filter: firstName == 'donald'");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_compare_constant_to_uppercase()
    {
        var i = _o("$filter=firstName eq toupper('donald')");
        var o = _q("Filter: firstName == 'DONALD'");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_convert_lowercase_eq_comparison_to_regex()
    {
        var i = _o("$filter=tolower(firstName) eq tolower('DONALD.*42')");
        var o = _q("Filter: matchs(firstName, '//^donald\\.\\*42$//i')");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_convert_lowercase_ne_comparison_to_regex()
    {
        var i = _o("$filter=tolower(firstName) ne tolower('DONALD.*42')");
        var o = _q("Filter: !(matchs(firstName, '//^donald\\.\\*42$//i'))");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_convert_uppercase_eq_comparison_to_regex()
    {
        var i = _o("$filter=toupper(firstName) eq toupper('donald.*42')");
        var o = _q("Filter: matchs(firstName, '//^DONALD\\.\\*42$//i')");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_convert_uppercase_ne_comparison_to_regex()
    {
        var i = _o("$filter=toupper(firstName) ne toupper('donald.*42')");
        var o = _q("Filter: !(matchs(firstName, '//^DONALD\\.\\*42$//i'))");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_not_convert_lowercase_to_regex_if_right_side_is_mixed()
    {
        Assert.Throws<NotSupportedException>(() => _o("$filter=tolower(firstName) ne 'Donald.*42'"));
    }

    [Fact]
    public void Should_not_convert_uppercase_to_regex_if_right_side_is_mixed()
    {
        Assert.Throws<NotSupportedException>(() => _o("$filter=toupper(firstName) ne 'Donald.*42'"));
    }

    [Fact]
    public void Should_full_text()
    {
        var i = _o("$search=Duck");
        var o = _q("FullText: 'Duck'");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_collation1()
    {
        var i = _o("collation=Collation");
        var o = _q("Collation: 'Collation'");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_collation2()
    {
        var i = _o("$collation=Collation");
        var o = _q("Collation: 'Collation'");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_text_and_multiple_terms()
    {
        var i = _o("$search=Dagobert or Donald");
        var o = _q("FullText: 'Dagobert or Donald'");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_full_text_numbers()
    {
        var i = _o("$search=\"33k\"");
        var o = _q("FullText: '33k'");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_orderby()
    {
        var i = _o("$orderby=age desc");
        var o = _q("Sort: age Descending");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_orderby_with_multiple_field()
    {
        var i = _o("$orderby=age, incomeMio desc");
        var o = _q("Sort: age Ascending, incomeMio Descending");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_top()
    {
        var i = _o("$top=3");
        var o = _q("Take: 3");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_skip()
    {
        var i = _o("$skip=4");
        var o = _q("Skip: 4");

        Assert.Equal(o, i);
    }

    [Fact]
    public void Should_parse_random()
    {
        var i = _o("$random=4");
        var o = _q("Random: 4");

        Assert.Equal(o, i);
    }

    private static string _q(string value)
    {
        return value.Replace('/', '.').Replace("..", "/", StringComparison.OrdinalIgnoreCase);
    }

    private static string? _o(string value)
    {
        var parser = EdmModel.ParseQuery(value);

        return parser?.ToQuery().ToString();
    }
}
