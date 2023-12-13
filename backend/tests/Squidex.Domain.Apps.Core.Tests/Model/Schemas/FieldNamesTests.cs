// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Core.Model.Schemas;

public class FieldNamesTests
{
    [Theory]
    [InlineData("id")]
    [InlineData("lastModified")]
    [InlineData("lastModifiedBy.avatar")]
    public void Should_return_true_for_valid_meta_field(string fieldName)
    {
        Assert.True(FieldNames.IsMetaField(fieldName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("metaField")]
    public void Should_return_false_for_invalid_meta_field(string? fieldName)
    {
        Assert.False(FieldNames.IsMetaField(fieldName));
    }

    [Theory]
    [InlineData("data.fieldName")]
    public void Should_return_true_for_valid_data_field(string fieldName)
    {
        Assert.True(FieldNames.IsDataField(fieldName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("data")]
    [InlineData("data_field")]
    public void Should_return_false_for_invalid_data_field(string? fieldName)
    {
        Assert.False(FieldNames.IsDataField(fieldName));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("data")]
    [InlineData("data_field")]
    public void Should_return_false_for_invalid_data_field_with_out(string? fieldName)
    {
        Assert.False(FieldNames.IsDataField(fieldName, out _));
    }

    [Fact]
    public void Should_extract_data_field()
    {
        Assert.True(FieldNames.IsDataField("data.dataField", out var dataField));
        Assert.Equal("dataField", dataField);
    }

    [Fact]
    public void Should_not_migrate_empty_names()
    {
        var source = FieldNames.Empty;

        var migrated = source.Migrate();

        Assert.Same(source, migrated);
    }

    [Fact]
    public void Should_not_migrate_new_format_with_meta()
    {
        var source = FieldNames.Create(
            "id",
            "lastModifiedBy.avatar");

        var migrated = source.Migrate();

        Assert.Same(source, migrated);
    }

    [Fact]
    public void Should_not_migrate_new_format_with_data()
    {
        var source = FieldNames.Create(
            "data.field1",
            "data.field2.iv");

        var migrated = source.Migrate();

        Assert.Same(source, migrated);
    }

    [Fact]
    public void Should_not_migrate_new_format_with_mixed_fields()
    {
        var source = FieldNames.Create(
            "id",
            "data.field1",
            "data.field2.iv");

        var migrated = source.Migrate();

        Assert.Same(source, migrated);
    }

    [Fact]
    public void Should_migrate_old_format()
    {
        var source = FieldNames.Create(
            "meta.id",
            "meta.lastModified",
            "meta.lastModifiedBy.avatar",
            "data1",
            "data2.iv");

        var migrated = source.Migrate();

        Assert.Equal(new[]
        {
            "id",
            "lastModified",
            "lastModifiedBy.avatar",
            "data.data1",
            "data.data2.iv"
        }, migrated.ToArray());
    }
}
