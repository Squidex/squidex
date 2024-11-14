// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.MongoDb.Contents;
using Squidex.Domain.Apps.Entities.MongoDb.Contents.Operations;

namespace Squidex.Domain.Apps.Entities.Contents.MongoDb;

public class AdaptionTests
{
    static AdaptionTests()
    {
        MongoContentEntity.RegisterClassMap();
    }

    [Fact]
    public void Should_adapt_to_meta_field()
    {
        var source = "lastModified";

        var result = Adapt.MapPath(source).ToString();

        Assert.Equal("mt", result);
    }

    [Fact]
    public void Should_adapt_to_data_field()
    {
        var source = "data.test";

        var result = Adapt.MapPath(source).ToString();

        Assert.Equal("do.test", result);
    }

    [Fact]
    public void Should_adapt_from_meta_field()
    {
        var source = "mt";

        var result = Adapt.MapPathReverse(source).ToString();

        Assert.Equal("lastModified", result);
    }

    [Fact]
    public void Should_adapt_from_data_field()
    {
        var source = "do.test";

        var result = Adapt.MapPathReverse(source).ToString();

        Assert.Equal("data.test", result);
    }
}
