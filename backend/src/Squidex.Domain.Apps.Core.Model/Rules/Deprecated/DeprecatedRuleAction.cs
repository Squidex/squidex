// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Flows;

namespace Squidex.Domain.Apps.Core.Rules.Deprecated;

public abstract record DeprecatedRuleAction
{
    public abstract FlowStep ToFlowStep();
}
