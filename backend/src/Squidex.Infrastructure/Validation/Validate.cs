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
            errors ??= new List<ValidationError>();
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
            errors ??= new List<ValidationError>();
            errors.Add(new ValidationError(m, p));
        });

        await action(addValidation);

        if (errors != null)
        {
            throw new ValidationException(errors);
        }
    }
}

public delegate void AddValidation(string message, params string[] propertyNames);
