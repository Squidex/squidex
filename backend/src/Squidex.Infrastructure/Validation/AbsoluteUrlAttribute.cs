// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using Squidex.Infrastructure.Translations;
using Squidex.Text;

namespace Squidex.Infrastructure.Validation
{
    public sealed class AbsoluteUrlAttribute : ValidationAttribute
    {
        public override string FormatErrorMessage(string name)
        {
            var property = T.Get($"common.{name.ToCamelCase()}", name);

            return T.Get("dotnet_annotations_AbsoluteUrl", new { property });
        }

        public override bool IsValid(object value)
        {
            return !(value is Uri uri) || uri.IsAbsoluteUri;
        }
    }
}
