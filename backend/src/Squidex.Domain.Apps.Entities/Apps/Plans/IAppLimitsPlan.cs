// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Apps.Plans
{
    public interface IAppLimitsPlan
    {
        string Id { get; }

        string Name { get; }

        string Costs { get; }

        string? ConfirmText { get; }

        string? YearlyCosts { get; }

        string? YearlyId { get; }

        string? YearlyConfirmText { get; }

        long BlockingApiCalls { get; }

        long MaxApiCalls { get; }

        long MaxApiBytes { get; }

        long MaxAssetSize { get; }

        int MaxContributors { get; }
    }
}