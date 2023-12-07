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

public class AssetFolderTests
{
    private readonly AssetFolder assetFolder_0 = new AssetFolder();

    [Fact]
    public void Should_throw_exception_if_new_name_is_null()
    {
        Assert.Throws<ArgumentNullException>(() => assetFolder_0.Rename(null!));
    }

    [Fact]
    public void Should_rename()
    {
        var newName = "MyFolder";

        var assetFolder_1 = assetFolder_0.Rename(newName);
        var assetFolder_2 = assetFolder_1.Rename(newName);

        Assert.NotSame(assetFolder_0, assetFolder_1);
        Assert.Equal(newName, assetFolder_1.FolderName);
        Assert.Equal(newName, assetFolder_2.FolderName);
        Assert.Same(assetFolder_1, assetFolder_2);
    }

    [Fact]
    public void Should_move()
    {
        var newParentId = DomainId.NewGuid();

        var assetFolder_1 = assetFolder_0.Move(newParentId);
        var assetFolder_2 = assetFolder_1.Move(newParentId);

        Assert.NotSame(assetFolder_0, assetFolder_1);
        Assert.Equal(newParentId, assetFolder_1.ParentId);
        Assert.Equal(newParentId, assetFolder_2.ParentId);
        Assert.Same(assetFolder_1, assetFolder_2);
    }

    [Fact]
    public void Should_deserialize_state()
    {
        var json = File.ReadAllText("Model/Assets/AssetFolder.json");

        var deserialized = TestUtils.DefaultSerializer.Deserialize<AssetFolder>(json);

        Assert.NotNull(deserialized);
    }

    [Fact]
    public void Should_serialize_deserialize_state()
    {
        var json = File.ReadAllText("Model/Assets/AssetFolder.json").CleanJson();

        var serialized = TestUtils.SerializeWithoutNulls(TestUtils.DefaultSerializer.Deserialize<AssetFolder>(json));

        Assert.Equal(json, serialized);
    }
}
