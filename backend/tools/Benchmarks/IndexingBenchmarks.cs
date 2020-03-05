// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Benchmarks.Utils;
using LoremNET;
using Orleans.Concurrency;
using Squidex.Domain.Apps.Entities.Contents.Text;
using Squidex.Domain.Apps.Entities.Contents.Text.Lucene;
using Squidex.Infrastructure;

namespace Benchmarks
{
    [ShortRunJob]
    [StopOnFirstError]
    [RPlotExporter]
    public class IndexingBenchmarks
    {
        private readonly IIndexStorage storageAssets = IndexStorages.Assets();
        private readonly IIndexStorage storageTempFolder = IndexStorages.TempFolder();
        private readonly IIndexStorage storageMongoDB = IndexStorages.MongoDB();
        private readonly Dictionary<string, string> texts;

        public IndexingBenchmarks()
        {
            texts = new Dictionary<string, string>
            {
                ["iv"] = Lorem.Paragraph(10, 10)
            };
        }

        [Params(1000)]
        public int N { get; set; }

        [Params(10)]
        public int M { get; set; }

        [Benchmark(Baseline = true)]
        public async Task Index_TempFolder()
        {
            await IndexAndSearchAsync(storageTempFolder);
        }

        [Benchmark]
        public async Task Index_Assets()
        {
            await IndexAndSearchAsync(storageAssets);
        }

        [Benchmark]
        public async Task Index_MongoDB()
        {
            await IndexAndSearchAsync(storageMongoDB);
        }

        private async Task IndexAndSearchAsync(IIndexStorage storage)
        {
            var schemaId = NamedId.Of(Guid.NewGuid(), "my-schema");

            var factory = new IndexManager(storage, new NoopLog());

            var grain = new LuceneTextIndexGrain(factory);

            await grain.ActivateAsync(Guid.NewGuid());

            for (var i = 0; i < M; i++)
            {
                var ids = new Guid[N];

                for (var j = 0; j < ids.Length; j++)
                {
                    ids[j] = Guid.NewGuid();
                }

                foreach (var id in ids)
                {
                    var commands = new IndexCommand[]
                    {
                        new UpsertIndexEntry
                        {
                             ContentId = id,
                             DocId = id.ToString(),
                             ServeAll = true,
                             ServePublished = true,
                             Texts = texts
                        }
                    };

                    await grain.IndexAsync(schemaId, commands.AsImmutable());
                }

                await grain.CommitAsync();
            }
        }
    }
}
