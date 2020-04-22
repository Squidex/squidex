﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public sealed class DefaultValidatorsFactory : IValidatorsFactory
    {
        public IEnumerable<IValidator> CreateFieldValidators(ValidationContext context, IField field, FieldValidatorFactory createFieldValidator)
        {
            if (field is IField<UIFieldProperties>)
            {
                yield return NoValueValidator.Instance;
            }
        }

        public IEnumerable<IValidator> CreateValueValidators(ValidationContext context, IField field, FieldValidatorFactory createFieldValidator)
        {
            return DefaultFieldValueValidatorsFactory.CreateValidators(field, createFieldValidator);
        }
    }
}
