// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.TestHelpers;

namespace Squidex.Domain.Apps.Core.Model.Contents;

public class StatusTests
{
    private readonly TypeConverter typeConverter = TypeDescriptor.GetConverter(typeof(Status));

    [Fact]
    public void Should_convert_from_string()
    {
        var actual = typeConverter.ConvertFromString("Draft");

        Assert.Equal(Status.Draft, actual);
    }

    [Fact]
    public void Should_convert_to_string()
    {
        var actual = typeConverter.ConvertToString(Status.Draft);

        Assert.Equal("Draft", actual);
    }

    [Fact]
    public void Should_initialize_default()
    {
        Status status = default;

        Assert.Equal("Unknown", status.Name);
        Assert.Equal("Unknown", status.ToString());
    }

    [Fact]
    public void Should_initialize_status_from_string()
    {
        var status = new Status("Custom");

        Assert.Equal("Custom", status.Name);
        Assert.Equal("Custom", status.ToString());
    }

    [Fact]
    public void Should_provide_draft_status()
    {
        var status = Status.Draft;

        Assert.Equal("Draft", status.Name);
        Assert.Equal("Draft", status.ToString());
    }

    [Fact]
    public void Should_provide_archived_status()
    {
        var status = Status.Archived;

        Assert.Equal("Archived", status.Name);
        Assert.Equal("Archived", status.ToString());
    }

    [Fact]
    public void Should_provide_published_status()
    {
        var status = Status.Published;

        Assert.Equal("Published", status.Name);
        Assert.Equal("Published", status.ToString());
    }

    [Fact]
    public void Should_make_correct_equal_comparisons()
    {
        var status_1_a = Status.Draft;
        var status_1_b = Status.Draft;

        var status2_a = Status.Published;

        Assert.Equal(status_1_a, status_1_b);
        Assert.Equal(status_1_a.GetHashCode(), status_1_b.GetHashCode());
        Assert.True(status_1_a.Equals((object)status_1_b));

        Assert.NotEqual(status_1_a, status2_a);
        Assert.NotEqual(status_1_a.GetHashCode(), status2_a.GetHashCode());
        Assert.False(status_1_a.Equals((object)status2_a));

        Assert.True(status_1_a == status_1_b);
        Assert.True(status_1_a != status2_a);

        Assert.False(status_1_a != status_1_b);
        Assert.False(status_1_a == status2_a);
    }

    [Fact]
    public void Should_serialize_and_deserialize()
    {
        var status = Status.Draft;

        var serialized = status.SerializeAndDeserialize();

        Assert.Equal(status, serialized);
    }

    [Fact]
    public void Should_serialize_and_deserialize_as_dictionary_key()
    {
        var dictionary = new Dictionary<Status, int>
        {
            [Status.Draft] = 123
        };

        var serialized = dictionary.SerializeAndDeserialize();

        Assert.Equal(123, serialized[Status.Draft]);
    }
}
