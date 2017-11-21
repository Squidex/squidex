// ==========================================================================
//  IRuleDequeuerGrain.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;
using Orleans.Concurrency;

namespace Squidex.Domain.Apps.Read.Rules.Orleans.Grains
{
    public interface IRuleDequeuerGrain : IGrainWithStringKey
    {
        Task ActivateAsync();

        Task HandleAsync(Immutable<IRuleEventEntity> @event);
    }
}
