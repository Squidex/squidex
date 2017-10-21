// ==========================================================================
//  TypeNameRegistry.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Squidex.Infrastructure
{
    public static class Validate
    {
        public static void It(Func<string> message, Action<Action<ValidationError>> action)
        {
            var errors = new List<ValidationError>();

            action(errors.Add);

            if (errors.Any())
            {
                throw new ValidationException(message(), errors);
            }
        }

        public static async Task It(Func<string> message, Func<Action<ValidationError>, Task> action)
        {
            var errors = new List<ValidationError>();

            await action(errors.Add);

            if (errors.Any())
            {
                throw new ValidationException(message(), errors);
            }
        }
    }
}
