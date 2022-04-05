// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public delegate void AddError(IEnumerable<string> path, string message);

    public interface IValidator
    {
        ValueTask ValidateAsync(object? value, ValidationContext context, AddError addError);
    }
}
