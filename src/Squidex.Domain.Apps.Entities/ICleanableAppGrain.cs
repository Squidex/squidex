// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Orleans;

namespace Squidex.Domain.Apps.Entities
{
    public interface ICleanableAppGrain : IGrainWithGuidKey
    {
        Task ClearAsync();
    }
}
