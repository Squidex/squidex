// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure.Translations;
using Squidex.Log;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class AggregateValidator : IValidator
    {
        private readonly IValidator[]? validators;
        private readonly ISemanticLog log;

        public AggregateValidator(IEnumerable<IValidator>? validators, ISemanticLog log)
        {
            this.validators = validators?.ToArray();

            this.log = log;
        }

        public async Task ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            try
            {
                if (validators?.Length > 0)
                {
                    await Task.WhenAll(validators.Select(x => x.ValidateAsync(value, context, addError)));
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, w => w
                    .WriteProperty("action", "validateField")
                    .WriteProperty("status", "Failed"));

                addError(context.Path, T.Get("contents.validation.error"));
            }
        }
    }
}
