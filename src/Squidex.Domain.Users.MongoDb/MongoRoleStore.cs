// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Users.MongoDb
{
    public sealed class MongoRoleStore : MongoRepositoryBase<MongoRole>, IRoleStore<IRole>, IRoleFactory
    {
        public MongoRoleStore(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Identity_Roles";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoRole> collection, CancellationToken ct = default(CancellationToken))
        {
            return collection.Indexes.CreateOneAsync(
                new CreateIndexModel<MongoRole>(Index.Ascending(x => x.NormalizedName), new CreateIndexOptions { Unique = true }), cancellationToken: ct);
        }

        protected override MongoCollectionSettings CollectionSettings()
        {
            return new MongoCollectionSettings { WriteConcern = WriteConcern.WMajority };
        }

        public void Dispose()
        {
        }

        public IRole Create(string name)
        {
            return new MongoRole { Name = name };
        }

        public async Task<IRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return await Collection.Find(x => x.Id == roleId).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return await Collection.Find(x => x.NormalizedName == normalizedRoleName).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IdentityResult> CreateAsync(IRole role, CancellationToken cancellationToken)
        {
            await Collection.InsertOneAsync((MongoRole)role, null, cancellationToken);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(IRole role, CancellationToken cancellationToken)
        {
            await Collection.ReplaceOneAsync(x => x.Id == ((MongoRole)role).Id, (MongoRole)role, null, cancellationToken);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(IRole role, CancellationToken cancellationToken)
        {
            await Collection.DeleteOneAsync(x => x.Id == ((MongoRole)role).Id, null, cancellationToken);

            return IdentityResult.Success;
        }

        public Task<string> GetRoleIdAsync(IRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoRole)role).Id);
        }

        public Task<string> GetRoleNameAsync(IRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoRole)role).Name);
        }

        public Task<string> GetNormalizedRoleNameAsync(IRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(((MongoRole)role).NormalizedName);
        }

        public Task SetRoleNameAsync(IRole role, string roleName, CancellationToken cancellationToken)
        {
            ((MongoRole)role).Name = roleName;

            return TaskHelper.Done;
        }

        public Task SetNormalizedRoleNameAsync(IRole role, string normalizedName, CancellationToken cancellationToken)
        {
            ((MongoRole)role).NormalizedName = normalizedName;

            return TaskHelper.Done;
        }
    }
}
