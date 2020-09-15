﻿// ==========================================================================
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
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class LocalizedEmailAddressAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            return !(value is string s) || s.IsEmail();
        }

        public override string FormatErrorMessage(string name)
        {
            var property = T.Get($"common.{name.ToCamelCase()}", name);

            return T.Get("annotations_EmailAddress", base.FormatErrorMessage(name), new { property });
        }
    }
}
