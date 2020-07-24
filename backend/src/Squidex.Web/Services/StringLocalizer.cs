// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Localization;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Translations;

namespace Squidex.Web.Services
{
    public sealed class StringLocalizer : IStringLocalizer, IStringLocalizerFactory
    {
        private readonly ITranslationService translationService;
        private readonly CultureInfo? culture;

        public LocalizedString this[string name]
        {
            get { return this[name, null!]; }
        }

        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return new LocalizedString(name, name, false);
                }

                var currentCulture = culture ?? CultureInfo.CurrentUICulture;

                TranslateProperty(name, arguments, currentCulture);

                var (result, notFound) = translationService.Get(currentCulture, $"aspnet_{name}");

                if (arguments != null && !notFound)
                {
                    result = string.Format(currentCulture, result, arguments);
                }

                return new LocalizedString(name, result, notFound);
            }
        }

        private void TranslateProperty(string name, object[] arguments, CultureInfo currentCulture)
        {
            if (arguments?.Length == 1 && IsValidationError(name))
            {
                var key = $"common.{arguments[0].ToString()?.ToCamelCase()}";

                var (result, notFound) = translationService.Get(currentCulture, key);

                if (!notFound)
                {
                    arguments[0] = result;
                }
            }
        }

        public StringLocalizer(ITranslationService translationService)
            : this(translationService, null)
        {
        }

        private StringLocalizer(ITranslationService translationService, CultureInfo? culture)
        {
            Guard.NotNull(translationService, nameof(translationService));

            this.translationService = translationService;

            this.culture = culture;
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
        {
            return Enumerable.Empty<LocalizedString>();
        }

        public IStringLocalizer WithCulture(CultureInfo culture)
        {
            return new StringLocalizer(translationService, culture);
        }

        public IStringLocalizer Create(string baseName, string location)
        {
            return this;
        }

        public IStringLocalizer Create(Type resourceSource)
        {
            return this;
        }

        private static bool IsValidationError(string name)
        {
            return name.Contains("Attribute_", StringComparison.OrdinalIgnoreCase);
        }
    }
}
