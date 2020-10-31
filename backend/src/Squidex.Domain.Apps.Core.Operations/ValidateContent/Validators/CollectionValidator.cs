// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public sealed class CollectionValidator : CollectionValidatorBase, IValidator
    {
        public CollectionValidator(bool isRequired, int? minItems = null, int? maxItems = null)
            : base(isRequired, minItems, maxItems)
        {
        }

        public Task ValidateAsync(object? value, ValidationContext context, AddError addError)
        {
            ValidateRequired(value, context, addError);
            ValidateSize(value, context, addError);

            return Task.CompletedTask;
        }
    }
}