// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Squidex.Domain.Apps.Entities.Contents.State;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Infrastructure.Migrations;
using Squidex.Infrastructure.States;

namespace Migrate_01.Migrations
{
    public sealed class BuildFullTextIndices : IMigration
    {
        private readonly ITextIndexer textIndexer;
        private readonly IStore<Guid> store;

        public BuildFullTextIndices(ITextIndexer textIndexer, IStore<Guid> store)
        {
            this.textIndexer = textIndexer;

            this.store = store;
        }

        public async Task UpdateAsync()
        {
            var snapshotStore = store.GetSnapshotStore<ContentState>();

            var worker = new ActionBlock<ContentState>(state =>
                {
                    return textIndexer.IndexAsync(state.SchemaId.Id, state.Id, state.Data, state.DataDraft);
                },
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount * 2
                });

            await snapshotStore.ReadAllAsync((state, version) => worker.SendAsync(state));

            worker.Complete();

            await worker.Completion;
        }
    }
}
