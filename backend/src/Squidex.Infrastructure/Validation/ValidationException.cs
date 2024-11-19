// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;

namespace Squidex.Infrastructure.Validation;

[Serializable]
public class ValidationException(IReadOnlyList<ValidationError> errors, Exception? inner = null) : DomainException(FormatMessage(errors), ValidationError, inner)
{
    private const string ValidationError = "VALIDATION_ERROR";

    public IReadOnlyList<ValidationError> Errors { get; } = errors;

    public ValidationException(string error, Exception? inner = null)
        : this(new ValidationError(error), inner)
    {
    }

    public ValidationException(ValidationError error, Exception? inner = null)
        : this([error], inner)
    {
    }

    private static string FormatMessage(IReadOnlyList<ValidationError> errors)
    {
        Guard.NotNull(errors);

        var sb = new StringBuilder();

        for (var i = 0; i < errors.Count; i++)
        {
            var error = errors[i]?.Message;

            if (!string.IsNullOrWhiteSpace(error))
            {
                sb.Append(error);

                if (!error.EndsWith(".", StringComparison.OrdinalIgnoreCase))
                {
                    sb.Append('.');
                }

                if (i < errors.Count - 1)
                {
                    sb.Append(' ');
                }
            }
        }

        return sb.ToString();
    }
}
