// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;
using Squidex.Infrastructure.Translations;
using Squidex.Text;

namespace Squidex.Infrastructure.Validation;

public sealed class LocalizedCompareAttribute : CompareAttribute
{
    public LocalizedCompareAttribute(string otherProperty)
        : base(otherProperty)
    {
    }

    public override string FormatErrorMessage(string name)
    {
        var property = T.Get($"common.{name.ToCamelCase()}", name);

        var other = T.Get($"common.{OtherProperty.ToCamelCase()}", OtherProperty);

        return T.Get("annotations_Compare", base.FormatErrorMessage(name), new { name = property, other });
    }
}
