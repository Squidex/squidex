// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Domain.Apps.Core.Schemas;

namespace Squidex.Domain.Apps.Core.ValidateContent
{
    public delegate IValidator FieldValidatorFactory(IField field);

    public interface IValidatorsFactory
    {
        IEnumerable<IValidator> CreateFieldValidators(ValidationContext context, IField field, FieldValidatorFactory createFieldValidator)
        {
            yield break;
        }

        IEnumerable<IValidator> CreateValueValidators(ValidationContext context, IField field, FieldValidatorFactory createFieldValidator)
        {
            yield break;
        }

        IEnumerable<IValidator> CreateContentValidators(ValidationContext context, FieldValidatorFactory createFieldValidator)
        {
            yield break;
        }
    }
}
