// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Core.ValidateContent.Validators;

namespace Squidex.Domain.Apps.Core.ValidateContent;

public sealed class DefaultValidatorsFactory : IValidatorsFactory
{
    public IEnumerable<IValidator> CreateFieldValidators(ValidationContext context, IField field, ValidatorFactory factory)
    {
        if (field is IField<UIFieldProperties>)
        {
            yield return NoValueValidator.Instance;
        }

        if (field is IRootField rootField && field.RawProperties.IsCreateOnly && context.Root.PreviousData is ContentData previous)
        {
            yield return new NotChangedValidator(rootField, previous);
        }
    }

    public IEnumerable<IValidator> CreateValueValidators(ValidationContext context, IField field, ValidatorFactory factory)
    {
        return DefaultFieldValueValidatorsFactory.CreateValidators(context, field, factory);
    }
}
