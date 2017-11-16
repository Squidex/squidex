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
using Squidex.Infrastructure.CQRS.Events;
using Squidex.Infrastructure.Json.Orleans;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains
{
    public interface IAppStateGrain : IGrainWithStringKey
    {
        Task<J<(IAppEntity, ISchemaEntity)>> GetAppWithSchemaAsync(Guid id);

        Task<J<IAppEntity>> GetAppAsync();

        Task<J<ISchemaEntity>> GetSchemaAsync(Guid id, bool provideDeleted = false);

        Task<J<ISchemaEntity>> GetSchemaAsync(string name, bool provideDeleted = false);

        Task<J<List<ISchemaEntity>>> GetSchemasAsync();

        Task<J<List<IRuleEntity>>> GetRulesAsync();

        Task HandleAsync(J<Envelope<IEvent>> message);
    }
}
