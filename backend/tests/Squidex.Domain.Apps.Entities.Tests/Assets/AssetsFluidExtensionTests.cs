// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly FluidTemplateEngine sut;

        public AssetsFluidExtensionTests()
        {
            var services =
                new ServiceCollection()
                    .AddSingleton(appProvider)
                    .AddSingleton(assetQuery)
                    .BuildServiceProvider();

            var extensions = new IFluidExtension[]
            {
                new AssetsFluidExtension(services)
            };

            A.CallTo(() => appProvider.GetAppAsync(appId.Id, false))
                .Returns(Mocks.App(appId));

            sut = new FluidTemplateEngine(extensions);
        }

        [Fact]
        public async Task Should_resolve_assets_in_loop()
        {
            var assetId1 = DomainId.NewGuid();
            var asset1 = CreateAsset(assetId1, 1);
            var assetId2 = DomainId.NewGuid();
            var asset2 = CreateAsset(assetId2, 2);

            var @event = new EnrichedContentEvent
            {
                Data =
                    new ContentData()
                        .AddField("assets",
                            new ContentFieldData()
                                .AddInvariant(JsonValue.Array(assetId1, assetId2))),
                AppId = appId
            };

            A.CallTo(() => assetQuery.FindAsync(A<Context>._, assetId1, EtagVersion.Any))
                .Returns(asset1);

            A.CallTo(() => assetQuery.FindAsync(A<Context>._, assetId2, EtagVersion.Any))
                .Returns(asset2);

            var vars = new TemplateVars
            {
                ["event"] = @event
            };

            var template = @"
                {% for id in event.data.assets.iv %}
                    {% asset 'ref', id %}
                    Text: {{ ref.fileName }} {{ ref.id }}
                {% endfor %}
                ";

            var expected = $@"
                Text: file1.jpg {assetId1}
                Text: file2.jpg {assetId2}
            ";

            var result = await sut.RenderAsync(template, vars);

            Assert.Equal(Cleanup(expected), Cleanup(result));
        }

        private static IEnrichedAssetEntity CreateAsset(DomainId assetId, int index)
        {
            return new AssetEntity { FileName = $"file{index}.jpg", Id = assetId };
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
