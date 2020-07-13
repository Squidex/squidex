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
        Task<(IAppEntity?, ISchemaEntity?)> GetAppWithSchemaAsync(Guid appId, Guid id, bool canCache = false);

        Task<IAppEntity?> GetAppAsync(Guid appId, bool canCache = false);

        Task<IAppEntity?> GetAppAsync(string appName, bool canCache = false);

        Task<List<IAppEntity>> GetUserAppsAsync(string userId, PermissionSet permissions);

        Task<ISchemaEntity?> GetSchemaAsync(Guid appId, Guid id, bool allowDeleted, bool canCache = false);

        Task<ISchemaEntity?> GetSchemaAsync(Guid appId, string name, bool canCache = false);

        Task<List<ISchemaEntity>> GetSchemasAsync(Guid appId);

        Task<List<IRuleEntity>> GetRulesAsync(Guid appId);
    }
}
