// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Assets;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Assets
{
    public class AssetsJintExtensionTests : IClassFixture<TranslationsFixture>
    {
        private readonly IAssetQueryService assetQuery = A.Fake<IAssetQueryService>();
        private readonly IAssetFileStore assetFileStore = A.Fake<IAssetFileStore>();
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly JintScriptEngine sut;

        public AssetsJintExtensionTests()
        {
            var services =
                new ServiceCollection()
                    .AddSingleton(appProvider)
                    .AddSingleton(assetQuery)
                    .AddSingleton(assetFileStore)
                    .BuildServiceProvider();

            var extensions = new IJintExtension[]
            {
                new AssetsJintExtension(services)
            };

            A.CallTo(() => appProvider.GetAppAsync(appId.Id, false))
                .Returns(Mocks.App(appId));

            sut = new JintScriptEngine(new MemoryCache(Options.Create(new MemoryCacheOptions())), extensions)
            {
                TimeoutScript = TimeSpan.FromSeconds(2),
                TimeoutExecution = TimeSpan.FromSeconds(10)
            };
        }

        [Fact]
        public async Task Should_resolve_asset()
        {
            var assetId1 = DomainId.NewGuid();
            var asset1 = CreateAsset(assetId1, 1);

            var user = new ClaimsPrincipal();

            var data =
                new ContentData()
                    .AddField("assets",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(assetId1)));

            A.CallTo(() => assetQuery.QueryAsync(
                    A<Context>.That.Matches(x => x.App.Id == appId.Id && x.User == user), null, A<Q>.That.HasIds(assetId1), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(1, asset1));

            var vars = new ScriptVars { Data = data, AppId = appId.Id, User = user };

            var script = @"
                getAsset(data.assets.iv[0], function (assets) {
                    var result1 = `Text: ${assets[0].fileName}`;

                    complete(`${result1}`);
                });";

            var expected = @"
                Text: file1.jpg
            ";

            var result = (await sut.ExecuteAsync(vars, script)).ToString();

            Assert.Equal(Cleanup(expected), Cleanup(result));
        }

        [Fact]
        public async Task Should_resolve_assets()
        {
            var assetId1 = DomainId.NewGuid();
            var asset1 = CreateAsset(assetId1, 1);
            var assetId2 = DomainId.NewGuid();
            var asset2 = CreateAsset(assetId1, 2);

            var user = new ClaimsPrincipal();

            var data =
                new ContentData()
                    .AddField("assets",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(assetId1, assetId2)));

            A.CallTo(() => assetQuery.QueryAsync(
                    A<Context>.That.Matches(x => x.App.Id == appId.Id && x.User == user), null, A<Q>.That.HasIds(assetId1, assetId2), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(2, asset1, asset2));

            var vars = new ScriptVars { Data = data, AppId = appId.Id, User = user };

            var script = @"
                getAssets(data.assets.iv, function (assets) {
                    var result1 = `Text: ${assets[0].fileName}`;
                    var result2 = `Text: ${assets[1].fileName}`;

                    complete(`${result1}\n${result2}`);
                });";

            var expected = @"
                Text: file1.jpg
                Text: file2.jpg
            ";

            var result = (await sut.ExecuteAsync(vars, script)).ToString();

            Assert.Equal(Cleanup(expected), Cleanup(result));
        }

        [Fact]
        public async Task Should_resolve_asset_text()
        {
            var assetId = DomainId.NewGuid();
            var asset = CreateAsset(assetId, 1);

            var user = new ClaimsPrincipal();

            var data =
                new ContentData()
                    .AddField("assets",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(assetId)));

            A.CallTo(() => assetQuery.QueryAsync(
                    A<Context>.That.Matches(x => x.App.Id == appId.Id && x.User == user), null, A<Q>.That.HasIds(assetId), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(2, asset));

            A.CallTo(() => assetFileStore.DownloadAsync(appId.Id, assetId, asset.FileVersion, A<Stream>._, A<BytesRange>._, A<CancellationToken>._))
                .Invokes(x =>
                {
                    var stream = x.GetArgument<Stream>(3)!;

                    stream.Write(Encoding.UTF8.GetBytes("Hello Asset"));
                });

            var vars = new ScriptVars { Data = data, AppId = appId.Id, User = user };

            var script = @"
                getAssets(data.assets.iv, function (assets) {
                    getAssetText(assets[0], function (text) {
                        var result = `Text: ${text}`;

                        complete(result);
                    });
                });";

            var expected = @"
                Text: Hello Asset
            ";

            var result = (await sut.ExecuteAsync(vars, script)).ToString();

            Assert.Equal(Cleanup(expected), Cleanup(result));
        }

        [Fact]
        public async Task Should_not_resolve_asset_text_if_too_big()
        {
            var assetId = DomainId.NewGuid();
            var asset = CreateAsset(assetId, 1, 1_000_000);

            var user = new ClaimsPrincipal();

            var data =
                new ContentData()
                    .AddField("assets",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(assetId)));

            A.CallTo(() => assetQuery.QueryAsync(
                    A<Context>.That.Matches(x => x.App.Id == appId.Id && x.User == user), null, A<Q>.That.HasIds(assetId), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(2, asset));

            var vars = new ScriptVars { Data = data, AppId = appId.Id, User = user };

            var script = @"
                getAssets(data.assets.iv, function (assets) {
                    getAssetText(assets[0], function (text) {
                        var result = `Text: ${text}`;

                        complete(result);
                    });
                });";

            var expected = @"
                Text: ErrorTooBig
            ";

            var result = (await sut.ExecuteAsync(vars, script)).ToString();

            Assert.Equal(Cleanup(expected), Cleanup(result));

            A.CallTo(() => assetFileStore.DownloadAsync(A<DomainId>._, A<DomainId>._, A<long>._, A<Stream>._, A<BytesRange>._, A<CancellationToken>._))
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

            A.CallTo(() => assetFileStore.DownloadAsync(appId.Id, @event.Id, @event.FileVersion, A<Stream>._, A<BytesRange>._, A<CancellationToken>._))
                .Invokes(x =>
                {
                    var stream = x.GetArgument<Stream>(3)!;

                    stream.Write(Encoding.UTF8.GetBytes("Hello Asset"));
                });

            var vars = new ScriptVars
            {
                ["event"] = @event
            };

            var script = @"
                getAssetText(event, function (text) {
                    var result = `Text: ${text}`;

                    complete(result);
                });";

            var expected = @"
                Text: Hello Asset
            ";

            var result = (await sut.ExecuteAsync(vars, script)).ToString();

            Assert.Equal(Cleanup(expected), Cleanup(result));
        }

        [Fact]
        public async Task Should_resolve_asset_text_from_event_if_too_big()
        {
            var @event = new EnrichedAssetEvent
            {
                Id = DomainId.NewGuid(),
                FileVersion = 0,
                FileSize = 1_000_000,
                AppId = appId
            };

            var vars = new ScriptVars
            {
                ["event"] = @event
            };

            var script = @"
                getAssetText(event, function (text) {
                    var result = `Text: ${text}`;

                    complete(result);
                });";

            var expected = @"
                Text: ErrorTooBig
            ";

            var result = (await sut.ExecuteAsync(vars, script)).ToString();

            Assert.Equal(Cleanup(expected), Cleanup(result));

            A.CallTo(() => assetFileStore.DownloadAsync(A<DomainId>._, A<DomainId>._, A<long>._, A<Stream>._, A<BytesRange>._, A<CancellationToken>._))
                .MustNotHaveHappened();
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
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty)
                .Replace(" ", string.Empty);
        }
    }
}
