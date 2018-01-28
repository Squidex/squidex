// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Rules.Actions;

namespace Squidex.Domain.Apps.Core.Rules
{
    public interface IRuleActionVisitor<out T>
    {
        T Visit(AlgoliaAction action);

        T Visit(WebhookAction action);
    }
}
