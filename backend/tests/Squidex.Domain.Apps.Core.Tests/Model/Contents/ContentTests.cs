// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.TestHelpers;

namespace Squidex.Domain.Apps.Core.Model.Contents;

public class ContentTests
{
    [Fact]
    public void Should_deserialize_state()
    {
        var json = File.ReadAllText("Model/Contents/Content.json");

        var deserialized = TestUtils.DefaultSerializer.Deserialize<Content>(json);

        Assert.NotNull(deserialized);
    }

    [Fact]
    public void Should_serialize_deserialize_state()
    {
        var json = File.ReadAllText("Model/Contents/Content.json").CleanJson();

        var serialized = TestUtils.SerializeWithoutNulls(TestUtils.DefaultSerializer.Deserialize<Content>(json));

        Assert.Equal(json, serialized);
    }
}
