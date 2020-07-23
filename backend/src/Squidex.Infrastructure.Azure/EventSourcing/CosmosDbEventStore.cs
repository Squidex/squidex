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
using Index = Microsoft.Azure.Documents.Index;

namespace Squidex.Infrastructure.EventSourcing
{
    public sealed partial class CosmosDbEventStore : DisposableObjectBase, IEventStore, IInitializable
    {
        private readonly DocumentClient documentClient;
        private readonly Uri collectionUri;
        private readonly Uri databaseUri;
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
            get { return documentClient.ServiceEndpoint; }
        }

        public CosmosDbEventStore(DocumentClient documentClient, string masterKey, string database, JsonSerializerSettings serializerSettings)
        {
            Guard.NotNull(documentClient, nameof(documentClient));
            Guard.NotNull(serializerSettings, nameof(serializerSettings));
            Guard.NotNullOrEmpty(masterKey, nameof(masterKey));
            Guard.NotNullOrEmpty(database, nameof(database));

            this.documentClient = documentClient;

            databaseUri = UriFactory.CreateDatabaseUri(database);
            databaseId = database;

            collectionUri = UriFactory.CreateDocumentCollectionUri(database, Constants.Collection);

            this.masterKey = masterKey;

            this.serializerSettings = serializerSettings;
        }

        protected override void DisposeObject(bool disposing)
        {
            if (disposing)
            {
                documentClient.Dispose();
            }
        }

        public async Task InitializeAsync(CancellationToken ct = default)
        {
            await documentClient.CreateDatabaseIfNotExistsAsync(new Database { Id = databaseId });

            await documentClient.CreateDocumentCollectionIfNotExistsAsync(databaseUri,
                new DocumentCollection
                {
                    PartitionKey = new PartitionKeyDefinition
                    {
                        Paths = new Collection<string>
                        {
                            "/id"
                        }
                    },
                    Id = Constants.LeaseCollection
                });

            await documentClient.CreateDocumentCollectionIfNotExistsAsync(databaseUri,
                new DocumentCollection
                {
                    PartitionKey = new PartitionKeyDefinition
                    {
                        Paths = new Collection<string>
                        {
                            "/eventStream"
                        }
                    },
                    IndexingPolicy = new IndexingPolicy
                    {
                        IncludedPaths = new Collection<IncludedPath>
                        {
                            new IncludedPath
                            {
                                Path = "/*",
                                Indexes = new Collection<Index>
                                {
                                    Index.Range(DataType.Number),
                                    Index.Range(DataType.String)
                                }
                            }
                        }
                    },
                    UniqueKeyPolicy = new UniqueKeyPolicy
                    {
                        UniqueKeys = new Collection<UniqueKey>
                        {
                            new UniqueKey
                            {
                                Paths = new Collection<string>
                                {
                                    "/eventStream",
                                    "/eventStreamOffset"
                                }
                            }
                        }
                    },
                    Id = Constants.Collection
                },
                new RequestOptions
                {
                    PartitionKey = new PartitionKey("/eventStream")
                });
        }
    }
}
