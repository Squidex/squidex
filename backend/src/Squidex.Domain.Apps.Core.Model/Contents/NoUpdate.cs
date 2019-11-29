// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschr�nkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Contents
{
    public sealed class NoUpdate : WorkflowCondition
    {
        public static readonly NoUpdate Always = new NoUpdate(null, null);

        public NoUpdate(string? expression, params string[]? roles)
            : base(expression, roles)
        {
        }

        public static NoUpdate When(string? expression, params string[]? roles)
        {
            return new NoUpdate(expression, roles);
        }
    }
}