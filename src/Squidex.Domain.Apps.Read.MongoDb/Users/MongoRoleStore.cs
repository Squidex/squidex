// ==========================================================================
//  MongoRoleStore.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.MongoDB;
using MongoDB.Driver;
using Squidex.Domain.Apps.Read.Users;

namespace Squidex.Domain.Apps.Read.MongoDb.Users
{
    public sealed class MongoRoleStore : 
        IRoleStore<IRole>, 
        IRoleFactory
    {
        private readonly RoleStore<WrappedIdentityRole> innerStore;

        public MongoRoleStore(IMongoDatabase database)
        {
            var rolesCollection = database.GetCollection<WrappedIdentityRole>("Identity_Roles");

            IndexChecks.EnsureUniqueIndexOnNormalizedRoleName(rolesCollection);

            innerStore = new RoleStore<WrappedIdentityRole>(rolesCollection);
        }

        public void Dispose()
        {
            innerStore.Dispose();
        }

        public IRole Create(string name)
        {
            return new WrappedIdentityRole { Name = name };
        }

        public async Task<IRole> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return await innerStore.FindByIdAsync(roleId, cancellationToken);
        }

        public async Task<IRole> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return await innerStore.FindByNameAsync(normalizedRoleName, cancellationToken);
        }

        public Task<IdentityResult> CreateAsync(IRole role, CancellationToken cancellationToken)
        {
            return innerStore.CreateAsync((WrappedIdentityRole)role, cancellationToken);
        }

        public Task<IdentityResult> UpdateAsync(IRole role, CancellationToken cancellationToken)
        {
            return innerStore.UpdateAsync((WrappedIdentityRole)role, cancellationToken);
        }

        public Task<IdentityResult> DeleteAsync(IRole role, CancellationToken cancellationToken)
        {
            return innerStore.DeleteAsync((WrappedIdentityRole)role, cancellationToken);
        }

        public Task<string> GetRoleIdAsync(IRole role, CancellationToken cancellationToken)
        {
            return innerStore.GetRoleIdAsync((WrappedIdentityRole)role, cancellationToken);
        }

        public Task<string> GetRoleNameAsync(IRole role, CancellationToken cancellationToken)
        {
            return innerStore.GetRoleNameAsync((WrappedIdentityRole)role, cancellationToken);
        }

        public Task SetRoleNameAsync(IRole role, string roleName, CancellationToken cancellationToken)
        {
            return innerStore.SetRoleNameAsync((WrappedIdentityRole)role, roleName, cancellationToken);
        }

        public Task<string> GetNormalizedRoleNameAsync(IRole role, CancellationToken cancellationToken)
        {
            return innerStore.GetNormalizedRoleNameAsync((WrappedIdentityRole)role, cancellationToken);
        }

        public Task SetNormalizedRoleNameAsync(IRole role, string normalizedName, CancellationToken cancellationToken)
        {
            return innerStore.SetNormalizedRoleNameAsync((WrappedIdentityRole)role, normalizedName, cancellationToken);
        }
    }
}
