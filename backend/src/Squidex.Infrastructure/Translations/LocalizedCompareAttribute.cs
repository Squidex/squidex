// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Infrastructure.Translations
{
    public sealed class LocalizedCompareAttribute : CompareAttribute
    {
        public LocalizedCompareAttribute(string otherProperty)
            : base(otherProperty)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return T.Get("annotations_Compare", new { name, other = OtherProperty });
        }
    }
}
