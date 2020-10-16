// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Rules.EnrichedEvents;
using Squidex.Domain.Apps.Core.Templates;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents
{
    public class ReferenceFluidExtensionTests
    {
        private readonly IContentQueryService contentQuery = A.Fake<IContentQueryService>();
        private readonly IAppProvider appProvider = A.Fake<IAppProvider>();
        private readonly NamedId<DomainId> appId = NamedId.Of(DomainId.NewGuid(), "my-app");
        private readonly FluidTemplateEngine sut;

        public ReferenceFluidExtensionTests()
        {
            var extensions = new IFluidExtension[]
            {
                new ReferencesFluidExtension(contentQuery, appProvider)
            };

            A.CallTo(() => appProvider.GetAppAsync(appId.Id, false))
                .Returns(Mocks.App(appId));

            sut = new FluidTemplateEngine(extensions);
        }

        [Fact]
        public async Task Should_resolve_references_in_loop()
        {
            var referenceId1 = DomainId.NewGuid();
            var reference1 = CreateReference(referenceId1, 1);
            var referenceId2 = DomainId.NewGuid();
            var reference2 = CreateReference(referenceId1, 2);

            var @event = new EnrichedContentEvent
            {
                Data =
                    new NamedContentData()
                        .AddField("references",
                            new ContentFieldData()
                                .AddJsonValue(JsonValue.Array(referenceId1, referenceId2))),
                AppId = appId
            };

            A.CallTo(() => contentQuery.QueryAsync(A<Context>._, A<IReadOnlyList<DomainId>>.That.Contains(referenceId1)))
                .Returns(ResultList.CreateFrom(1, reference1));

            A.CallTo(() => contentQuery.QueryAsync(A<Context>._, A<IReadOnlyList<DomainId>>.That.Contains(referenceId2)))
                .Returns(ResultList.CreateFrom(1, reference2));

            var vars = new TemplateVars
            {
                ["event"] = @event
            };

            var template = @"
{% for id in event.data.references.iv %}
    {% reference 'ref', id %}
    Text: {{ ref.data.field1.iv }} {{ ref.data.field2.iv }}
{% endfor %}
";

            var expected = @"
    Text: Hello 1 World 1
    Text: Hello 2 World 2
";

            var result = await sut.RenderAsync(template, vars);

            Assert.Equal(expected, result);
        }

        private static IEnrichedContentEntity CreateReference(DomainId referenceId, int index)
        {
            return new ContentEntity
            {
                Data =
                    new NamedContentData()
                        .AddField("field1",
                            new ContentFieldData()
                                .AddJsonValue(JsonValue.Create($"Hello {index}")))
                        .AddField("field2",
                            new ContentFieldData()
                                .AddJsonValue(JsonValue.Create($"World {index}"))),
                Id = referenceId
            };
        }
    }
}
