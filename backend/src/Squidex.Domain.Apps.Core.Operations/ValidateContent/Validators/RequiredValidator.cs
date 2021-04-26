// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using Squidex.Infrastructure.Translations;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public class RequiredValidator : IValidator
    {
        public Task ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            if (value.IsNullOrUndefined() && !context.IsOptional)
            {
                addError(context.Path, T.Get("contents.validation.required"));
            }

            return Task.CompletedTask;
        }
    }
}