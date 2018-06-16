// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Squidex.Infrastructure
{
    public delegate void AddValidation(string message, params string[] propertyNames);

    public static class Validate
    {
        public static void It(Func<string> message, Action<AddValidation> action)
        {
            List<ValidationError> errors = null;

            var addValidation = new AddValidation((m, p) =>
            {
                if (errors == null)
                {
                    errors = new List<ValidationError>();
                }

                errors.Add(new ValidationError(m, p));
            });

            action(addValidation);

            if (errors != null)
            {
                throw new ValidationException(message(), errors);
            }
        }

        public static async Task It(Func<string> message, Func<AddValidation, Task> action)
        {
            List<ValidationError> errors = null;

            var addValidation = new AddValidation((m, p) =>
            {
                if (errors == null)
                {
                    errors = new List<ValidationError>();
                }

                errors.Add(new ValidationError(m, p));
            });

            await action(addValidation);

            if (errors != null)
            {
                throw new ValidationException(message(), errors);
            }
        }
    }
}
