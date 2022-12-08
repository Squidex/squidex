// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Search;

public class SearchManagerTests
{
    private readonly ISearchSource source1 = A.Fake<ISearchSource>();
    private readonly ISearchSource source2 = A.Fake<ISearchSource>();
    private readonly ILogger<SearchManager> log = A.Fake<ILogger<SearchManager>>();
    private readonly Context requestContext = Context.Anonymous(Mocks.App(NamedId.Of(DomainId.NewGuid(), "my-app")));
    private readonly SearchManager sut;

    public SearchManagerTests()
    {
        sut = new SearchManager(new[] { source1, source2 }, log);
    }

    [Fact]
    public async Task Should_not_call_sources_and_return_empty_if_query_is_empty()
    {
        var actual = await sut.SearchAsync(string.Empty, requestContext);

        Assert.Empty(actual);

        A.CallTo(() => source1.SearchAsync(A<string>._, A<Context>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => source2.SearchAsync(A<string>._, A<Context>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_call_sources_and_return_empty_if_is_too_short()
    {
        var actual = await sut.SearchAsync("11", requestContext);

        Assert.Empty(actual);

        A.CallTo(() => source1.SearchAsync(A<string>._, A<Context>._, A<CancellationToken>._))
            .MustNotHaveHappened();

        A.CallTo(() => source2.SearchAsync(A<string>._, A<Context>._, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_aggregate_actuals_from_all_sources()
    {
        var actual1 = new SearchResults().Add("Name1", SearchResultType.Setting, "Url1");
        var actual2 = new SearchResults().Add("Name2", SearchResultType.Setting, "Url2");

        var query = "a query";

        A.CallTo(() => source1.SearchAsync(query, requestContext, A<CancellationToken>._))
            .Returns(actual1);

        A.CallTo(() => source2.SearchAsync(query, requestContext, A<CancellationToken>._))
            .Returns(actual2);

        var actual = await sut.SearchAsync(query, requestContext);

        actual.Should().BeEquivalentTo(
            new SearchResults()
                .Add("Name1", SearchResultType.Setting, "Url1")
                .Add("Name2", SearchResultType.Setting, "Url2"));
    }

    [Fact]
    public async Task Should_ignore_exception_from_source()
    {
        var actual2 = new SearchResults().Add("Name2", SearchResultType.Setting, "Url2");

        var query = "a query";

        A.CallTo(() => source1.SearchAsync(query, requestContext, A<CancellationToken>._))
            .Throws(new InvalidOperationException());

        A.CallTo(() => source2.SearchAsync(query, requestContext, A<CancellationToken>._))
            .Returns(actual2);

        var actual = await sut.SearchAsync(query, requestContext);

        actual.Should().BeEquivalentTo(actual2);

        A.CallTo(log).Where(x => x.Method.Name == "Log")
            .MustHaveHappened();
    }
}
