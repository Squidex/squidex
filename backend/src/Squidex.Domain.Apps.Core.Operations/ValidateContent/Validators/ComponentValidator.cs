// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Contents;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class ComponentValidator : IValidator
    {
        private readonly Func<string, IValidator?> validatorFactory;

        public ComponentValidator(Func<string, IValidator?> validatorFactory)
        {
            this.validatorFactory = validatorFactory;
        }

        public async Task ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            if (value is Component component)
            {
                var validator = validatorFactory(component.Type);

                if (validator != null)
                {
                    await validator.ValidateAsync(component.Data, context, addError);
                }
            }
        }
    }
}
