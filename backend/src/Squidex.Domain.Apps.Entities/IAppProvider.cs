// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Rules;
using Squidex.Domain.Apps.Entities.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Entities
{
    public interface IAppProvider
    {
        Task<(IAppEntity?, ISchemaEntity?)> GetAppWithSchemaAsync(DomainId appId, DomainId id, bool canCache = false);

        Task<IAppEntity?> GetAppAsync(DomainId appId, bool canCache = false);

        Task<IAppEntity?> GetAppAsync(string appName, bool canCache = false);

        Task<List<IAppEntity>> GetUserAppsAsync(string userId, PermissionSet permissions);

        Task<ISchemaEntity?> GetSchemaAsync(DomainId appId, DomainId id, bool canCache = false);

        Task<ISchemaEntity?> GetSchemaAsync(DomainId appId, string name, bool canCache = false);

        Task<List<ISchemaEntity>> GetSchemasAsync(DomainId appId);

        Task<List<IRuleEntity>> GetRulesAsync(DomainId appId);

        Task<IRuleEntity?> GetRuleAsync(DomainId appId, DomainId id);
    }
}
