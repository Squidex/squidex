// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Apps.Services
{
    public interface IAppLimitsPlan
    {
        string Id { get; }

        string Name { get; }

        string Costs { get; }

        string YearlyCosts { get; }

        string YearlyId { get; }

        long MaxApiCalls { get; }

        long MaxAssetSize { get; }

        int MaxContributors { get; }
    }
}