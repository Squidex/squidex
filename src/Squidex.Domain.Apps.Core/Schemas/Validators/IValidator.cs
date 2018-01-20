// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Core.Schemas.Validators
{
    public interface IValidator
    {
        Task ValidateAsync(object value, ValidationContext context, Action<string> addError);
    }
}
