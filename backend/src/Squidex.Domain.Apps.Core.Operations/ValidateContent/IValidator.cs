// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public delegate void AddError(IEnumerable<string> path, string message);

    public interface IValidator
    {
        Task ValidateAsync(object? value, ValidationContext context, AddError addError);
    }
}
