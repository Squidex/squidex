// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Jint.Native.Object;
using Squidex.Domain.Apps.Core.Scripting.Internal;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.Operations.Scripting;

public class JsonMapperTests
{
    [Fact]
    public void Should_map_json_objects_into_a_shared_shape()
    {
        var engine = new Engine(o => o.Strict());

        var mapped = (ObjectInstance)JsonMapper.Map(CreateJson(), engine);
        var nested = (ObjectInstance)mapped.Get("nested");

        // A shared shape is what keeps a script reading a batch of content items monomorphic. It is a
        // performance property and never a correctness one, but it is silent when it regresses: building
        // these objects as a host ObjectInstance subclass again would put them back in the per-object
        // dictionary with no test noticing.
        Assert.True(engine.Advanced.HasSharedShape(mapped));
        Assert.True(engine.Advanced.HasSharedShape(nested));
    }

    [Fact]
    public void Should_share_the_shape_between_objects_of_the_same_shape()
    {
        var engine = new Engine(o => o.Strict());

        var first = (ObjectInstance)JsonMapper.Map(CreateJson(), engine);
        var second = (ObjectInstance)JsonMapper.Map(CreateJson(), engine);

        Assert.True(engine.Advanced.HasSharedShape(first));
        Assert.True(engine.Advanced.HasSharedShape(second));
    }

    private static JsonValue CreateJson()
    {
        return JsonValue.Create(
            new JsonObject()
                .Add("name", JsonValue.Create("squidex"))
                .Add("count", JsonValue.Create(3))
                .Add("nested", JsonValue.Create(new JsonObject().Add("flag", JsonValue.True))));
    }
}
