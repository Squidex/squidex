// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschr�nkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.ObjectModel;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Contents
{
    [Equals(DoNotAddEqualityOperators = true)]
    public sealed class NoUpdate : WorkflowCondition
    {
        public static readonly NoUpdate Always = new NoUpdate(null, null);

        public NoUpdate(string? expression, ReadOnlyCollection<string>? roles)
            : base(expression, roles)
        {
        }

        public static NoUpdate When(string? expression, params string[]? roles)
        {
            if (roles?.Length > 0)
            {
                return new NoUpdate(expression, ReadOnlyCollection.Create(roles));
            }
            else if (!string.IsNullOrWhiteSpace(expression))
            {
                return new NoUpdate(expression, null);
            }
            else
            {
                return Always;
            }
        }
    }
}