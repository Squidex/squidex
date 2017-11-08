// ==========================================================================
//  IActionVisitor.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Actions;

namespace Squidex.Domain.Apps.Core.Rules
{
    public interface IRuleActionVisitor<T>
    {
        T Visit(WebhookAction action);
    }
}
