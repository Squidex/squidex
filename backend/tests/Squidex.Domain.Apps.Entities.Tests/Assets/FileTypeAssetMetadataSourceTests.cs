// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.TestHelpers;

namespace Squidex.Domain.Apps.Entities.Assets;

public class FileTypeAssetMetadataSourceTests
{
    private readonly FileTypeAssetMetadataSource sut = new FileTypeAssetMetadataSource();

    [Fact]
    public async Task Should_not_add_tag_if_no_file_info()
    {
        var command = new CreateAsset();

        await sut.EnhanceAsync(command, default);

        Assert.Empty(command.Tags);
    }

    [Fact]
    public async Task Should_add_file_type()
    {
        var command = new CreateAsset
        {
            File = new NoopAssetFile("File.DOCX")
        };

        await sut.EnhanceAsync(command, default);

        Assert.Contains("type/docx", command.Tags);
    }

    [Fact]
    public async Task Should_add_blob_if_without_extension()
    {
        var command = new CreateAsset
        {
            File = new NoopAssetFile("File")
        };

        await sut.EnhanceAsync(command, default);

        Assert.Contains("type/blob", command.Tags);
    }

    [Fact]
    public void Should_always_format_to_empty()
    {
        var source = new AssetEntity();

        var formatted = sut.Format(source);

        Assert.Empty(formatted);
    }
}
