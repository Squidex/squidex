// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using Squidex.Infrastructure.Translations;

namespace Squidex.Infrastructure.Validation
{
    public sealed class AbsoluteUrlAttribute : ValidationAttribute
    {
        public override string FormatErrorMessage(string name)
        {
            return T.Get("validation.absoluteUrl", new { property = name });
        }

        public override bool IsValid(object value)
        {
            return !(value is Uri uri) || uri.IsAbsoluteUri;
        }
    }
}
