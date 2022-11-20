// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using MongoDB.Bson.Serialization.Attributes;

namespace Squidex.Infrastructure.MongoDb;

public class MongoFieldTests
{
    public class Entity
    {
        public string Id { get; set; }

        public string Default { get; set; }

        [BsonElement("_c")]
        public string Custom { get; set; }
    }

    public sealed class Inherited : Entity
    {
    }

    [Fact]
    public void Should_resolve_id_field()
    {
        var name = Field.Of<Entity>(x => nameof(x.Id));

        Assert.Equal("_id", name);
    }

    [Fact]
    public void Should_resolve_id_field_from_base_class()
    {
        var name = Field.Of<Inherited>(x => nameof(x.Id));

        Assert.Equal("_id", name);
    }

    [Fact]
    public void Should_resolve_default_field()
    {
        var name = Field.Of<Entity>(x => nameof(x.Default));

        Assert.Equal("Default", name);
    }

    [Fact]
    public void Should_resolve_default_field_from_base_class()
    {
        var name = Field.Of<Inherited>(x => nameof(x.Default));

        Assert.Equal("Default", name);
    }

    [Fact]
    public void Should_resolve_custom_field()
    {
        var name = Field.Of<Entity>(x => nameof(x.Custom));

        Assert.Equal("_c", name);
    }

    [Fact]
    public void Should_resolve_custom_field_from_base_class()
    {
        var name = Field.Of<Inherited>(x => nameof(x.Custom));

        Assert.Equal("_c", name);
    }

    [Fact]
    public void Should_throw_exception_if_field_not_found()
    {
        Assert.Throws<InvalidOperationException>(() => Field.Of<Entity>(x => "invalid"));
    }
}
