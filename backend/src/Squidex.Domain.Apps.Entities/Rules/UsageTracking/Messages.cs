// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Entities.Rules.UsageTracking;

public sealed record UsageTrackingAdd(DomainId RuleId, NamedId<DomainId> AppId, int Limits, int? NumDays)
    : UsageTrackingMessage(RuleId);

public sealed record UsageTrackingRemove(DomainId RuleId)
    : UsageTrackingMessage(RuleId);

public sealed record UsageTrackingUpdate(DomainId RuleId, int Limits, int? NumDays)
    : UsageTrackingMessage(RuleId);

public abstract record UsageTrackingMessage(DomainId RuleId);
