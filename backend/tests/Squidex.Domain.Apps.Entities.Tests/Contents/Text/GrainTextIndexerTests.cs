//// ==========================================================================
////  Squidex Headless CMS
//// ==========================================================================
////  Copyright (c) Squidex UG (haftungsbeschraenkt)
////  All rights reserved. Licensed under the MIT license.
//// ==========================================================================

//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using FakeItEasy;
//using Orleans;
//using Squidex.Domain.Apps.Core.Contents;
//using Squidex.Domain.Apps.Entities.Apps;
//using Squidex.Domain.Apps.Entities.TestHelpers;
//using Squidex.Domain.Apps.Events.Contents;
//using Squidex.Infrastructure;
//using Squidex.Infrastructure.EventSourcing;
//using Xunit;

//namespace Squidex.Domain.Apps.Entities.Contents.Text
//{
//    public class GrainTextIndexerTests
//    {
//        private readonly IAppEntity app;
//        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
//        private readonly ITextIndexerGrain grain = A.Fake<ITextIndexerGrain>();
//        private readonly Guid contentId = Guid.NewGuid();
//        private readonly NamedId<Guid> appId = NamedId.Of(Guid.NewGuid(), "my-app");
//        private readonly NamedId<Guid> schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");
//        private readonly GrainTextIndexer sut;

//        public GrainTextIndexerTests()
//        {
//            app = Mocks.App(appId);

//            A.CallTo(() => grainFactory.GetGrain<ITextIndexerGrain>(schemaId.Id, null))
//                .Returns(grain);

//            sut = new GrainTextIndexer(grainFactory);
//        }

//        [Fact]
//        public async Task Should_call_grain_when_content_deleted()
//        {
//            await sut.On(E(new ContentDeleted()));

//            A.CallTo(() => grain.DeleteAsync(contentId))
//                .MustHaveHappened();
//        }

//        [Fact]
//        public async Task Should_call_grain_when_content_created()
//        {
//            await sut.On(E(new ContentCreated()));

//            A.CallTo(() => grain.IndexAsync(A<Update>.That.Matches(x => x.Text.Count == 0 && x.Id == contentId && x.OnlyDraft)))
//                .MustHaveHappened();
//        }

//        [Fact]
//        public async Task Should_call_grain_when_content_updated()
//        {
//            await sut.On(E(new ContentUpdated()));

//            A.CallTo(() => grain.IndexAsync(A<Update>.That.Matches(x => x.Text.Count == 0 && x.Id == contentId && !x.OnlyDraft)))
//                .MustHaveHappened();
//        }

//        [Fact]
//        public async Task Should_call_grain_when_content_change_proposed()
//        {
//            await sut.On(E(new ContentUpdateProposed()));

//            A.CallTo(() => grain.IndexAsync(A<Update>.That.Matches(x => x.Text.Count == 0 && x.Id == contentId && x.OnlyDraft)))
//                .MustHaveHappened();
//        }

//        [Fact]
//        public async Task Should_call_grain_when_content_change_published()
//        {
//            await sut.On(E(new ContentChangesPublished()));

//            A.CallTo(() => grain.CopyAsync(contentId, true))
//                .MustHaveHappened();
//        }

//        [Fact]
//        public async Task Should_call_grain_when_content_change_discarded()
//        {
//            await sut.On(E(new ContentChangesDiscarded()));

//            A.CallTo(() => grain.CopyAsync(contentId, false))
//                .MustHaveHappened();
//        }

//        [Fact]
//        public async Task Should_call_grain_when_content_published()
//        {
//            await sut.On(E(new ContentStatusChanged { Status = Status.Published }));

//            A.CallTo(() => grain.CopyAsync(contentId, true))
//                .MustHaveHappened();
//        }

//        [Fact]
//        public async Task Should_call_grain_when_searching()
//        {
//            var foundIds = new List<Guid> { Guid.NewGuid() };

//            A.CallTo(() => grain.SearchAsync("Search", A<SearchContext>.Ignored))
//                .Returns(foundIds);

//            var ids = await sut.SearchAsync("Search", app, schemaId.Id, Scope.Draft);

//            Assert.Equal(foundIds, ids);
//        }

//        [Fact]
//        public async Task Should_not_call_grain_when_input_is_empty()
//        {
//            var ids = await sut.SearchAsync(string.Empty, app, schemaId.Id, Scope.Published);

//            Assert.Null(ids);

//            A.CallTo(() => grain.SearchAsync(A<string>.Ignored, A<SearchContext>.Ignored))
//                .MustNotHaveHappened();
//        }

//        private Envelope<IEvent> E(ContentEvent contentEvent)
//        {
//            contentEvent.ContentId = contentId;
//            contentEvent.SchemaId = schemaId;

//            return new Envelope<IEvent>(contentEvent);
//        }
//    }
//}
