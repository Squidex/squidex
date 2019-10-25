﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Squidex.Infrastructure.MongoDb;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Domain.Users.MongoDb
{
    public sealed class MongoRoleStore : MongoRepositoryBase<IdentityRole>, IRoleStore<IdentityRole>
    {
        static MongoRoleStore()
        {
            BsonClassMap.RegisterClassMap<IdentityRole<string>>(cm =>
            {
                cm.AutoMap();

                cm.MapMember(x => x.Id).SetSerializer(new StringSerializer(BsonType.ObjectId));

                cm.UnmapMember(x => x.ConcurrencyStamp);
            });
        }

        public MongoRoleStore(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Identity_Roles";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<IdentityRole> collection, CancellationToken ct = default)
        {
            return collection.Indexes.CreateOneAsync(
                new CreateIndexModel<IdentityRole>(
                    Index
                        .Ascending(x => x.NormalizedName),
                    new CreateIndexOptions
                    {
                        Unique = true
                    }),
                cancellationToken: ct);
        }

        protected override MongoCollectionSettings CollectionSettings()
        {
            return new MongoCollectionSettings { WriteConcern = WriteConcern.WMajority };
        }

        public void Dispose()
        {
        }

        public IdentityRole Create(string name)
        {
            return new IdentityRole { Name = name };
        }

        public async Task<IdentityRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return await Collection.Find(x => x.Id == roleId).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IdentityRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return await Collection.Find(x => x.NormalizedName == normalizedRoleName).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            await Collection.InsertOneAsync(role, null, cancellationToken);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            await Collection.ReplaceOneAsync(x => x.Id == role.Id, role, null, cancellationToken);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            await Collection.DeleteOneAsync(x => x.Id == role.Id, null, cancellationToken);

            return IdentityResult.Success;
        }

        public Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id);
        }

        public Task<string> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        public Task<string> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName);
        }

        public Task SetRoleNameAsync(IdentityRole role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = roleName;

            return TaskHelper.Done;
        }

        public Task SetNormalizedRoleNameAsync(IdentityRole role, string normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = normalizedName;

            return TaskHelper.Done;
        }
    }
}