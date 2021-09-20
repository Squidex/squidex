// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ReferencesJintExtensionTests : IClassFixture<TranslationsFixture>
    {
        private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly JintScriptEngine sut;

        public ReferencesJintExtensionTests()
        {
            var services =
                new ServiceCollection()
                    .AddSingleton(appProvider)
                    .AddSingleton(contentQuery)
                    .BuildServiceProvider();

            var extensions = new IJintExtension[]
            {
                new ReferencesJintExtension(services)
            };

            A.CallTo(() => appProvider.GetAppAsync(appId.Id, false, default))
                .Returns(Mocks.App(appId));

            sut = new JintScriptEngine(new MemoryCache(Options.Create(new MemoryCacheOptions())), extensions)
            {
                TimeoutScript = TimeSpan.FromSeconds(2),
                TimeoutExecution = TimeSpan.FromSeconds(10)
            };
        }

        [Fact]
        public async Task Should_resolve_reference()
        {
            var referenceId1 = DomainId.NewGuid();
            var reference1 = CreateReference(referenceId1, 1);

            var user = new ClaimsPrincipal();

            var data =
                new ContentData()
                    .AddField("references",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(referenceId1)));

            A.CallTo(() => contentQuery.QueryAsync(
                    A<Context>.That.Matches(x => x.App.Id == appId.Id && x.User == user), A<Q>.That.HasIds(referenceId1), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(1, reference1));

            var vars = new ScriptVars
            {
                ["appId"] = appId.Id,
                ["data"] = data,
                ["dataOld"] = null,
                ["user"] = user
            };

            var script = @"
                getReference(data.references.iv[0], function (references) {
                    var result1 = `Text: ${references[0].data.field1.iv} ${references[0].data.field2.iv}`;

                    complete(`${result1}`);
                })";

            var expected = @"
                Text: Hello 1 World 1
            ";

            var result = (await sut.ExecuteAsync(vars, script)).ToString();

            Assert.Equal(Cleanup(expected), Cleanup(result));
        }

        [Fact]
        public async Task Should_resolve_references()
        {
            var referenceId1 = DomainId.NewGuid();
            var reference1 = CreateReference(referenceId1, 1);
            var referenceId2 = DomainId.NewGuid();
            var reference2 = CreateReference(referenceId1, 2);

            var user = new ClaimsPrincipal();

            var data =
                new ContentData()
                    .AddField("references",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Array(referenceId1, referenceId2)));

            A.CallTo(() => contentQuery.QueryAsync(
                    A<Context>.That.Matches(x => x.App.Id == appId.Id && x.User == user), A<Q>.That.HasIds(referenceId1, referenceId2), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(2, reference1, reference2));

            var vars = new ScriptVars
            {
                ["appId"] = appId.Id,
                ["data"] = data,
                ["dataOld"] = null,
                ["user"] = user
            };

            var script = @"
                getReferences(data.references.iv, function (references) {
                    var result1 = `Text: ${references[0].data.field1.iv} ${references[0].data.field2.iv}`;
                    var result2 = `Text: ${references[1].data.field1.iv} ${references[1].data.field2.iv}`;

                    complete(`${result1}\n${result2}`);
                })";

            var expected = @"
                Text: Hello 1 World 1
                Text: Hello 2 World 2
            ";

            var result = (await sut.ExecuteAsync(vars, script)).ToString();

            Assert.Equal(Cleanup(expected), Cleanup(result));
        }

        private static IEnrichedContentEntity CreateReference(DomainId referenceId, int index)
        {
            return new ContentEntity
            {
                Data =
                    new ContentData()
                        .AddField("field1",
                            new ContentFieldData()
                                .AddInvariant(JsonValue.Create($"Hello {index}")))
                        .AddField("field2",
                            new ContentFieldData()
                                .AddInvariant(JsonValue.Create($"World {index}"))),
                Id = referenceId
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
