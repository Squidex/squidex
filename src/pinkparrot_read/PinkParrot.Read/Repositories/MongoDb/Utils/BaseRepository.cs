using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using PinkParrot.Infrastructure;

namespace PinkParrot.Read.Repositories.MongoDb.Utils
{
    public abstract class BaseRepository<T>
    {
        private readonly IMongoCollection<T> collection;
        private readonly IndexKeysDefinitionBuilder<T> indexKeys = new IndexKeysDefinitionBuilder<T>();

        protected IMongoCollection<T> Collection
        {
            get { return collection; }
        }

        protected IndexKeysDefinitionBuilder<T> IndexKeys
        {
            get { return indexKeys; }
        }

        protected BaseRepository(IMongoDatabase database, string collectioName)
        {
            Guard.NotNull(database, nameof(database));
            Guard.NotNullOrEmpty(collectioName, nameof(collectioName));

            collection = database.GetCollection<T>(collectioName);
        }
    }
}
