// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Infrastructure.Validation;

public static class Validate
{
    public static void It(Action<AddValidation> action)
    {
        List<ValidationError>? errors = null;

        var addValidation = new AddValidation((m, p) =>
        {
            errors ??= [];
            errors.Add(new ValidationError(m, p));
        });

        action(addValidation);

        if (errors != null)
        {
            throw new ValidationException(errors);
        }
    }

    public static async Task It(Func<AddValidation, Task> action)
    {
        List<ValidationError>? errors = null;

        var addValidation = new AddValidation((m, p) =>
        {
            errors ??= [];
            errors.Add(new ValidationError(m, p));
        });

        await action(addValidation);

        if (errors != null)
        {
            throw new ValidationException(errors);
        }
    }

    public static AddValidation WithPrefix(string prefix, AddValidation inner)
    {
        return (message, propertyNames) =>
        {
            if (propertyNames.Length == 0)
            {
                inner(message, propertyNames);
            }

            var withPrefix = propertyNames.Select(path =>
            {
                if (!string.IsNullOrWhiteSpace(path))
                {
                    return $"{prefix}.{path}";
                }

                return path;
            }).ToArray();

            inner(message, withPrefix);
        };
    }
}

public delegate void AddValidation(string message, params string[] propertyNames);
