// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Schemas;

#pragma warning disable MA0048 // File name must match type name

namespace Squidex.Domain.Apps.Core.ValidateContent;

public delegate IValidator ValidatorFactory(IField field);

public interface IValidatorsFactory
{
    IEnumerable<IValidator> CreateFieldValidators(ValidationContext context, IField field, ValidatorFactory factory)
    {
        yield break;
    }

    IEnumerable<IValidator> CreateValueValidators(ValidationContext context, IField field, ValidatorFactory factory)
    {
        yield break;
    }

    IEnumerable<IValidator> CreateContentValidators(ValidationContext context, ValidatorFactory factory)
    {
        yield break;
    }
}
