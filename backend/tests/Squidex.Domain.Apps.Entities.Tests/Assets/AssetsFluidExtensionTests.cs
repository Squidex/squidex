// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Templates;
using Squidex.Domain.Apps.Core.Templates.Extensions;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Assets;

public class AssetsFluidExtensionTests
{
    private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
    private readonly IAssetFileStore assetFileStore = A.Fake<IAssetFileStore>();
    private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
    private readonly IAssetThumbnailGenerator assetThumbnailGenerator = A.Fake<IAssetThumbnailGenerator>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly FluidTemplateEngine sut;

    public AssetsFluidExtensionTests()
    {
        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(appProvider)
                .AddSingleton(assetFileStore)
                .AddSingleton(assetQuery)
                .AddSingleton(assetThumbnailGenerator)
                .BuildServiceProvider();

        var extensions = new IFluidExtension[]
        {
            new ContentFluidExtension(),
            new AssetsFluidExtension(serviceProvider)
        };

        A.CallTo(() => appProvider.GetAppAsync(appId.Id, false, default))
            .Returns(Mocks.App(appId));

        sut = new FluidTemplateEngine(extensions);
    }

    public static IEnumerable<object[]> Encodings()
    {
        yield return new object[] { "ascii" };
        yield return new object[] { "unicode" };
        yield return new object[] { "utf8" };
        yield return new object[] { "base64" };
    }

    public static byte[] Encode(string encoding, string text)
    {
        switch (encoding)
        {
            case "base64":
                return Convert.FromBase64String(text);
            case "ascii":
                return Encoding.ASCII.GetBytes(text);
            case "unicode":
                return Encoding.Unicode.GetBytes(text);
            default:
                return Encoding.UTF8.GetBytes(text);
        }
    }

    [Fact]
    public async Task Should_resolve_assets_in_loop()
    {
        var (vars, assets) = SetupAssetsVars();

        var template = @"
                {% for id in event.data.assets.iv %}
                    {% asset 'ref', id %}
                    Text: {{ ref.fileName }} {{ ref.id }}
                {% endfor %}
            ";

        var expected = $@"
                Text: {assets[0].FileName} {assets[0].Id}
                Text: {assets[1].FileName} {assets[1].Id}
            ";

        var actual = await sut.RenderAsync(template, vars);

        Assert.Equal(Cleanup(expected), Cleanup(actual));
    }

    [Fact]
    public async Task Should_resolve_assets_in_loop_with_filter()
    {
        var (vars, assets) = SetupAssetsVars();

        var template = @"
                {% for id in event.data.assets.iv %}
                    {% assign ref = id | asset %}
                    Text: {{ ref.fileName }} {{ ref.id }}
                {% endfor %}
            ";

        var expected = $@"
                Text: {assets[0].FileName} {assets[0].Id}
                Text: {assets[1].FileName} {assets[1].Id}
            ";

        var actual = await sut.RenderAsync(template, vars);

        Assert.Equal(Cleanup(expected), Cleanup(actual));
    }

    [Theory]
    [MemberData(nameof(Encodings))]
    public async Task Should_resolve_text(string encoding)
    {
        var (vars, asset) = SetupAssetVars();

        SetupText(asset.ToRef(), Encode(encoding, "hello+assets"));

        var template = $@"
                {{% assign ref = event.data.assets.iv[0] | asset %}}
                Text: {{{{ ref | assetText: '{encoding}' }}}}
            ";

        var expected = $@"
                Text: hello+assets
            ";

        var actual = await sut.RenderAsync(template, vars);

        Assert.Equal(Cleanup(expected), Cleanup(actual));
    }

    [Fact]
    public async Task Should_not_resolve_text_if_too_big()
    {
        var (vars, _) = SetupAssetVars(1_000_000);

        var template = @"
                {% assign ref = event.data.assets.iv[0] | asset %}
                Text: {{ ref | assetText }}
            ";

        var expected = $@"
                Text: ErrorTooBig
            ";

        var actual = await sut.RenderAsync(template, vars);

        Assert.Equal(Cleanup(expected), Cleanup(actual));

        A.CallTo(() => assetFileStore.DownloadAsync(A<DomainId>._, A<DomainId>._, A<long>._, null, A<Stream>._, A<BytesRange>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Theory]
    [MemberData(nameof(Encodings))]
    public async Task Should_resolve_text_from_event(string encoding)
    {
        var @event = new EnrichedAssetEvent
        {
            Id = DomainId.NewGuid(),
            FileVersion = 0,
            FileSize = 100,
            AppId = appId
        };

        SetupText(@event.ToRef(), Encode(encoding, "hello+assets"));

        var vars = new TemplateVars
        {
            ["event"] = @event
        };

        var template = $@"
                Text: {{{{ event | assetText: '{encoding}' }}}}
            ";

        var expected = $@"
                Text: hello+assets
            ";

        var actual = await sut.RenderAsync(template, vars);

        Assert.Equal(Cleanup(expected), Cleanup(actual));
    }

    [Fact]
    public async Task Should_resolve_blur_hash()
    {
        var (vars, asset) = SetupAssetVars();

        SetupBlurHash(asset.ToRef(), "Hash");

        var template = @"
                {% assign ref = event.data.assets.iv[0] | asset %}
                Text: {{ ref | assetBlurHash: 3,4 }}
            ";

        var expected = $@"
                Text: Hash
            ";

        var actual = await sut.RenderAsync(template, vars);

        Assert.Equal(Cleanup(expected), Cleanup(actual));
    }

    [Fact]
    public async Task Should_not_resolve_blur_hash_if_too_big()
    {
        var (vars, _) = SetupAssetVars(1_000_000);

        var template = @"
                {% assign ref = event.data.assets.iv[0] | asset %}
                Text: {{ ref | assetBlurHash }}
            ";

        var expected = $@"
                Text: ErrorTooBig
            ";

        var actual = await sut.RenderAsync(template, vars);

        Assert.Equal(Cleanup(expected), Cleanup(actual));

        A.CallTo(() => assetFileStore.DownloadAsync(A<DomainId>._, A<DomainId>._, A<long>._, null, A<Stream>._, A<BytesRange>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_resolve_blur_hash_if_not_an_image()
    {
        var (vars, _) = SetupAssetVars(type: AssetType.Unknown);

        var template = @"
                {% assign ref = event.data.assets.iv[0] | asset %}
                Text: {{ ref | assetBlurHash }}
            ";

        var expected = $@"
                Text: NoImage
            ";

        var actual = await sut.RenderAsync(template, vars);

        Assert.Equal(Cleanup(expected), Cleanup(actual));

        A.CallTo(() => assetFileStore.DownloadAsync(A<DomainId>._, A<DomainId>._, A<long>._, null, A<Stream>._, A<BytesRange>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_resolve_blur_hash_from_event()
    {
        var @event = new EnrichedAssetEvent
        {
            Id = DomainId.NewGuid(),
            AssetType = AssetType.Image,
            FileVersion = 0,
            FileSize = 100,
            AppId = appId
        };

        SetupBlurHash(@event.ToRef(), "Hash");

        var vars = new TemplateVars
        {
            ["event"] = @event
        };

        var template = @"
                Text: {{ event | assetBlurHash }}
            ";

        var expected = $@"
                Text: Hash
            ";

        var actual = await sut.RenderAsync(template, vars);

        Assert.Equal(Cleanup(expected), Cleanup(actual));
    }

    private void SetupBlurHash(AssetRef asset, string hash)
    {
        A.CallTo(() => assetThumbnailGenerator.ComputeBlurHashAsync(A<Stream>._, asset.MimeType, A<BlurOptions>._, A<CancellationToken>._))
            .Returns(hash);
    }

    private void SetupText(AssetRef asset, byte[] bytes)
    {
        A.CallTo(() => assetFileStore.DownloadAsync(appId.Id, asset.Id, asset.FileVersion, null, A<Stream>._, A<BytesRange>._, A<CancellationToken>._))
            .Invokes(x => x.GetArgument<Stream>(4)?.Write(bytes));
    }

    private (TemplateVars, IAssetEntity) SetupAssetVars(int fileSize = 100, AssetType type = AssetType.Image)
    {
        var assetId = DomainId.NewGuid();
        var asset = CreateAsset(assetId, 1, fileSize, type);

        var @event = new EnrichedContentEvent
        {
            Data =
                new ContentData()
                    .AddField("assets",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(assetId))),
            AppId = appId
        };

        A.CallTo(() => assetQuery.FindAsync(A<Context>._, assetId, EtagVersion.Any, A<CancellationToken>._))
            .Returns(asset);

        var vars = new TemplateVars
        {
            ["event"] = @event
        };

        return (vars, asset);
    }

    private (TemplateVars, IAssetEntity[]) SetupAssetsVars(int fileSize = 100, AssetType type = AssetType.Image)
    {
        var assetId1 = DomainId.NewGuid();
        var asset1 = CreateAsset(assetId1, 1, fileSize, type);
        var assetId2 = DomainId.NewGuid();
        var asset2 = CreateAsset(assetId2, 2, fileSize, type);

        var @event = new EnrichedContentEvent
        {
            Data =
                new ContentData()
                    .AddField("assets",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(assetId1, assetId2))),
            AppId = appId
        };

        A.CallTo(() => assetQuery.FindAsync(A<Context>._, assetId1, EtagVersion.Any, A<CancellationToken>._))
            .Returns(asset1);

        A.CallTo(() => assetQuery.FindAsync(A<Context>._, assetId2, EtagVersion.Any, A<CancellationToken>._))
            .Returns(asset2);

        var vars = new TemplateVars
        {
            ["event"] = @event
        };

        return (vars, new[] { asset1, asset2 });
    }

    private IEnrichedAssetEntity CreateAsset(DomainId assetId, int index, int fileSize = 100, AssetType type = AssetType.Unknown)
    {
        return new AssetEntity
        {
            AppId = appId,
            Id = assetId,
            FileSize = fileSize,
            FileName = $"file{index}.jpg",
            MimeType = "image/jpg",
            Type = type
        };
    }

    private static string Cleanup(string text)
    {
        return text
            .Replace("\r", string.Empty, StringComparison.Ordinal)
            .Replace("\n", string.Empty, StringComparison.Ordinal)
            .Replace(" ", string.Empty, StringComparison.Ordinal);
    }
}
