// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    public class TextIndexerBenchmark : IDisposable
    {
        private readonly Guid schemaId = Guid.NewGuid();
        private readonly TextIndexerGrain sut;

        public TextIndexerBenchmark()
        {
            var factory = new FSDirectoryFactory();

            sut = new TextIndexerGrain(factory);
            sut.ActivateAsync(schemaId).Wait();
        }

        public void Dispose()
        {
            sut.OnDeactivateAsync().Wait();
        }

        [Fact]
        public async Task Should_index_many_documents()
        {
            var data =
                new NamedContentData()
                    .AddField("test",
                        new ContentFieldData()
                            .AddValue("iv", "Hallo Welt"));

            var ids = new Guid[10000];

            for (var i = 0; i < ids.Length; i++)
            {
                ids[i] = Guid.NewGuid();
            }

            var watch = ValueStopwatch.StartNew();

            foreach (var id in ids)
            {
                await sut.IndexAsync(new Update { Data = data, Id = id });
            }

            sut.OnDeactivateAsync().Wait();

            var elapsed = watch.Stop();

            Assert.InRange(elapsed, 0, 1);
        }
    }
}
