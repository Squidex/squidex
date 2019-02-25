// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed partial class CosmosDbEventStore : IEventStore, IInitializable
    {
        private readonly DocumentClient documentClient;
        private readonly Uri databaseUri;
        private readonly Uri collectionUri;
        private readonly string databaseId;
        private readonly string collectionId;

        public CosmosDbEventStore(DocumentClient documentClient, string database)
        {
            Guard.NotNull(documentClient, nameof(documentClient));
            Guard.NotNullOrEmpty(database, nameof(database));

            this.documentClient = documentClient;

            databaseUri = UriFactory.CreateDatabaseUri(database);
            databaseId = database;

            collectionUri = UriFactory.CreateDocumentCollectionUri(database, FilterBuilder.Collection);
            collectionId = FilterBuilder.Collection;
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            await documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseId });

            await documentClient.CreateDocumentCollectionIfNotExistsAsync(databaseUri,
                new DocumentCollection
                {
                    UniqueKeyPolicy = new UniqueKeyPolicy
                    {
                        UniqueKeys = new Collection<UniqueKey>
                        {
                            new UniqueKey
                            {
                                Paths = new Collection<string>
                                {
                                    $"/{FilterBuilder.EventStreamField}",
                                    $"/{FilterBuilder.EventStreamOffsetField}"
                                }
                            }
                        }
                    },
                    Id = FilterBuilder.Collection,
                },
                new RequestOptions
                {
                    PartitionKey = new PartitionKey($"/{FilterBuilder.EventStreamField}")
                });
        }
    }
}
