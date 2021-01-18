// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Infrastructure.Translations;
using Squidex.Text;

namespace Squidex.Infrastructure.Validation
{
    public sealed class LocalizedStringLengthAttribute : StringLengthAttribute
    {
        public LocalizedStringLengthAttribute(int maximumLength)
            : base(maximumLength)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            var property = T.Get($"common.{name.ToCamelCase()}", name);

            var min = MinimumLength;
            var max = MaximumLength;

            var args = new { name = property, min, max };

            if (min > 0)
            {
                return T.Get("annotations_StringLengthMinimum", base.FormatErrorMessage(name), args);
            }

            return T.Get("annotations_StringLength", base.FormatErrorMessage(name), args);
        }
    }
}
