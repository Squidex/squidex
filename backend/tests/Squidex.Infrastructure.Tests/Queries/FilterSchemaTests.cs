// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Collections;

namespace Squidex.Infrastructure.Queries;

public class FilterSchemaTests
{
    [Fact]
    public void Should_flatten_schema()
    {
        var schema = new FilterSchema(FilterSchemaType.Object)
        {
            Fields = new[]
            {
                new FilterField(new FilterSchema(FilterSchemaType.Object)
                {
                    Fields = new[]
                    {
                        new FilterField(new FilterSchema(FilterSchemaType.Object)
                        {
                            Fields = new[]
                            {
                                new FilterField(FilterSchema.Number, "nested3")
                            }.ToReadonlyList()
                        }, "nested2")
                    }.ToReadonlyList()
                }, "nested1")
            }.ToReadonlyList()
        };

        var expected = new FilterSchema(FilterSchemaType.Object)
        {
            Fields = new[]
            {
                new FilterField(new FilterSchema(FilterSchemaType.Object), "nested1"),
                new FilterField(new FilterSchema(FilterSchemaType.Object), "nested1.nested2"),
                new FilterField(new FilterSchema(FilterSchemaType.Number), "nested1.nested2.nested3")
            }.ToReadonlyList()
        };

        var actual = schema.Flatten();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_ignore_conflicts_when_flatten()
    {
        var schema = new FilterSchema(FilterSchemaType.Object)
        {
            Fields = new[]
            {
                new FilterField(FilterSchema.Number, "property1"),
                new FilterField(FilterSchema.String, "property1"),
                new FilterField(FilterSchema.String, "property2"),
                new FilterField(FilterSchema.String, "property2")
            }.ToReadonlyList()
        };

        var expected = new FilterSchema(FilterSchemaType.Object)
        {
            Fields = new[]
            {
                new FilterField(FilterSchema.String, "property2")
            }.ToReadonlyList()
        };

        var actual = schema.Flatten();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_remove_Descriptions_for_merged_fields()
    {
        var schema = new FilterSchema(FilterSchemaType.Object)
        {
            Fields = new[]
            {
                new FilterField(FilterSchema.String, "property1", "Description1"),
                new FilterField(FilterSchema.String, "property2", "Description2"),
                new FilterField(FilterSchema.String, "property2", "Description3")
            }.ToReadonlyList()
        };

        var expected = new FilterSchema(FilterSchemaType.Object)
        {
            Fields = new[]
            {
                new FilterField(FilterSchema.String, "property1", "Description1"),
                new FilterField(FilterSchema.String, "property2")
            }.ToReadonlyList()
        };

        var actual = schema.Flatten();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_filter_out_fields_by_predicate_when_flatten()
    {
        var schema = new FilterSchema(FilterSchemaType.Object)
        {
            Fields = new[]
            {
                new FilterField(new FilterSchema(FilterSchemaType.Object), "property1"),
                new FilterField(FilterSchema.String, "property2")
            }.ToReadonlyList()
        };

        var expected = new FilterSchema(FilterSchemaType.Object)
        {
            Fields = new[]
            {
                new FilterField(FilterSchema.String, "property2")
            }.ToReadonlyList()
        };

        var actual = schema.Flatten(predicate: x => x.Type != FilterSchemaType.Object);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_filter_out_fields_without_operators_when_flattened_by_model()
    {
        var schema = new FilterSchema(FilterSchemaType.Object)
        {
            Fields = new[]
            {
                new FilterField(new FilterSchema(FilterSchemaType.Object), "property1"),
                new FilterField(FilterSchema.String, "property2")
            }.ToReadonlyList()
        };

        var expected = new FilterSchema(FilterSchemaType.Object)
        {
            Fields = new[]
            {
                new FilterField(FilterSchema.String, "property2")
            }.ToReadonlyList()
        };

        var actual = new QueryModel { Schema = schema }.Flatten().Schema;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_not_filter_out_fields_without_operators_when_flattened_by_model_but_flag_is_false()
    {
        var schema = new FilterSchema(FilterSchemaType.Object)
        {
            Fields = new[]
            {
                new FilterField(new FilterSchema(FilterSchemaType.Object), "property1"),
                new FilterField(FilterSchema.String, "property2")
            }.ToReadonlyList()
        };

        var expected = new FilterSchema(FilterSchemaType.Object)
        {
            Fields = new[]
            {
                new FilterField(new FilterSchema(FilterSchemaType.Object), "property1"),
                new FilterField(FilterSchema.String, "property2")
            }.ToReadonlyList()
        };

        var actual = new QueryModel { Schema = schema }.Flatten(onlyWithOperators: false).Schema;

        Assert.Equal(expected, actual);
    }
}
