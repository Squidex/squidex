// ==========================================================================
//  IRuleActionValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules.Actions
{
    public interface IRuleActionValidator
    {
        IList<ValidationError> Validate(RuleAction ruleAction);
    }
}