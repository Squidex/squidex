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
using Squidex.Infrastructure.Validation;

namespace Squidex.Domain.Apps.Entities.Contents;

public class ContentsJintExtensionTests : GivenContext, IClassFixture<TranslationsFixture>
{
    private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
    private readonly JintScriptEngine sut;

    public ContentsJintExtensionTests()
    {
        var serviceProvider =
            new ServiceCollection()
                .AddSingleton(AppProvider)
                .AddSingleton(contentQuery)
                .BuildServiceProvider();

        var extensions = new IJintExtension[]
        {
            new ContentsJintExtension(serviceProvider)
        };

        sut = new JintScriptEngine(new MemoryCache(Options.Create(new MemoryCacheOptions())),
            Options.Create(new JintScriptOptions
            {
                TimeoutScript = TimeSpan.FromSeconds(2),
                TimeoutExecution = TimeSpan.FromSeconds(10)
            }),
            extensions);
    }

    [Fact]
    public async Task Should_throw_exception_if_callback_is_null()
    {
        var (vars, _) = SetupQueryVars("my-schema", "$filter=data/field/iv eq 42", 2);

        var script = @"getContents('my-schema', '$filter=data/field/iv eq 42')";

        await Assert.ThrowsAsync<ValidationException>(() => sut.ExecuteAsync(vars, script, ct: CancellationToken));
    }

    [Fact]
    public async Task Should_query_contents()
    {
        var (vars, _) = SetupQueryVars("my-schema", "$filter=data/field/iv eq 42", 2);

        var expected = @"
                Text: Hello 1 World 1
            ";

        var script = @"
                getContents('my-schema', { query: '$filter=data/field/iv eq 42' }, function (references) {
                    var actual1 = `Text: ${references[0].data.field1.iv} ${references[0].data.field2.iv}`;

                    complete(`${actual1}`);
                })";

        var actual = (await sut.ExecuteAsync(vars, script, ct: CancellationToken)).ToString();

        Assert.Equal(Cleanup(expected), Cleanup(actual));
    }

    [Fact]
    public async Task Should_query_contents_with_string()
    {
        var (vars, _) = SetupQueryVars("my-schema", "$filter=data/field/iv eq 42", 2);

        var expected = @"
                Text: Hello 1 World 1
            ";

        var script = @"
                getContents('my-schema', '$filter=data/field/iv eq 42', function (references) {
                    var actual1 = `Text: ${references[0].data.field1.iv} ${references[0].data.field2.iv}`;

                    complete(`${actual1}`);
                })";

        var actual = (await sut.ExecuteAsync(vars, script, ct: CancellationToken)).ToString();

        Assert.Equal(Cleanup(expected), Cleanup(actual));
    }

    private (ScriptVars, EnrichedContent[]) SetupQueryVars(string schema, string filter, int count)
    {
        var references = Enumerable.Range(0, count).Select((x, i) => CreateContent(i + 1)).ToArray();
        var referenceIds = references.Select(x => x.Id);

        var user = new ClaimsPrincipal();

        A.CallTo(() => contentQuery.QueryAsync(
                A<Context>.That.Matches(x => x.App == App && x.UserPrincipal == user),
                schema,
                A<Q>.That.Matches(x => x.QueryAsOdata == filter),
                A<CancellationToken>._))
            .Returns(ResultList.CreateFrom(2, [CreateContent(1)]));

        var vars = new ScriptVars
        {
            ["appId"] = AppId.Id,
            ["appName"] = AppId.Name,
            ["user"] = user
        };

        return (vars, references);
    }

    private EnrichedContent CreateContent(int index)
    {
        return CreateContent() with
        {
            Data =
                new ContentData()
                    .AddField("field1",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Create($"Hello {index}")))
                    .AddField("field2",
                        new ContentFieldData()
                            .AddInvariant(JsonValue.Create($"World {index}")))
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
