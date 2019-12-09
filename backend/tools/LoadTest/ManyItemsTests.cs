// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using LoadTest.Model;
using LoadTest.Utils;
using Squidex.ClientLibrary;
using Xunit;

namespace LoadTest
{
    public class ManyItemsTests
    {
        private readonly SquidexClient<TestEntity, TestEntityData> client;

        public ManyItemsTests()
        {
            client = TestClient.BuildAsync("multiple").Result;
        }

        [Fact]
        public async Task Should_read_many_async()
        {
            var contents = await client.GetAsync();

            for (var i = contents.Total; i < 20000; i++)
            {
                await client.CreateAsync(new TestEntityData
                {
                    String = RandomString.Create(1000)
                }, publish: true);
            }

            var found = await client.GetAll2Async();

            Assert.Equal(20000, found.Items.Count);
        }
    }

    public static class SquidexClientExtensions
    {
        public static async Task<SquidexEntities<TEntity, TData>> GetAll2Async<TEntity, TData>(this SquidexClient<TEntity, TData> client, int batchSize = 200)
            where TEntity : SquidexEntityBase<TData>
            where TData : class, new()
        {
            var query = new ODataQuery { Top = batchSize, Skip = 0 };

            var entities = new SquidexEntities<TEntity, TData>();
            do
            {
                var getResult = await client.GetAsync(query);

                entities.Total = getResult.Total;
                entities.Items.AddRange(getResult.Items);

                query.Skip += getResult.Items.Count;
            }
            while (query.Skip < entities.Total);

            return entities;
        }
    }
}
