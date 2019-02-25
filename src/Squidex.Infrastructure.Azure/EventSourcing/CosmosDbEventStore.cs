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
using Newtonsoft.Json;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed partial class CosmosDbEventStore : IEventStore, IInitializable
    {
        private readonly DocumentClient documentClient;
        private readonly Uri databaseUri;
        private readonly Uri collectionUri;
        private readonly Uri serviceUri;
        private readonly string masterKey;
        private readonly string databaseId;
        private readonly JsonSerializerSettings serializerSettings;

        public JsonSerializerSettings SerializerSettings
        {
            get { return serializerSettings; }
        }

        public string DatabaseId
        {
            get { return databaseId; }
        }

        public string MasterKey
        {
            get { return masterKey; }
        }

        public Uri ServiceUri
        {
            get { return serviceUri; }
        }

        public CosmosDbEventStore(Uri uri, string masterKey, JsonSerializerSettings serializerSettings, string database)
        {
            Guard.NotNull(uri, nameof(uri));
            Guard.NotNull(serializerSettings, nameof(serializerSettings));
            Guard.NotNullOrEmpty(masterKey, nameof(masterKey));
            Guard.NotNullOrEmpty(database, nameof(database));

            documentClient = new DocumentClient(uri, masterKey, serializerSettings);

            databaseUri = UriFactory.CreateDatabaseUri(database);
            databaseId = database;

            collectionUri = UriFactory.CreateDocumentCollectionUri(database, Constants.Collection);

            serviceUri = uri;

            this.masterKey = masterKey;

            this.serializerSettings = serializerSettings;
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            await documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseId });

            await documentClient.CreateDocumentCollectionIfNotExistsAsync(databaseUri,
                new DocumentCollection
                {
                    Id = Constants.LeaseCollection,
                });

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
                                    $"/eventStream",
                                    $"/eventStreamOffset"
                                }
                            }
                        }
                    },
                    Id = Constants.Collection,
                },
                new RequestOptions
                {
                    PartitionKey = new PartitionKey($"/eventStream")
                });
        }
    }
}
