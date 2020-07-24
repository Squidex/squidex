// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;

namespace Squidex.Infrastructure.Translations
{
    public static class T
    {
        private static ITranslationService? translationService;

        public static void Setup(ITranslationService service)
        {
            translationService = service;
        }

        public static string Get(string key, object? args = null)
        {
            Guard.NotNullOrEmpty(key, nameof(key));

            if (translationService == null)
            {
                return key;
            }

            var (result, _) = translationService.Get(CultureInfo.CurrentUICulture, key, args);

            return result;
        }
    }
}
