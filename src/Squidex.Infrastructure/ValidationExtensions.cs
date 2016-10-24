// ==========================================================================
//  ValidationExtensions.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace Squidex.Infrastructure
{
    public static class ValidationExtensions
    {
        public static void Validate(this IValidatable target, Func<string> message)
        {
            var errors = new List<ValidationError>();

            target.Validate(errors);

            if (errors.Any())
            {
                throw new ValidationException(message(), errors);
            }
        }
    }
}
