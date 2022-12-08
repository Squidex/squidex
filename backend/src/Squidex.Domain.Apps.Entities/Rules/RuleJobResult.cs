// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities.Rules;

public enum RuleJobResult
{
    Pending,
    Success,
    Retry,
    Failed,
    Cancelled
}
