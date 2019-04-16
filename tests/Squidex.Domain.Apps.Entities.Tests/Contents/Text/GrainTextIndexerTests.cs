// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Events.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Orleans;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public class GrainTextIndexerTests
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly ITextIndexerGrain grain = A.Fake<ITextIndexerGrain>();
        private readonly Guid schemaId = Guid.NewGuid();
        private readonly Guid contentId = Guid.NewGuid();
        private readonly NamedContentData data = new NamedContentData();
        private readonly GrainTextIndexer sut;

        public GrainTextIndexerTests()
        {
            A.CallTo(() => grainFactory.GetGrain<ITextIndexerGrain>(schemaId, null))
                .Returns(grain);

            sut = new GrainTextIndexer(grainFactory);
        }

        [Fact]
        public async Task Should_call_grain_when_content_deleted()
        {
            await sut.On(E(new ContentDeleted()));

            A.CallTo(() => grain.DeleteAsync(contentId))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_when_content_created()
        {
            await sut.On(E(new ContentCreated { Data = data }));

            A.CallTo(() => grain.IndexAsync(A<J<Update>>.That.Matches(x => x.Value.Data == data && x.Value.Id == contentId && x.Value.OnlyDraft)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_when_content_updated()
        {
            await sut.On(E(new ContentUpdated { Data = data }));

            A.CallTo(() => grain.IndexAsync(A<J<Update>>.That.Matches(x => x.Value.Data == data && x.Value.Id == contentId && !x.Value.OnlyDraft)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_when_content_change_proposed()
        {
            await sut.On(E(new ContentUpdateProposed { Data = data }));

            A.CallTo(() => grain.IndexAsync(A<J<Update>>.That.Matches(x => x.Value.Data == data && x.Value.Id == contentId && x.Value.OnlyDraft)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_when_content_change_published()
        {
            await sut.On(E(new ContentChangesPublished()));

            A.CallTo(() => grain.CopyAsync(contentId, true))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_when_content_change_discarded()
        {
            await sut.On(E(new ContentChangesDiscarded()));

            A.CallTo(() => grain.CopyAsync(contentId, false))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_when_content_published()
        {
            await sut.On(E(new ContentStatusChanged { Status = Status.Published }));

            A.CallTo(() => grain.CopyAsync(contentId, true))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_call_grain_when_searching()
        {
            var foundIds = new List<Guid> { Guid.NewGuid() };

            A.CallTo(() => grain.SearchAsync("Search", A<SearchContext>.Ignored))
                .Returns(foundIds);

            var ids = await sut.SearchAsync("Search", GetApp(), schemaId, Scope.Draft);

            Assert.Equal(foundIds, ids);
        }

        [Fact]
        public async Task Should_not_call_grain_when_input_is_empty()
        {
            var ids = await sut.SearchAsync(string.Empty, GetApp(), schemaId, Scope.Published);

            Assert.Null(ids);

            A.CallTo(() => grain.SearchAsync(A<string>.Ignored, A<SearchContext>.Ignored))
                .MustNotHaveHappened();
        }

        private static IAppEntity GetApp()
        {
            var app = A.Fake<IAppEntity>();

            A.CallTo(() => app.LanguagesConfig).Returns(LanguagesConfig.Build(Language.EN, Language.DE));

            return app;
        }

        private Envelope<IEvent> E(ContentEvent contentEvent)
        {
            contentEvent.ContentId = contentId;
            contentEvent.SchemaId = NamedId.Of(schemaId, "my-schema");

            return new Envelope<IEvent>(contentEvent);
        }
    }
}
