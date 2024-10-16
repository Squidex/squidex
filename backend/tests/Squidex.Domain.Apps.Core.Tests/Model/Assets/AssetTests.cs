// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Model.Assets;

#pragma warning disable SA1310 // Field names must not contain underscore

public class AssetTests
{
    private readonly Asset asset_0 = new Asset();

    [Fact]
    public void Should_move()
    {
        var newParentId = DomainId.NewGuid();

        var asset_1 = asset_0.Move(newParentId);
        var asset_2 = asset_1.Move(newParentId);

        Assert.NotSame(asset_0, asset_1);
        Assert.Equal(newParentId, asset_1.ParentId);
        Assert.Equal(newParentId, asset_2.ParentId);
        Assert.Same(asset_1, asset_2);
    }

    [Fact]
    public void Should_not_annotate_with_null_file_name()
    {
        var asset_1 = asset_0.Annotate(fileName: null);

        Assert.Same(asset_1, asset_0);
    }

    [Fact]
    public void Should_annotate_with_file_name()
    {
        var newFileName = "MyFile.png";

        var asset_1 = asset_0.Annotate(fileName: newFileName);
        var asset_2 = asset_1.Annotate(fileName: newFileName);

        Assert.NotSame(asset_0, asset_1);
        Assert.Equal(newFileName, asset_1.FileName);
        Assert.Equal(newFileName, asset_2.FileName);
        Assert.Same(asset_1, asset_2);
    }

    [Fact]
    public void Should_not_annotate_with_null_slug()
    {
        var asset_1 = asset_0.Annotate(slug: null);

        Assert.Same(asset_1, asset_0);
    }

    [Fact]
    public void Should_annotate_with_slug()
    {
        var newSlug = "my-file.png";

        var asset_1 = asset_0.Annotate(slug: newSlug);
        var asset_2 = asset_1.Annotate(slug: newSlug);

        Assert.NotSame(asset_0, asset_1);
        Assert.Equal(newSlug, asset_1.Slug);
        Assert.Equal(newSlug, asset_2.Slug);
        Assert.Same(asset_1, asset_2);
    }

    [Fact]
    public void Should_not_annotate_with_null_protected()
    {
        var asset_1 = asset_0.Annotate(isProtected: null);

        Assert.Same(asset_1, asset_0);
    }

    [Fact]
    public void Should_annotate_with_protected()
    {
        var newProtected = true;

        var asset_1 = asset_0.Annotate(isProtected: newProtected);
        var asset_2 = asset_1.Annotate(isProtected: newProtected);

        Assert.NotSame(asset_0, asset_1);
        Assert.Equal(newProtected, asset_1.IsProtected);
        Assert.Equal(newProtected, asset_2.IsProtected);
        Assert.Same(asset_1, asset_2);
    }

    [Fact]
    public void Should_not_annotate_with_null_tags()
    {
        var asset_1 = asset_0.Annotate(tags: null);

        Assert.Same(asset_1, asset_0);
    }

    [Fact]
    public void Should_annotate_with_tags()
    {
        var newTags1 = new HashSet<string> { "tag1" };
        var newTags2 = new HashSet<string> { "tag1" };

        var asset_1 = asset_0.Annotate(tags: newTags1);
        var asset_2 = asset_1.Annotate(tags: newTags2);

        Assert.NotSame(asset_0, asset_1);
        Assert.Equal(newTags1, asset_1.Tags);
        Assert.Equal(newTags2, asset_2.Tags);
        Assert.Same(asset_1, asset_2);
    }

    [Fact]
    public void Should_not_annotate_with_null_metadata()
    {
        var asset_1 = asset_0.Annotate(metadata: null);

        Assert.Same(asset_1, asset_0);
    }

    [Fact]
    public void Should_annotate_with_metadata()
    {
        var newMetadata1 = new AssetMetadata { [KnownMetadataKeys.PixelWidth] = 1024 };
        var newMetadata2 = new AssetMetadata { [KnownMetadataKeys.PixelWidth] = 1024 };

        var asset_1 = asset_0.Annotate(metadata: newMetadata1);
        var asset_2 = asset_1.Annotate(metadata: newMetadata2);

        Assert.NotSame(asset_0, asset_1);
        Assert.Equal(newMetadata1, asset_1.Metadata);
        Assert.Equal(newMetadata2, asset_2.Metadata);
        Assert.Same(asset_1, asset_2);
    }

    [Fact]
    public void Should_annotate_with_multiple_properties()
    {
        var newSlug = "my-file.png";
        var newFile = "My File";

        var asset_1 = asset_0.Annotate(fileName: newFile, slug: newSlug);
        var asset_2 = asset_1.Annotate(fileName: newFile, slug: newSlug);

        Assert.NotSame(asset_0, asset_1);
        Assert.Equal(newSlug, asset_1.Slug);
        Assert.Equal(newSlug, asset_2.Slug);
        Assert.Equal(newFile, asset_1.FileName);
        Assert.Equal(newFile, asset_2.FileName);
        Assert.Same(asset_1, asset_2);
    }

    [Fact]
    public void Should_deserialize_state()
    {
        var json = File.ReadAllText("Model/Assets/Asset.json");

        var deserialized = TestUtils.DefaultSerializer.Deserialize<Asset>(json);

        Assert.NotNull(deserialized);
    }

    [Fact]
    public void Should_serialize_deserialize_state()
    {
        var json = File.ReadAllText("Model/Assets/Asset.json").CleanJson();

        var serialized = TestUtils.SerializeWithoutNulls(TestUtils.DefaultSerializer.Deserialize<Asset>(json));

        Assert.Equal(json, serialized);
    }
}
