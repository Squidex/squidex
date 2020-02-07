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
        private TextIndexerGrain grain;

        public LuceneIndexFactory(IIndexStorage storage)
        {
            this.storage = storage;

            A.CallTo(() => grainFactory.GetGrain<ITextIndexerGrain>(A<Guid>.Ignored, null))
                .ReturnsLazily(() => grain);
        }

        public async Task<ITextIndexer> CreateAsync(Guid schemaId)
        {
            grain = new TextIndexerGrain(new IndexManager(storage, A.Fake<ISemanticLog>()));

            await grain.ActivateAsync(schemaId);

            return new LuceneTextIndexer(grainFactory);
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
