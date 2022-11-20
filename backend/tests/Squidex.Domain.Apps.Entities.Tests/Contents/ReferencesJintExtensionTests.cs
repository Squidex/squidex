// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting;
using Squidex.Domain.Apps.Core.TestHelpers;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Entities.Contents;

public class ReferencesJintExtensionTests : IClassFixture<TranslationsFixture>
{
    private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
    private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
    private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
    private readonly JintScriptEngine sut;

    public ReferencesJintExtensionTests()
    {
        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(appProvider)
                .AddSingleton(contentQuery)
                .BuildServiceProvider();

        var extensions = new IJintExtension[]
        {
            new ReferencesJintExtension(serviceProvider)
        };

        A.CallTo(() => appProvider.GetAppAsync(appId.Id, false, default))
            .Returns(Mocks.App(appId));

        sut = new JintScriptEngine(new MemoryCache(Options.Create(new MemoryCacheOptions())),
            Options.Create(new JintScriptOptions
            {
                TimeoutScript = TimeSpan.FromSeconds(2),
                TimeoutExecution = TimeSpan.FromSeconds(10)
            }),
            extensions);
    }

    [Fact]
    public async Task Should_resolve_reference()
    {
        var (vars, _) = SetupReferenceVars(1);

        var expected = @"
                Text: Hello 1 World 1
            ";

        var script = @"
                getReference(data.references.iv[0], function (references) {
                    var actual1 = `Text: ${references[0].data.field1.iv} ${references[0].data.field2.iv}`;

                    complete(`${actual1}`);
                })";

        var actual = (await sut.ExecuteAsync(vars, script)).ToString();

        Assert.Equal(Cleanup(expected), Cleanup(actual));
    }

    [Fact]
    public async Task Should_resolve_references()
    {
        var (vars, _) = SetupReferenceVars(2);

        var expected = @"
                Text: Hello 1 World 1
                Text: Hello 2 World 2
            ";

        var script = @"
                getReferences(data.references.iv, function (references) {
                    var actual1 = `Text: ${references[0].data.field1.iv} ${references[0].data.field2.iv}`;
                    var actual2 = `Text: ${references[1].data.field1.iv} ${references[1].data.field2.iv}`;

                    complete(`${actual1}\n${actual2}`);
                })";

        var actual = (await sut.ExecuteAsync(vars, script)).ToString();

        Assert.Equal(Cleanup(expected), Cleanup(actual));
    }

    private (ScriptVars, IContentEntity[]) SetupReferenceVars(int count)
    {
        var references = Enumerable.Range(0, count).Select((x, i) => CreateReference(i + 1)).ToArray();
        var referenceIds = references.Select(x => x.Id);

        var user = new ClaimsPrincipal();

        var data =
            new ContentData()
                .AddField("references",
                    new ContentFieldData()
                        .AddInvariant(JsonValue.Array(referenceIds)));

        A.CallTo(() => contentQuery.QueryAsync(
                A<Context>.That.Matches(x => x.App.Id == appId.Id && x.UserPrincipal == user), A<Q>.That.HasIds(referenceIds), A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(2, references));

        var vars = new ScriptVars
        {
            ["appId"] = appId.Id,
            ["data"] = data,
            ["dataOld"] = null,
            ["user"] = user
        };

        return (vars, references);
    }

    private static IEnrichedContentEntity CreateReference(int index)
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
            Id = DomainId.NewGuid()
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
