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
using Orleans.Concurrency;
using Squidex.Domain.Apps.Read.Apps;
using Squidex.Domain.Apps.Read.Rules;
using Squidex.Domain.Apps.Read.Schemas;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains
{
    public interface IAppStateGrain : IGrainWithStringKey
    {
        Task<Immutable<(IAppEntity, ISchemaEntity)>> GetAppWithSchemaAsync(Guid id);

        Task<Immutable<IAppEntity>> GetAppAsync();

        Task<Immutable<ISchemaEntity>> GetSchemaAsync(Guid id, bool provideDeleted = false);

        Task<Immutable<ISchemaEntity>> GetSchemaAsync(string name, bool provideDeleted = false);

        Task<Immutable<List<ISchemaEntity>>> GetSchemasAsync();

        Task<Immutable<List<IRuleEntity>>> GetRulesAsync();

        Task HandleAsync(Immutable<Envelope<IEvent>> message);
    }
}
