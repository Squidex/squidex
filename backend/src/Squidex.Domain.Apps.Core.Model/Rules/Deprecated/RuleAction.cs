// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Flows;

namespace Squidex.Domain.Apps.Core.Rules.Deprecated;

[Obsolete("Has been replaced by flows.")]
public abstract record RuleAction
{
    public abstract FlowStep ToFlowStep();
}
