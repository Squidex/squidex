// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

namespace Squidex.Domain.Apps.Core.HandleRules;

public sealed class CronJobContext
{
    required public NamedId<DomainId> AppId { get; set; }

    required public DomainId RuleId { get; set; }

    required public JsonValue Value { get; set; }
}
