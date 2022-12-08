// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Entities.Contents.Commands;
using Squidex.Domain.Apps.Entities.Contents.Queries;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Commands;

namespace Squidex.Domain.Apps.Entities.Contents.DomainObject;

public sealed class ContentCommandMiddlewareTests : HandlerTestBase<ContentDomainObject.State>
{
    private readonly IDomainObjectCache domainObjectCache = A.Fake<IDomainObjectCache>();
    private readonly IDomainObjectFactory domainObjectFactory = A.Fake<IDomainObjectFactory>();
    private readonly IContentEnricher contentEnricher = A.Fake<IContentEnricher>();
    private readonly IContextProvider contextProvider = A.Fake<IContextProvider>();
    private readonly DomainId contentId = DomainId.NewGuid();
    private readonly Context requestContext;
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
        requestContext = Context.Anonymous(Mocks.App(AppNamedId));

        A.CallTo(() => contextProvider.Context)
            .Returns(requestContext);

        sut = new ContentCommandMiddleware(
            domainObjectFactory,
            domainObjectCache,
            contentEnricher,
            contextProvider);
    }

    [Fact]
    public async Task Should_not_invoke_enricher_for_other_actual()
    {
        await HandleAsync(new CreateContent(), 12);

        A.CallTo(() => contentEnricher.EnrichAsync(A<IEnrichedContentEntity>._, A<bool>._, requestContext, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_not_invoke_enricher_if_already_enriched()
    {
        var actual = new ContentEntity();

        var context =
            await HandleAsync(new CreateContent(),
                actual);

        Assert.Same(actual, context.Result<IEnrichedContentEntity>());

        A.CallTo(() => contentEnricher.EnrichAsync(A<IEnrichedContentEntity>._, A<bool>._, requestContext, A<CancellationToken>._))
            .MustNotHaveHappened();
    }

    [Fact]
    public async Task Should_enrich_content_actual()
    {
        var actual = A.Fake<IContentEntity>();

        var enriched = new ContentEntity();

        A.CallTo(() => contentEnricher.EnrichAsync(actual, true, requestContext, A<CancellationToken>._))
            .Returns(enriched);

        var context =
            await HandleAsync(new CreateContent(),
                actual);

        Assert.Same(enriched, context.Result<IEnrichedContentEntity>());
    }

    private Task<CommandContext> HandleAsync(ContentCommand command, object actual)
    {
        command.ContentId = contentId;

        CreateCommand(command);

        var domainObject = A.Fake<ContentDomainObject>();

        A.CallTo(() => domainObject.ExecuteAsync(A<IAggregateCommand>._, A<CancellationToken>._))
            .Returns(new CommandResult(command.AggregateId, 1, 0, actual));

        A.CallTo(() => domainObjectFactory.Create<ContentDomainObject>(command.AggregateId))
            .Returns(domainObject);

        return HandleAsync(sut, command);
    }
}
