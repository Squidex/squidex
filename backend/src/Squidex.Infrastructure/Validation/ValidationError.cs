// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.Validation;

[Serializable]
public sealed class ValidationError
{
    private readonly string message;
    private readonly string[] propertyNames;

    public string Message
    {
        get => message;
    }

    public IEnumerable<string> PropertyNames
    {
        get => propertyNames;
    }

    public ValidationError(string message, params string[] propertyNames)
    {
        Guard.NotNullOrEmpty(message);

        this.message = message;

        this.propertyNames = propertyNames ?? Array.Empty<string>();
    }

    public ValidationError WithPrefix(string prefix)
    {
        if (propertyNames.Length > 0)
        {
            return new ValidationError(Message, propertyNames.Select(x => $"{prefix}.{x}").ToArray());
        }
        else
        {
            return new ValidationError(Message, prefix);
        }
    }

    public void AddTo(AddValidation e)
    {
        e(Message, propertyNames);
    }
}
