// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Orleans;
using Orleans.Concurrency;

namespace Squidex.Domain.Apps.Entities.Apps.Plans
{
    public interface IUsageNotifierGrain : IGrainWithStringKey
    {
        [OneWay]
        Task NotifyAsync(UsageNotification notification);
    }
}
