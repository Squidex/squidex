﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Squidex.Domain.Apps.Core.ValidateContent.Validators
{
    public delegate void AddError(IEnumerable<string> path, string message);

    public interface IValidator
    {
        Task ValidateAsync(object? value, ValidationContext context, AddError addError);
    }
}
