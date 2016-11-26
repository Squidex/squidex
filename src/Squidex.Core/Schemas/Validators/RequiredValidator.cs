// ==========================================================================
//  RequiredValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Core.Schemas.Validators
{
    public class RequiredValidator : IValidator
    {
        public Task ValidateAsync(object value, ICollection<string> errors)
        {
            if (value == null)
            {
                errors.Add("<FIELD> is required");
            }

            return TaskHelper.Done;
        }
    }
}
