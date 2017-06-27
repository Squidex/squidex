// ==========================================================================
//  IValidator.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Core.Schemas.Validators
{
    public interface IValidator
    {
        Task ValidateAsync(object value, ValidationContext context, Action<string> addError);
    }
}
