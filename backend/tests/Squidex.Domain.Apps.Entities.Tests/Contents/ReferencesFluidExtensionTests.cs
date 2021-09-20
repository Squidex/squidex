// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading;
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

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ReferencesFluidExtensionTests
    {
        private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly FluidTemplateEngine sut;

        public ReferencesFluidExtensionTests()
        {
            var services =
                new ServiceCollection()
                    .AddSingleton(appProvider)
                    .AddSingleton(contentQuery)
                    .BuildServiceProvider();

            var extensions = new IFluidExtension[]
            {
                new ReferencesFluidExtension(services)
            };

            A.CallTo(() => appProvider.GetAppAsync(appId.Id, false, default))
                .Returns(Mocks.App(appId));

            sut = new FluidTemplateEngine(extensions);
        }

        [Fact]
        public async Task Should_resolve_references_in_loop()
        {
            var referenceId1 = DomainId.NewGuid();
            var reference1 = CreateReference(referenceId1, 1);
            var referenceId2 = DomainId.NewGuid();
            var reference2 = CreateReference(referenceId2, 2);

            var @event = new EnrichedContentEvent
            {
                Data =
                    new ContentData()
                        .AddField("references",
                            new ContentFieldData()
                                .AddInvariant(JsonValue.Array(referenceId1, referenceId2))),
                AppId = appId
            };

            A.CallTo(() => contentQuery.QueryAsync(A<Context>._, A<Q>.That.HasIds(referenceId1), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(1, reference1));

            A.CallTo(() => contentQuery.QueryAsync(A<Context>._, A<Q>.That.HasIds(referenceId2), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(1, reference2));

            var vars = new TemplateVars
            {
                ["event"] = @event
            };

            var template = @"
                {% for id in event.data.references.iv %}
                    {% reference 'ref', id %}
                    Text: {{ ref.data.field1.iv }} {{ ref.data.field2.iv }} {{ ref.id }}
                {% endfor %}
            ";

            var expected = $@"
                Text: Hello 1 World 1 {referenceId1}
                Text: Hello 2 World 2 {referenceId2}
            ";

            var result = await sut.RenderAsync(template, vars);

            Assert.Equal(Cleanup(expected), Cleanup(result));
        }

        [Fact]
        public async Task Should_resolve_references_in_loop_with_filter()
        {
            var referenceId1 = DomainId.NewGuid();
            var reference1 = CreateReference(referenceId1, 1);
            var referenceId2 = DomainId.NewGuid();
            var reference2 = CreateReference(referenceId2, 2);

            var @event = new EnrichedContentEvent
            {
                Data =
                    new ContentData()
                        .AddField("references",
                            new ContentFieldData()
                                .AddInvariant(JsonValue.Array(referenceId1, referenceId2))),
                AppId = appId
            };

            A.CallTo(() => contentQuery.QueryAsync(A<Context>._, A<Q>.That.HasIds(referenceId1), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(1, reference1));

            A.CallTo(() => contentQuery.QueryAsync(A<Context>._, A<Q>.That.HasIds(referenceId2), A<CancellationToken>._))
                .Returns(ResultList.CreateFrom(1, reference2));

            var vars = new TemplateVars
            {
                ["event"] = @event
            };

            var template = @"
                {% for id in event.data.references.iv %}
                    {% assign ref = id | reference %}
                    Text: {{ ref.data.field1.iv }} {{ ref.data.field2.iv }} {{ ref.id }}
                {% endfor %}
            ";

            var expected = $@"
                Text: Hello 1 World 1 {referenceId1}
                Text: Hello 2 World 2 {referenceId2}
            ";

            var result = await sut.RenderAsync(template, vars);

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
