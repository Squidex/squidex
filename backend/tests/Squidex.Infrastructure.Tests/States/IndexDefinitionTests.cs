// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Queries;

namespace Squidex.Infrastructure.States;

public class IndexDefinitionTests
{
    [Fact]
    public void Should_create_name_for_empty_definition()
    {
        var definition = new IndexDefinition();

        Assert.Equal(string.Empty, definition.ToName());
    }

    [Fact]
    public void Should_create_name_for_asc_order()
    {
        var definition = new IndexDefinition
        {
            new IndexField("field1", SortOrder.Ascending)
        };

        Assert.Equal("field1_asc", definition.ToName());
    }

    [Fact]
    public void Should_create_name_for_dasc_order()
    {
        var definition = new IndexDefinition
        {
            new IndexField("field1", SortOrder.Descending)
        };

        Assert.Equal("field1_desc", definition.ToName());
    }

    [Fact]
    public void Should_create_name_for_multiple_fields()
    {
        var definition = new IndexDefinition
        {
            new IndexField("field1", SortOrder.Ascending),
            new IndexField("field2", SortOrder.Descending)
        };

        Assert.Equal("field1_asc_field2_desc", definition.ToName());
    }
}
