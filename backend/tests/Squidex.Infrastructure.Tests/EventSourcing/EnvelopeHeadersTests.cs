// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.TestHelpers;

namespace Squidex.Infrastructure.EventSourcing;

public class EnvelopeHeadersTests
{
    [Fact]
    public void Should_create_headers()
    {
        var headers = new EnvelopeHeaders();

        Assert.Empty(headers);
    }

    [Fact]
    public void Should_create_headers_as_copy()
    {
        var source = new EnvelopeHeaders
        {
            ["key1"] = JsonValue.Create(13)
        };

        var headers = new EnvelopeHeaders(source);

        CompareHeaders(headers, source);
    }

    [Fact]
    public void Should_clone_headers()
    {
        var source = new EnvelopeHeaders
        {
            ["key1"] = JsonValue.Create(13)
        };

        var headers = source.CloneHeaders();

        CompareHeaders(headers, source);
    }

    [Fact]
    public void Should_serialize_and_deserialize()
    {
        var source = new EnvelopeHeaders
        {
            ["key1"] = JsonValue.Create(13)
        };

        var deserialized = source.SerializeAndDeserialize();

        CompareHeaders(deserialized, source);
    }

    private static void CompareHeaders(EnvelopeHeaders lhs, EnvelopeHeaders rhs)
    {
        foreach (var key in lhs.Keys.Concat(rhs.Keys).Distinct())
        {
            Assert.Equal(lhs[key].ToString(), rhs[key].ToString());
        }
    }
}
