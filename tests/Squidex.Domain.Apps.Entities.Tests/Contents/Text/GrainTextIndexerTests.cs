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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
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
        private readonly GrainTextIndexer sut;

        public GrainTextIndexerTests()
        {
            A.CallTo(() => grainFactory.GetGrain<ITextIndexerGrain>(schemaId, null))
                .Returns(grain);

            sut = new GrainTextIndexer(grainFactory, A.Fake<ISemanticLog>());
        }

        [Fact]
        public async Task Should_call_grain_when_deleting_entry()
        {
            await sut.DeleteAsync(schemaId, contentId);

            A.CallTo(() => grain.DeleteAsync(contentId))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_catch_exception_when_deleting_failed()
        {
            A.CallTo(() => grain.DeleteAsync(contentId))
                .Throws(new InvalidOperationException());

            await sut.DeleteAsync(schemaId, contentId);
        }

        [Fact]
        public async Task Should_call_grain_when_indexing_data()
        {
            var data = new NamedContentData();
            var dataDraft = new NamedContentData();

            await sut.IndexAsync(schemaId, contentId, data, dataDraft);

            A.CallTo(() => grain.IndexAsync(contentId, A<J<IndexData>>.That.Matches(x => x.Value.Data == data && !x.Value.IsDraft)))
                .MustHaveHappened();

            A.CallTo(() => grain.IndexAsync(contentId, A<J<IndexData>>.That.Matches(x => x.Value.Data == dataDraft && x.Value.IsDraft)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_call_grain_when_data_is_null()
        {
            var dataDraft = new NamedContentData();

            await sut.IndexAsync(schemaId, contentId, null, dataDraft);

            A.CallTo(() => grain.IndexAsync(contentId, A<J<IndexData>>.That.Matches(x => !x.Value.IsDraft)))
                .MustNotHaveHappened();

            A.CallTo(() => grain.IndexAsync(contentId, A<J<IndexData>>.That.Matches(x => x.Value.Data == dataDraft && x.Value.IsDraft)))
                .MustHaveHappened();
        }

        [Fact]
        public async Task Should_not_call_grain_when_data_draft_is_null()
        {
            var data = new NamedContentData();

            await sut.IndexAsync(schemaId, contentId, data, null);

            A.CallTo(() => grain.IndexAsync(contentId, A<J<IndexData>>.That.Matches(x => x.Value.Data == data && !x.Value.IsDraft)))
                .MustHaveHappened();

            A.CallTo(() => grain.IndexAsync(contentId, A<J<IndexData>>.That.Matches(x => x.Value.IsDraft)))
                .MustNotHaveHappened();
        }

        [Fact]
        public async Task Should_catch_exception_when_indexing_failed()
        {
            var data = new NamedContentData();

            A.CallTo(() => grain.IndexAsync(contentId, A<J<IndexData>>.Ignored))
                .Throws(new InvalidOperationException());

            await sut.IndexAsync(schemaId, contentId, data, null);
        }

        [Fact]
        public async Task Should_call_grain_when_searching()
        {
            var foundIds = new List<Guid> { Guid.NewGuid() };

            A.CallTo(() => grain.SearchAsync("Search", A<SearchContext>.Ignored))
                .Returns(foundIds);

            var ids = await sut.SearchAsync("Search", GetApp(), schemaId, true);

            Assert.Equal(foundIds, ids);
        }

        [Fact]
        public async Task Should_not_call_grain_when_input_is_empty()
        {
            var ids = await sut.SearchAsync(string.Empty, GetApp(), schemaId, false);

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
    }
}
