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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Log;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public class TextIndexerBenchmark
    {
        private readonly Guid schemaId = Guid.NewGuid();
        private readonly TextIndexerGrain sut;

        public TextIndexerBenchmark()
        {
            var factory = new IndexManager(new FileIndexStorage(), A.Fake<ISemanticLog>());

            sut = new TextIndexerGrain(factory);
            sut.ActivateAsync(schemaId).Wait();
        }

        [Fact]// (Skip = "Only used for benchmarks")]
        public async Task Should_index_many_documents()
        {
            var text = new Dictionary<string, string>
            {
                ["iv"] = "Hallo Welt"
            };

            var ids = new Guid[10000];

            for (var i = 0; i < ids.Length; i++)
            {
                ids[i] = Guid.NewGuid();
            }

            var watch = ValueStopwatch.StartNew();

            foreach (var id in ids)
            {
                await sut.IndexAsync(new Update { Text = text, Id = id });
            }

            sut.OnDeactivateAsync().Wait();

            var elapsed = watch.Stop();

            Assert.InRange(elapsed, 0, 1);
        }
    }
}
