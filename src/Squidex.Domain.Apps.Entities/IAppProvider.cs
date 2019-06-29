// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Entities
{
    public interface IAppProvider
    {
        Task<(IAppEntity, ISchemaEntity)> GetAppWithSchemaAsync(Guid appId, Guid id);

        Task<IAppEntity> GetAppAsync(Guid appId);

        Task<IAppEntity> GetAppAsync(string appName);

        Task<ISchemaEntity> GetSchemaAsync(Guid appId, Guid id, bool allowDeleted = false);

        Task<ISchemaEntity> GetSchemaAsync(Guid appId, string name);

        Task<List<ISchemaEntity>> GetSchemasAsync(Guid appId);

        Task<List<IRuleEntity>> GetRulesAsync(Guid appId);

        Task<List<IAppEntity>> GetUserApps(string userId, PermissionSet permissions);
    }
}
