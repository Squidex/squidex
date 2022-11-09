// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using Microsoft.Extensions.Localization;
using Squidex.Infrastructure.Translations;
using Squidex.Text;

namespace Squidex.Web.Services;

public sealed class StringLocalizer : IStringLocalizer, IStringLocalizerFactory
{
    private readonly ILocalizer translationService;
    private readonly CultureInfo? culture;

    public LocalizedString this[string name]
    {
        get => this[name, null!];
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

            var (result, found) = translationService.Get(currentCulture, $"dotnet_{name}", name);

            if (arguments != null && found)
            {
                try
                {
                    result = string.Format(currentCulture, result, arguments);
                }
                catch (FormatException)
                {
                    return new LocalizedString(name, name, true);
                }
            }

            return new LocalizedString(name, result, !found);
        }
    }

    private void TranslateProperty(string name, object[] arguments, CultureInfo currentCulture)
    {
        if (arguments?.Length == 1 && IsValidationError(name))
        {
            var key = $"common.{arguments[0].ToString()?.ToCamelCase()}";

            var (result, found) = translationService.Get(currentCulture, key, name);

            if (found)
            {
                arguments[0] = result;
            }
        }
    }

    public StringLocalizer(ILocalizer translationService)
        : this(translationService, null)
    {
    }

    private StringLocalizer(ILocalizer translationService, CultureInfo? culture)
    {
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
        return name.Contains("annotations_", StringComparison.OrdinalIgnoreCase);
    }
}
