// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Templates;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetsFluidExtensionTests
    {
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        private readonly IAssetFileStore assetFileStore = A.Fake<IAssetFileStore>();
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly FluidTemplateEngine sut;

        public AssetsFluidExtensionTests()
        {
            var services =
                new ServiceCollection()
                    .AddSingleton(appProvider)
                    .AddSingleton(assetQuery)
                    .AddSingleton(assetFileStore)
                    .BuildServiceProvider();

            var extensions = new IFluidExtension[]
            {
                new AssetsFluidExtension(services)
            };

            A.CallTo(() => appProvider.GetAppAsync(appId.Id, false, default))
                .Returns(Mocks.App(appId));

            sut = new FluidTemplateEngine(extensions);
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

            var result = await sut.RenderAsync(template, vars);

            Assert.Equal(Cleanup(expected), Cleanup(result));
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

            var result = await sut.RenderAsync(template, vars);

            Assert.Equal(Cleanup(expected), Cleanup(result));
        }

        [Fact]
        public async Task Should_resolve_asset_text()
        {
            var (vars, asset) = SetupAssetVars();

            SetupText(asset.Id, Encoding.UTF8.GetBytes("Hello Asset"));

            var template = @"
                {% assign ref = event.data.assets.iv[0] | asset %}
                Text: {{ ref | assetText }}
            ";

            var expected = $@"
                Text: Hello Asset
            ";

            var result = await sut.RenderAsync(template, vars);

            Assert.Equal(Cleanup(expected), Cleanup(result));
        }

        [Fact]
        public async Task Should_resolve_asset_text_with_utf8()
        {
            var (vars, asset) = SetupAssetVars();

            SetupText(asset.Id, Encoding.UTF8.GetBytes("Hello Asset"));

            var template = @"
                {% assign ref = event.data.assets.iv[0] | asset %}
                Text: {{ ref | assetText: 'utf8' }}
            ";

            var expected = $@"
                Text: Hello Asset
            ";

            var result = await sut.RenderAsync(template, vars);

            Assert.Equal(Cleanup(expected), Cleanup(result));
        }

        [Fact]
        public async Task Should_resolve_asset_text_with_unicode()
        {
            var (vars, asset) = SetupAssetVars();

            SetupText(asset.Id, Encoding.Unicode.GetBytes("Hello Asset"));

            var template = @"
                {% assign ref = event.data.assets.iv[0] | asset %}
                Text: {{ ref | assetText: 'unicode' }}
            ";

            var expected = $@"
                Text: Hello Asset
            ";

            var result = await sut.RenderAsync(template, vars);

            Assert.Equal(Cleanup(expected), Cleanup(result));
        }

        [Fact]
        public async Task Should_resolve_asset_text_with_ascii()
        {
            var (vars, asset) = SetupAssetVars();

            SetupText(asset.Id, Encoding.ASCII.GetBytes("Hello Asset"));

            var template = @"
                {% assign ref = event.data.assets.iv[0] | asset %}
                Text: {{ ref | assetText: 'ascii' }}
            ";

            var expected = $@"
                Text: Hello Asset
            ";

            var result = await sut.RenderAsync(template, vars);

            Assert.Equal(Cleanup(expected), Cleanup(result));
        }

        [Fact]
        public async Task Should_resolve_asset_text_with_base64()
        {
            var (vars, asset) = SetupAssetVars();

            SetupText(asset.Id, Encoding.UTF8.GetBytes("Hello Asset"));

            var template = @"
                {% assign ref = event.data.assets.iv[0] | asset %}
                Text: {{ ref | assetText: 'base64' }}
            ";

            var expected = $@"
                Text: SGVsbG8gQXNzZXQ=
            ";

            var result = await sut.RenderAsync(template, vars);

            Assert.Equal(Cleanup(expected), Cleanup(result));
        }

        [Fact]
        public async Task Should_not_resolve_asset_text_if_too_big()
        {
            var (vars, _) = SetupAssetVars(1_000_000);

            var template = @"
                {% assign ref = event.data.assets.iv[0] | asset %}
                Text: {{ ref | assetText }}
            ";

            var expected = $@"
                Text: ErrorTooBig
            ";

            var result = await sut.RenderAsync(template, vars);

            Assert.Equal(Cleanup(expected), Cleanup(result));

            A.CallTo(() => assetFileStore.DownloadAsync(A<DomainId>._, A<DomainId>._, A<long>._, null, A<Stream>._, A<BytesRange>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_resolve_asset_text_from_event()
        {
            var @event = new EnrichedAssetEvent
            {
                Id = DomainId.NewGuid(),
                FileVersion = 0,
                FileSize = 100,
                AppId = appId
            };

            SetupText(@event.Id, Encoding.UTF8.GetBytes("Hello Asset"));

            var vars = new TemplateVars
            {
                ["event"] = @event
            };

            var template = @"
                Text: {{ event | assetText }}
            ";

            var expected = $@"
                Text: Hello Asset
            ";

            var result = await sut.RenderAsync(template, vars);

            Assert.Equal(Cleanup(expected), Cleanup(result));
        }

        [Fact]
        public async Task Should_not_resolve_asset_text_from_event_if_too_big()
        {
            var @event = new EnrichedAssetEvent
            {
                Id = DomainId.NewGuid(),
                FileVersion = 0,
                FileSize = 1_000_000,
                AppId = appId
            };

            var vars = new TemplateVars
            {
                ["event"] = @event
            };

            var template = @"
                Text: {{ event | assetText }}
            ";

            var expected = $@"
                Text: ErrorTooBig
            ";

            var result = await sut.RenderAsync(template, vars);

            Assert.Equal(Cleanup(expected), Cleanup(result));

            A.CallTo(() => assetFileStore.DownloadAsync(A<DomainId>._, A<DomainId>._, A<long>._, null, A<Stream>._, A<BytesRange>._, A<CancellationToken>._))
                .MustNotHaveHappened();
        }

        private void SetupText(DomainId id, byte[] bytes)
        {
            A.CallTo(() => assetFileStore.DownloadAsync(appId.Id, id, 0, null, A<Stream>._, A<BytesRange>._, A<CancellationToken>._))
                .Invokes(x =>
                {
                    var stream = x.GetArgument<Stream>(4)!;

                    stream.Write(bytes);
                });
        }

        private (TemplateVars, IAssetEntity) SetupAssetVars(int fileSize = 100)
        {
            var assetId = DomainId.NewGuid();
            var asset = CreateAsset(assetId, 1, fileSize);

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

            SetupText(@event.Id, Encoding.UTF8.GetBytes("Hello Asset"));

            var vars = new TemplateVars
            {
                ["event"] = @event
            };

            return (vars, asset);
        }

        private (TemplateVars, IAssetEntity[]) SetupAssetsVars(int fileSize = 100)
        {
            var assetId1 = DomainId.NewGuid();
            var asset1 = CreateAsset(assetId1, 1, fileSize);
            var assetId2 = DomainId.NewGuid();
            var asset2 = CreateAsset(assetId2, 2, fileSize);

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

        private IEnrichedAssetEntity CreateAsset(DomainId assetId, int index, int fileSize = 100)
        {
            return new AssetEntity
            {
                AppId = appId,
                Id = assetId,
                FileSize = fileSize,
                FileName = $"file{index}.jpg",
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
}
