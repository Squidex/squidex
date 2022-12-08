// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Model.Schemas;

public class FieldCompareTests
{
    [Fact]
    public void Should_compare_two_string_fields_as_equal()
    {
        var lhs = new StringFieldProperties
        {
            DefaultValues = new LocalizedValue<string?>(new Dictionary<string, string?>
            {
                ["iv"] = "ABC"
            })
        };

        var rhs = new StringFieldProperties
        {
            DefaultValues = new LocalizedValue<string?>(new Dictionary<string, string?>
            {
                ["iv"] = "ABC"
            })
        };

        Assert.Equal(lhs, rhs);
    }

    [Fact]
    public void Should_compare_two_tags_fields_as_equal()
    {
        var lhs = new TagsFieldProperties
        {
            DefaultValues = new LocalizedValue<ReadonlyList<string>?>(new Dictionary<string, ReadonlyList<string>?>
            {
                ["iv"] = ReadonlyList.Create("A", "B", "C")
            })
        };

        var rhs = new TagsFieldProperties
        {
            DefaultValues = new LocalizedValue<ReadonlyList<string>?>(new Dictionary<string, ReadonlyList<string>?>
            {
                ["iv"] = ReadonlyList.Create("A", "B", "C")
            })
        };

        Assert.Equal(lhs, rhs);
    }
}
