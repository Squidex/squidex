// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Entities.Rules.Runner;

public sealed record RuleRunnerRun(DomainId AppId, DomainId RuleId, bool FromSnapshots);

public sealed record RuleRunnerCancel(DomainId AppId);
