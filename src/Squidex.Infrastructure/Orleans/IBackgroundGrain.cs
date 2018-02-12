// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;

namespace Squidex.Infrastructure.Orleans
{
    public interface IBackgroundGrain : IGrainWithStringKey
    {
        Task ActivateAsync();
    }
}
