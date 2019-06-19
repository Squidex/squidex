// ==========================================================================
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
    public static class Extensions
    {
        public static FieldValidator CreateValidator(this IField field)
        {
            return new FieldValidator(CreateValueValidators(field), field);
        }

        private static IEnumerable<IValidator> CreateValueValidators(IField field)
        {
            return FieldValueValidatorsFactory.CreateValidators(field);
        }

        public static IEnumerable<IValidator> CreateBagValidator(this IField field)
        {
            return FieldBagValidatorsFactory.CreateValidators(field);
        }
    }
}
