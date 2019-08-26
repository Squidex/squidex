// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans.Concurrency;

namespace Squidex.Infrastructure.Orleans
{
    public interface IDeactivatableGrain
    {
        [AlwaysInterleave]
        [OneWay]
        Task DeactivateAsync();
    }
}
