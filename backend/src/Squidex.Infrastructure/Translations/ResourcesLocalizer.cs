// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Text;

namespace Squidex.Infrastructure.Translations
{
    public sealed class ResourcesLocalizer : ILocalizer
    {
        private const string MissingFileName = "__missing.txt";

        private readonly ResourceManager resourceManager;
        private readonly HashSet<string> missingTranslations;
        private readonly object lockObject = new object();

        public ResourcesLocalizer(ResourceManager resourceManager)
        {
            Guard.NotNull(resourceManager, nameof(resourceManager));

            this.resourceManager = resourceManager;
#if DEBUG
            if (File.Exists(MissingFileName))
            {
                var missing = File.ReadAllLines(MissingFileName);

                missingTranslations = new HashSet<string>(missing);
            }
            else
            {
                missingTranslations = new HashSet<string>();
            }
#endif
        }

        public (string Result, bool NotFound) Get(CultureInfo culture, string key, object? args = null)
        {
            Guard.NotNull(culture, nameof(culture));
            Guard.NotNullOrEmpty(key, nameof(key));

            var translation = GetCore(culture, key);

            if (translation == null)
            {
                return (key, true);
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

                    var indexOfEnd = span.Slice(indexOfStart).IndexOf('}');

                    if (indexOfEnd < 0)
                    {
                        break;
                    }

                    indexOfEnd += indexOfStart;

                    sb.Append(span.Slice(0, indexOfStart - 1));

                    var variable = span[indexOfStart..indexOfEnd];

                    var shouldTranslate = false;
                    var shouldLower = false;
                    var shouldUpper = false;

                    if (variable.Length > 0)
                    {
                        if (variable.StartsWith("!"))
                        {
                            variable = variable.Slice(1);
                            shouldTranslate = true;
                        }

                        if (variable.EndsWith("|lower"))
                        {
                            variable = variable[0..^6];
                            shouldLower = true;
                        }

                        if (variable.EndsWith("|upper"))
                        {
                            variable = variable[0..^6];
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
                            variableValue = Convert.ToString(property.GetValue(args), culture);
                        }
                        catch
                        {
                            variableValue = null;
                        }
                    }

                    if (variableValue == null)
                    {
                        variableValue = variableName;
                    }

                    if (shouldTranslate)
                    {
                        var converted = variableValue.ToCamelCase();

                        variableValue = GetCore(culture, converted);

                        if (variableValue == null)
                        {
                            variableValue = GetCore(culture, $"common.{converted}");
                        }
                    }

                    variableValue ??= variableName;

                    if (variableValue!.Length > 0)
                    {
                        if (shouldLower && !char.IsLower(variableValue[0]))
                        {
                            sb.Append(char.ToLower(variableValue[0]));

                            sb.Append(variableValue.AsSpan().Slice(1));
                        }
                        else if (shouldUpper && !char.IsUpper(variableValue[0]))
                        {
                            sb.Append(char.ToUpper(variableValue[0]));

                            sb.Append(variableValue.AsSpan().Slice(1));
                        }
                        else
                        {
                            sb.Append(variableValue);
                        }
                    }

                    span = span.Slice(indexOfEnd + 1);
                }

                sb.Append(span);

                return (sb.ToString(), false);
            }

            return (translation, false);
        }

        private string? GetCore(CultureInfo culture, string key)
        {
            var translation = resourceManager.GetString(key, culture);

            if (translation == null)
            {
#if DEBUG
                lock (lockObject)
                {
                    if (!missingTranslations.Add(key))
                    {
                        File.AppendAllLines(MissingFileName, new string[] { key });
                    }
                }
#endif
            }

            return translation;
        }
    }
}
