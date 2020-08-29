// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;
using Squidex.Text;

namespace Squidex.Infrastructure.Translations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class LocalizedRequired : RequiredAttribute
    {
        public override string FormatErrorMessage(string name)
        {
            var property = T.Get($"common.{name.ToCamelCase()}", name);

            return T.Get("annotations_Required", new { property });
        }
    }
}
