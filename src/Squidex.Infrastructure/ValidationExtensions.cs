// ==========================================================================
//  ValidationExtensions.cs
//  PinkParrot Headless CMS
// ==========================================================================
//  Copyright (c) PinkParrot Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace PinkParrot.Infrastructure
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
