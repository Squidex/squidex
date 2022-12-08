// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Assets;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Assets;

public class AssetsJintExtensionTests : IClassFixture<TranslationsFixture>
{
    private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
    private readonly IAssetFileStore assetFileStore = A.Fake<IAssetFileStore>();
    private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
    private readonly IAssetThumbnailGenerator assetThumbnailGenerator = A.Fake<IAssetThumbnailGenerator>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly JintScriptEngine sut;

    public AssetsJintExtensionTests()
    {
        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(appProvider)
                .AddSingleton(assetFileStore)
                .AddSingleton(assetQuery)
                .AddSingleton(assetThumbnailGenerator)
                .BuildServiceProvider();

        var extensions = new IJintExtension[]
        {
            new AssetsJintExtension(serviceProvider)
        };

        A.CallTo(() => appProvider.GetAppAsync(appId.Id, false, A<CancellationToken>._))
            .Returns(Mocks.App(appId));

        sut = new JintScriptEngine(new MemoryCache(Options.Create(new MemoryCacheOptions())),
            Options.Create(new JintScriptOptions
            {
                TimeoutScript = TimeSpan.FromSeconds(2),
                TimeoutExecution = TimeSpan.FromSeconds(10)
            }),
            extensions);
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
    public async Task Should_resolve_asset()
    {
        var (vars, assets) = SetupAssetsVars(1);

        var expected = $@"
                Text: {assets[0].FileName} {assets[0].Id}
            ";

        var script = @"
                getAsset(data.assets.iv[0], function (assets) {
                    var actual1 = `Text: ${assets[0].fileName} ${assets[0].id}`;

                    complete(`${actual1}`);
                });";

        var actual = (await sut.ExecuteAsync(vars, script)).ToString();

        Assert.Equal(Cleanup(expected), Cleanup(actual));
    }

    [Fact]
    public async Task Should_resolve_assets()
    {
        var (vars, assets) = SetupAssetsVars(2);

        var expected = $@"
                Text: {assets[0].FileName} {assets[0].Id}
                Text: {assets[1].FileName} {assets[1].Id}
            ";

        var script = @"
                getAssets(data.assets.iv, function (assets) {
                    var actual1 = `Text: ${assets[0].fileName} ${assets[0].id}`;
                    var actual2 = `Text: ${assets[1].fileName} ${assets[1].id}`;

                    complete(`${actual1}\n${actual2}`);
                });";

        var actual = (await sut.ExecuteAsync(vars, script)).ToString();

        Assert.Equal(Cleanup(expected), Cleanup(actual));
    }

    [Theory]
    [MemberData(nameof(Encodings))]
    public async Task Should_resolve_text(string encoding)
    {
        var (vars, assets) = SetupAssetsVars(1);

        SetupText(assets[0].ToRef(), Encode(encoding, "hello+assets"));

        var expected = @"
                Text: hello+assets
            ";

        var script = $@"
                getAssets(data.assets.iv, function (assets) {{
                    getAssetText(assets[0], function (text) {{
                        var actual = `Text: ${{text}}`;

                        complete(actual);
                    }}, '{encoding}');
                }});";

        var actual = (await sut.ExecuteAsync(vars, script)).ToString();

        Assert.Equal(Cleanup(expected), Cleanup(actual));
    }

    [Fact]
    public async Task Should_not_resolve_text_if_too_big()
    {
        var (vars, _) = SetupAssetsVars(1, 1_000_000);

        var expected = @"
                Text: ErrorTooBig
            ";

        var script = @"
                getAssets(data.assets.iv, function (assets) {
                    getAssetText(assets[0], function (text) {
                        var actual = `Text: ${text}`;

                        complete(actual);
                    });
                });";

        var actual = (await sut.ExecuteAsync(vars, script)).ToString();

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

        var vars = new ScriptVars
        {
            ["event"] = @event
        };

        var expected = @"
                Text: hello+assets
            ";

        var script = $@"
                getAssetText(event, function (text) {{
                    var actual = `Text: ${{text}}`;

                    complete(actual);
                }}, '{encoding}');";

        var actual = (await sut.ExecuteAsync(vars, script)).ToString();

        Assert.Equal(Cleanup(expected), Cleanup(actual));
    }

    [Fact]
    public async Task Should_resolve_blur_hash()
    {
        var (vars, assets) = SetupAssetsVars(1);

        SetupBlurHash(assets[0].ToRef(), "Hash");

        var expected = @"
                Hash: Hash
            ";

        var script = @"
                getAssets(data.assets.iv, function (assets) {
                    getAssetBlurHash(assets[0], function (text) {
                        var actual = `Hash: ${text}`;

                        complete(actual);
                    });
                });";

        var actual = (await sut.ExecuteAsync(vars, script)).ToString();

        Assert.Equal(Cleanup(expected), Cleanup(actual));
    }

    [Fact]
    public async Task Should_not_resolve_blur_hash_if_too_big()
    {
        var (vars, assets) = SetupAssetsVars(1, 1_000_000);

        SetupBlurHash(assets[0].ToRef(), "Hash");

        var expected = @"
                Hash: null
            ";

        var script = @"
                getAssets(data.assets.iv, function (assets) {
                    getAssetBlurHash(assets[0], function (text) {
                        var actual = `Hash: ${text}`;

                        complete(actual);
                    });
                });";

        var actual = (await sut.ExecuteAsync(vars, script)).ToString();

        Assert.Equal(Cleanup(expected), Cleanup(actual));
    }

    [Fact]
    public async Task Should_not_resolve_blue_hash_if_not_an_image()
    {
        var (vars, assets) = SetupAssetsVars(1, type: AssetType.Audio);

        SetupBlurHash(assets[0].ToRef(), "Hash");

        var expected = @"
                Hash: null
            ";

        var script = @"
                getAssets(data.assets.iv, function (assets) {
                    getAssetBlurHash(assets[0], function (text) {
                        var actual = `Hash: ${text}`;

                        complete(actual);
                    });
                });";

        var actual = (await sut.ExecuteAsync(vars, script)).ToString();

        Assert.Equal(Cleanup(expected), Cleanup(actual));
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

        var vars = new ScriptVars
        {
            ["event"] = @event
        };

        var expected = @"
                Text: Hash
            ";

        var script = @"
                getAssetBlurHash(event, function (text) {
                    var actual = `Text: ${text}`;

                    complete(actual);
                });";

        var actual = (await sut.ExecuteAsync(vars, script)).ToString();

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

    private (ScriptVars, IAssetEntity[]) SetupAssetsVars(int count, int fileSize = 100, AssetType type = AssetType.Image)
    {
        var assets = Enumerable.Range(0, count).Select(x => CreateAsset(1, fileSize, type)).ToArray();
        var assetIds = assets.Select(x => x.Id);

        var user = new ClaimsPrincipal();

        var data =
            new ContentData()
                .AddField("assets",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(assetIds)));

        A.CallTo(() => assetQuery.QueryAsync(
                A<Context>.That.Matches(x => x.App.Id == appId.Id && x.UserPrincipal == user), null, A<Q>.That.HasIds(assetIds), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(2, assets));

        var vars = new ScriptVars
        {
            ["data"] = data,
            ["appId"] = appId.Id,
            ["appName"] = appId.Name,
            ["user"] = user
        };

        return (vars, assets);
    }

    private IEnrichedAssetEntity CreateAsset(int index, int fileSize = 100, AssetType type = AssetType.Image)
    {
        return new AssetEntity
        {
            AppId = appId,
            Id = DomainId.NewGuid(),
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
