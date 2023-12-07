// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Queries;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject;

public sealed class ContentCommandMiddlewareTests : HandlerTestBase<WriteContent>
{
    private readonly IDomainObjectCache domainObjectCache = A.Fake<IDomainObjectCache>();
    private readonly IDomainObjectFactory domainObjectFactory = A.Fake<IDomainObjectFactory>();
    private readonly IContentEnricher contentEnricher = A.Fake<IContentEnricher>();
    private readonly DomainId contentId = DomainId.NewGuid();
    private readonly ContentCommandMiddleware sut;

    public sealed class MyCommand : SquidexCommand
    {
    }

    protected override DomainId Id
    {
        get => contentId;
    }

    public ContentCommandMiddlewareTests()
    {
        sut = new ContentCommandMiddleware(
            domainObjectFactory,
            domainObjectCache,
            contentEnricher,
            ApiContextProvider);
    }

    [Fact]
    public async Task Should_not_invoke_enricher_for_other_result()
    {
        await HandleAsync(new CreateContent(), 12);

        A.CallTo(() => contentEnricher.EnrichAsync(A<EnrichedContent>._, A<bool>._, ApiContext, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_invoke_enricher_if_already_enriched()
    {
        var actual = CreateContent();

        var context =
            await HandleAsync(new CreateContent(),
                actual);

        Assert.Same(actual, context.Result<Content>());

        A.CallTo(() => contentEnricher.EnrichAsync(A<Content>._, A<bool>._, ApiContext, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_enrich_content_result()
    {
        var actual = new Content();

        var enriched = CreateContent();

        A.CallTo(() => contentEnricher.EnrichAsync(actual, true, ApiContext, CancellationToken))
            .Returns(enriched);

        var context =
            await HandleAsync(new CreateContent(),
                actual);

        Assert.Same(enriched, context.Result<Content>());
    }

    [Fact]
    public async Task Should_enrich_write_content_result()
    {
        var actual = CreateWriteContent();

        var enriched = CreateContent();

        A.CallTo(() => contentEnricher.EnrichAsync(A<Content>._, true, ApiContext, CancellationToken))
            .Returns(enriched);

        var context =
            await HandleAsync(new CreateContent(),
                actual);

        Assert.Same(enriched, context.Result<Content>());
    }

    private Task<CommandContext> HandleAsync(ContentCommand command, object actual)
    {
        command.ContentId = contentId;

        CreateCommand(command);

        var domainObject = A.Fake<ContentDomainObject>();

        A.CallTo(() => domainObject.ExecuteAsync(A<IAggregateCommand>._, CancellationToken))
            .Returns(new CommandResult(command.AggregateId, 1, 0, actual));

        A.CallTo(() => domainObjectFactory.Create<ContentDomainObject>(command.AggregateId))
            .Returns(domainObject);

        return HandleAsync(sut, command, CancellationToken);
    }
}
