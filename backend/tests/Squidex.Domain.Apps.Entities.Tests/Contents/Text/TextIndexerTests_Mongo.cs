// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using Squidex.Domain.Apps.Entities.MongoDb.FullText;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Contents.Text
{
    [Trait("Category", "Dependencies")]
    public class TextIndexerTests_Mongo : TextIndexerTestsBase
    {
        private sealed class TheFactory : IIndexerFactory
        {
            private readonly MongoClient mongoClient = new MongoClient("mongodb://localhost");

            public Task CleanupAsync()
            {
                return Task.CompletedTask;
            }

            public async Task<ITextIndex> CreateAsync(Guid schemaId)
            {
                var database = mongoClient.GetDatabase("FullText");

                var index = new MongoTextIndex(database, false);

                await index.InitializeAsync();

                return index;
            }
        }

        public override IIndexerFactory Factory { get; } = new TheFactory();

        public TextIndexerTests_Mongo()
        {
            SupportsSearchSyntax = false;
            SupportsMultiLanguage = false;
        }
    }
}