﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.ComponentModel.DataAnnotations;

namespace Squidex.Infrastructure.Translations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class LocalizedRequired : RequiredAttribute
    {
        public override string FormatErrorMessage(string name)
        {
            return T.Get("annotations_Required", new { name });
        }
    }
}
