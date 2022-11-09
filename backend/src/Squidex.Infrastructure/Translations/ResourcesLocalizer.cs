// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Globalization;
using System.Resources;
using System.Text;

namespace Squidex.Infrastructure.Translations;

public sealed class ResourcesLocalizer : ILocalizer
{
#if DEBUG
    private static readonly MissingKeys MissingKeys = new MissingKeys();
#endif
    private readonly ResourceManager resourceManager;

    public ResourcesLocalizer(ResourceManager resourceManager)
    {
        this.resourceManager = resourceManager;
    }

    public (string Result, bool Found) Get(CultureInfo culture, string key, string fallback, object? args = null)
    {
        Guard.NotNull(culture);
        Guard.NotNullOrEmpty(key);
        Guard.NotNull(fallback);

        var translation = GetCore(culture, key);

        if (translation == null)
        {
            return (fallback, false);
        }

        if (args != null)
        {
            var argsType = args.GetType();

            var sb = new StringBuilder(translation.Length);

            var span = translation.AsSpan();

            while (span.Length > 0)
            {
                var indexOfStart = span.IndexOf('{');

                if (indexOfStart < 0)
                {
                    break;
                }

                indexOfStart++;

                var indexOfEnd = span[indexOfStart..].IndexOf('}');

                if (indexOfEnd < 0)
                {
                    break;
                }

                indexOfEnd += indexOfStart;

                sb.Append(span[.. (indexOfStart - 1)]);

                var variable = span[indexOfStart..indexOfEnd];

                var shouldLower = false;
                var shouldUpper = false;

                if (variable.Length > 0)
                {
                    if (variable.EndsWith("|lower", StringComparison.OrdinalIgnoreCase))
                    {
                        variable = variable[..^6];
                        shouldLower = true;
                    }

                    if (variable.EndsWith("|upper", StringComparison.OrdinalIgnoreCase))
                    {
                        variable = variable[..^6];
                        shouldUpper = true;
                    }
                }

                var variableName = variable.ToString();
                var variableValue = variableName;

                var property = argsType.GetProperty(variableName);

                if (property != null)
                {
                    try
                    {
                        var value = property.GetValue(args);

                        if (value != null)
                        {
                            variableValue = Convert.ToString(value, culture) ?? variableName;
                        }
                    }
                    catch
                    {
                        variableValue = variableName;
                    }
                }

                variableValue ??= variableName;

                if (variableValue!.Length > 0)
                {
                    if (shouldLower && !char.IsLower(variableValue[0]))
                    {
                        sb.Append(char.ToLower(variableValue[0], CultureInfo.InvariantCulture));

                        sb.Append(variableValue.AsSpan()[1..]);
                    }
                    else if (shouldUpper && !char.IsUpper(variableValue[0]))
                    {
                        sb.Append(char.ToUpper(variableValue[0], CultureInfo.InvariantCulture));

                        sb.Append(variableValue.AsSpan()[1..]);
                    }
                    else
                    {
                        sb.Append(variableValue);
                    }
                }

                span = span[(indexOfEnd + 1)..];
            }

            sb.Append(span);

            return (sb.ToString(), true);
        }

        return (translation, true);
    }

    private string? GetCore(CultureInfo culture, string key)
    {
        var translation = resourceManager.GetString(key, culture);
#if DEBUG
        if (translation == null)
        {
            MissingKeys.Log(key);
        }
#endif
        return translation;
    }
}
