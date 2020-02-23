// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using FakeItEasy;
using Orleans;
using Squidex.Domain.Apps.Entities.Contents.Text.Lucene;
using Squidex.Infrastructure.Log;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public sealed class LuceneIndexFactory : IIndexerFactory
    {
        private readonly IGrainFactory grainFactory = A.Fake<IGrainFactory>();
        private readonly IIndexStorage storage;
        private LuceneTextIndexGrain grain;

        public LuceneIndexFactory(IIndexStorage storage)
        {
            this.storage = storage;

            A.CallTo(() => grainFactory.GetGrain<ILuceneTextIndexGrain>(A<Guid>._, null))
                .ReturnsLazily(() => grain);
        }

        public async Task<ITextIndex> CreateAsync(Guid schemaId)
        {
            var indexManager = new IndexManager(storage, A.Fake<ISemanticLog>());

            grain = new LuceneTextIndexGrain(indexManager);

            await grain.ActivateAsync(schemaId);

            return new LuceneTextIndex(grainFactory, indexManager);
        }

        public async Task CleanupAsync()
        {
            if (grain != null)
            {
                await grain.OnDeactivateAsync();
            }
        }
    }
}
