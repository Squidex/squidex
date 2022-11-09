// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Billing;

public sealed record Plan
{
    public string Id { get; init; }

    public string Name { get; init; }

    public string Costs { get; init; }

    public string? ConfirmText { get; init; }

    public string? YearlyCosts { get; init; }

    public string? YearlyId { get; init; }

    public string? YearlyConfirmText { get; init; }

    public long BlockingApiCalls { get; init; }

    public long MaxApiCalls { get; init; }

    public long MaxApiBytes { get; init; }

    public long MaxAssetSize { get; init; }

    public long MaxContributors { get; init; }

    public bool IsFree { get; init; }
}
