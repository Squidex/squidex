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
    public delegate IValidator ValidatorFactory(IField field);

    public interface IValidatorsFactory
    {
        IEnumerable<IValidator> CreateFieldValidators(ValidatorContext context, IField field, ValidatorFactory factory)
        {
            yield break;
        }

        IEnumerable<IValidator> CreateValueValidators(ValidatorContext context, IField field, ValidatorFactory factory)
        {
            yield break;
        }

        IEnumerable<IValidator> CreateContentValidators(ValidatorContext context, ValidatorFactory factory)
        {
            yield break;
        }
    }
}
