// ==========================================================================
//  IAppState.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Rules;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Domain.Apps.Read
{
    public interface IAppState
    {
        Task<IAppEntity> GetAppAsync(Guid appId);

        Task<ISchemaEntity> GetSchemaAsync(Guid appId, Guid id, bool provideDeleted = false);

        Task<ISchemaEntity> GetSchemaAsync(Guid appId, string name, bool provideDeleted = false);

        Task<List<ISchemaEntity>> GetSchemasAsync(Guid appId);

        Task<List<IRuleEntity>> GetRulesAsync(Guid appId);

        Task<List<IAppEntity>> GetUserApps(string userId);
    }
}
