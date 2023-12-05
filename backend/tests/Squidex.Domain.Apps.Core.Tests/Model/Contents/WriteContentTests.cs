// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Model.Contents;

public class WriteContentTests
{
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");

    [Fact]
    public void Should_convert_draft_data_to_content()
    {
        var source = new WriteContent
        {
            AppId = appId,
            CurrentVersion = new ContentVersion(
                Status.Published,
                new ContentData()
                    .AddField("location",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Create("Berlin")))),
            NewVersion = new ContentVersion(
                Status.Draft,
                new ContentData()
                    .AddField("location",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Create("London")))),
        };

        var expected = new Content
        {
            AppId = appId,
            Status = Status.Published,
            Data =
                new ContentData()
                    .AddField("location",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Create("London"))),
            NewStatus = Status.Draft
        };

        var actual = source.ToContent();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_convert_normal_data_to_content()
    {
        var source = new WriteContent
        {
            AppId = appId,
            CurrentVersion = new ContentVersion(
                Status.Draft,
                new ContentData()
                    .AddField("location",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Create("Berlin")))),
        };

        var expected = new Content
        {
            AppId = appId,
            Status = Status.Draft,
            Data =
                new ContentData()
                    .AddField("location",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Create("Berlin"))),
            NewStatus = null
        };

        var actual = source.ToContent();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Should_deserialize_state()
    {
        var json = File.ReadAllText("Model/Contents/WriteContent.json");

        var deserialized = TestUtils.DefaultSerializer.Deserialize<WriteContent>(json);

        Assert.NotNull(deserialized);
    }

    [Fact]
    public void Should_serialize_deserialize_state()
    {
        var json = File.ReadAllText("Model/Contents/WriteContent.json").CleanJson();

        var serialized = TestUtils.SerializeWithoutNulls(TestUtils.DefaultSerializer.Deserialize<WriteContent>(json));

        Assert.Equal(json, serialized);
    }
}
