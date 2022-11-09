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

[AttributeUsage(AttributeTargets.Property)]
public class LocalizedRequiredAttribute : RequiredAttribute
{
    public override string FormatErrorMessage(string name)
    {
        var property = T.Get($"common.{name.ToCamelCase()}", name);

        return T.Get("annotations_Required", base.FormatErrorMessage(name), new { name = property });
    }
}
