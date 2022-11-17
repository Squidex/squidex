// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Squidex.Assets;
using Squidex.Domain.Apps.Entities.Assets.Commands;
using Squidex.Domain.Apps.Entities.Properties;
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Assets;

public class SvgAssetMetadataSourceTests
{
    private readonly MemoryStream stream = new MemoryStream();
    private readonly SvgAssetMetadataSource sut = new SvgAssetMetadataSource();

    public SvgAssetMetadataSourceTests()
    {
        sut = new SvgAssetMetadataSource();
    }

    [Fact]
    public async Task Should_add_image_tag_if_svg_mime()
    {
        var svg = new DelegateAssetFile("MyImage.png", "image/svg+xml", 1024, () => stream);

        var command = new CreateAsset { File = svg };

        await sut.EnhanceAsync(command, default);

        Assert.Contains("image", command.Tags);
    }

    [Fact]
    public async Task Should_add_image_tag_if_svg_extension()
    {
        var svg = new DelegateAssetFile("MyImage.svg", "other", 1024, () => stream);

        var command = new CreateAsset { File = svg };

        await sut.EnhanceAsync(command, default);

        Assert.Contains("image", command.Tags);
    }

    [Fact]
    public async Task Should_throw_exception_if_svg_is_malicious()
    {
        var bytes = Encoding.UTF8.GetBytes(Resources.SvgInvalid);

        stream.Write(bytes);
        stream.Seek(0, SeekOrigin.Begin);

        var svg = new DelegateAssetFile("MyImage.svg", "other", 1024, () => stream);

        var command = new CreateAsset { File = svg };

        await Assert.ThrowsAsync<ValidationException>(() => sut.EnhanceAsync(command, default));
    }

    [Fact]
    public async Task Should_not_throw_exception_if_svg_is_not_malicious()
    {
        var bytes = Encoding.UTF8.GetBytes(Resources.SvgValid);

        stream.Write(bytes);
        stream.Seek(0, SeekOrigin.Begin);

        var svg = new DelegateAssetFile("MyImage.svg", "other", 1024, () => stream);

        var command = new CreateAsset { File = svg };

        await sut.EnhanceAsync(command, default);
    }
}
