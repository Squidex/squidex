// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace Squidex.Infrastructure
{
    public sealed class AbsoluteUrlAttribute : ValidationAttribute
    {
        public AbsoluteUrlAttribute()
            : base(() => "The {0} field must be an absolute URL.")
        {
        }

        public override bool IsValid(object value)
        {
            if (value is Uri uri && !uri.IsAbsoluteUri)
            {
                return false;
            }

            return true;
        }
    }
}
