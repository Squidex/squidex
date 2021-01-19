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
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class LocalizedRangeAttribute : RangeAttribute
    {
        public LocalizedRangeAttribute(int minimum, int maximum)
            : base(minimum, maximum)
        {
        }

        public LocalizedRangeAttribute(double minimum, double maximum)
            : base(minimum, maximum)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            var property = T.Get($"common.{name.ToCamelCase()}", name);

            var min = Minimum;
            var max = Maximum;

            return T.Get("annotations_Range", base.FormatErrorMessage(name), new { name = property, min, max });
        }
    }
}
