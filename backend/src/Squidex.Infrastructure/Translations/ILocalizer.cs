// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;

namespace Squidex.Infrastructure.Translations
{
    public interface ILocalizer
    {
        (string Result, bool Found) Get(CultureInfo culture, string key, string fallback, object? args = null);
    }
}
