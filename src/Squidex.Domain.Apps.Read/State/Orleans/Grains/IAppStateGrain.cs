// ==========================================================================
//  IAppStateGrain.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Orleans;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Rules;
using Squidex.Domain.Apps.Read.Schemas;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains
{
    public interface IAppStateGrain : IGrainWithGuidKey
    {
        Task<IAppEntity> GetAppAsync();

        Task<ISchemaEntity> GetSchemaAsync(Guid id, bool provideDeleted = false);

        Task<ISchemaEntity> GetSchemaAsync(string name, bool provideDeleted = false);

        Task<List<ISchemaEntity>> GetSchemasAsync();

        Task<List<IRuleEntity>> GetRulesAsync();

        Task HandleAsync(EventMessage message);
    }
}
