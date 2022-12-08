// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Core.Model.Contents;

public class ContentDataTests
{
    [Fact]
    public void Should_return_same_content_if_merging_same_references()
    {
        var source =
            new ContentData()
                .AddField("field1",
                    new ContentFieldData()
                        .AddInvariant(1))
                .AddField("field2",
                    new ContentFieldData()
                        .AddLocalized("de", 2));

        var actual = source.MergeInto(source);

        Assert.Same(source, actual);
    }

    [Fact]
    public void Should_merge_two_name_models()
    {
        var lhs =
            new ContentData()
                .AddField("field1",
                    new ContentFieldData()
                        .AddInvariant(1))
                .AddField("field2",
                    new ContentFieldData()
                        .AddLocalized("de", 2)
                        .AddLocalized("it", 2));

        var rhs =
            new ContentData()
                .AddField("field2",
                    new ContentFieldData()
                        .AddLocalized("it", 3)
                        .AddLocalized("en", 3))
                .AddField("field3",
                    new ContentFieldData()
                        .AddInvariant(4));

        var expected =
            new ContentData()
                .AddField("field1",
                    new ContentFieldData()
                        .AddInvariant(1))
                .AddField("field2",
                    new ContentFieldData()
                        .AddLocalized("it", 2)
                        .AddLocalized("de", 2)
                        .AddLocalized("en", 3))
                .AddField("field3",
                    new ContentFieldData()
                        .AddInvariant(4));

        var actual = lhs.MergeInto(rhs);

        Assert.Equal(expected, actual);
        Assert.NotSame(expected, rhs);
        Assert.NotSame(expected, lhs);
    }

    [Fact]
    public void Should_be_equal_if_data_have_same_structure()
    {
        var lhs =
            new ContentData()
                .AddField("field1",
                    new ContentFieldData()
                        .AddInvariant(2))
                .AddField("field2",
                    new ContentFieldData()
                        .AddInvariant(2));

        var rhs =
            new ContentData()
                .AddField("field1",
                    new ContentFieldData()
                        .AddInvariant(2))
                .AddField("field2",
                    new ContentFieldData()
                        .AddInvariant(2));

        Assert.True(lhs.Equals(rhs));
        Assert.True(lhs.Equals((object)rhs));
        Assert.Equal(lhs.GetHashCode(), rhs.GetHashCode());
    }

    [Fact]
    public void Should_not_be_equal_if_data_have_not_same_structure()
    {
        var lhs =
            new ContentData()
                .AddField("field1",
                    new ContentFieldData()
                        .AddInvariant(2))
                .AddField("field2",
                    new ContentFieldData()
                        .AddInvariant(2));

        var rhs =
            new ContentData()
                .AddField("field1",
                    new ContentFieldData()
                        .AddLocalized("en", 2))
                .AddField("field3",
                    new ContentFieldData()
                        .AddInvariant(2));

        Assert.False(lhs.Equals(rhs));
        Assert.False(lhs.Equals((object)rhs));
        Assert.NotEqual(lhs.GetHashCode(), rhs.GetHashCode());
    }

    [Fact]
    public void Should_be_equal_fields_if_they_have_same_value()
    {
        var lhs =
            new ContentFieldData()
                .AddInvariant(2);

        var rhs =
            new ContentFieldData()
                .AddInvariant(2);

        Assert.True(lhs.Equals(rhs));
        Assert.True(lhs.Equals((object)rhs));
        Assert.Equal(lhs.GetHashCode(), rhs.GetHashCode());
    }

    [Fact]
    public void Should_clone_named_value_and_also_children()
    {
        var source = new ContentData
        {
            ["field1"] = new ContentFieldData(),
            ["field2"] = new ContentFieldData()
        };

        var clone = source.Clone();

        Assert.NotSame(source, clone);

        foreach (var (key, value) in clone)
        {
            Assert.NotSame(value, source[key]);
        }
    }

    [Fact]
    public void Should_copy_fields_from_other_data_if_they_are_equal()
    {
        var oldData =
            new ContentData()
                .AddField("field1",
                    new ContentFieldData()
                        .AddInvariant(1))
                .AddField("field2",
                    new ContentFieldData()
                        .AddInvariant(2));

        var newData =
            new ContentData()
                .AddField("field1",
                    new ContentFieldData()
                        .AddInvariant(1))
                .AddField("field2",
                    new ContentFieldData()
                        .AddInvariant(3));

        newData.UseSameFields(oldData);

        Assert.Same(newData["field1"], oldData["field1"]);
        Assert.NotSame(newData["field2"], oldData["field2"]);
    }

    [Fact]
    public void Should_copy_field_values_from_other_data_if_they_are_equal()
    {
        var oldData =
            new ContentData()
                .AddField("field1",
                    new ContentFieldData()
                        .AddLocalized("en", 1)
                        .AddLocalized("de", 2));
        var newData =
            new ContentData()
                .AddField("field1",
                    new ContentFieldData()
                        .AddLocalized("en", 1)
                        .AddLocalized("de", 3));

        newData.UseSameFields(oldData);

        Assert.Same(newData["field1"]!["en"].Value, oldData["field1"]!["en"].Value);
        Assert.NotSame(newData["field1"]!["de"].Value, oldData["field1"]!["de"].Value);
        Assert.NotSame(newData["field1"], oldData["field1"]);
    }
}
