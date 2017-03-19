// ==========================================================================
//  RequiredValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure.Tasks;

namespace Squidex.Core.Schemas.Validators
{
    public class RequiredValidator : IValidator
    {
        public Task ValidateAsync(object value, Action<string> addError)
        {
            if (value == null)
            {
                addError("<FIELD> is required");
            }

            return TaskHelper.Done;
        }
    }
}
