// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Rules
{
    public abstract class RuleAction : Freezable
    {
        public IEnumerable<ValidationError> Validate()
        {
            var context = new ValidationContext(this);
            var errors = new List<ValidationResult>();

            if (!Validator.TryValidateObject(this, context, errors, true))
            {
                foreach (var error in errors)
                {
                    yield return new ValidationError(error.ErrorMessage, error.MemberNames.ToArray());
                }
            }

            foreach (var error in CustomValidate())
            {
                yield return error;
            }
        }

        protected virtual IEnumerable<ValidationError> CustomValidate()
        {
            yield break;
        }
    }
}
