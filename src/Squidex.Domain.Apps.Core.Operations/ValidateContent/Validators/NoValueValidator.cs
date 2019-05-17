// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class NoValueValidator : IValidator
    {
        public static readonly NoValueValidator Instance = new NoValueValidator();

        private NoValueValidator()
        {
        }

        public Task ValidateAsync(object value, ValidationContext context, AddError addError)
        {
            if (value != null)
            {
                addError(context.Path, "Field does not accept a value.");
            }

            return Task.CompletedTask;
        }
    }
}
