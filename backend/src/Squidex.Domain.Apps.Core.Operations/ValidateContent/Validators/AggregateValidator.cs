// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Logging;
using Squidex.Infrastructure.Tasks;
using Squidex.Infrastructure.Translations;
using Squidex.Log;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class AggregateValidator : IValidator
    {
        private readonly IValidator[]? validators;
        private readonly ILogger<ContentValidator> log;

        public AggregateValidator(IEnumerable<IValidator>? validators, ILogger<ContentValidator> log)
        {
            this.validators = validators?.ToArray();

            this.log = log;
        }

        public async ValueTask ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            try
            {
                if (validators?.Length > 0)
                {
                    await AsyncHelper.WhenAllThrottledAsync(validators, (x, _) => x.ValidateAsync(value, context, addError));
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to validate fields.");

                addError(context.Path, T.Get("contents.validation.error"));
            }
        }
    }
}
