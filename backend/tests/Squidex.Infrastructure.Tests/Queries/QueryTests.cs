// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Queries;

public class QueryTests
{
    [Fact]
    public void Should_add_fields_from_sorting()
    {
        var query = new ClrQuery
        {
            Sort = new List<SortNode>
            {
                new SortNode("field1", SortOrder.Ascending),
                new SortNode("field1", SortOrder.Ascending),
                new SortNode("field2", SortOrder.Ascending)
            }
        };

        var fields = query.GetAllFields();

        var expected = new HashSet<string>
        {
            "field1",
            "field2"
        };

        Assert.Equal(expected, fields);
    }

    [Fact]
    public void Should_add_fields_from_filters()
    {
        var query = new ClrQuery
        {
            Filter =
                ClrFilter.And(
                    ClrFilter.Not(
                        ClrFilter.Eq("field1", 1)),
                    ClrFilter.Or(
                        ClrFilter.Eq("field2", 2),
                        ClrFilter.Eq("field2", 4)))
        };

        var fields = query.GetAllFields();

        var expected = new HashSet<string>
        {
            "field1",
            "field2"
        };

        Assert.Equal(expected, fields);
    }
}
