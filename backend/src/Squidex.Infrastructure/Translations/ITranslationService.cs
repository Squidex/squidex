// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;

namespace Squidex.Infrastructure.Translations
{
    public interface ITranslationService
    {
        (string Result, bool NotFound) Get(CultureInfo culture, string key, object? args = null);
    }
}
