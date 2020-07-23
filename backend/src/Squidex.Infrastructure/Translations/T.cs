// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Globalization;
using System.IO;
using System.Resources;
using System.Text;

namespace Squidex.Infrastructure.Translations
{
    public static class T
    {
        private static readonly object LockObject = new object();
        private static ResourceManager? resources;

        public static void Setup(ResourceManager resourceManager)
        {
            resources = resourceManager;
        }

        public static string Get(string key, object? args = null)
        {
            Guard.NotNullOrEmpty(key, nameof(key));

            if (resources == null)
            {
                return key;
            }

            var translation = resources.GetString(key);

            if (translation == null)
            {
                return key;
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
                            variableValue = Convert.ToString(property.GetValue(args), CultureInfo.CurrentUICulture);
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
                        variableValue = Get(variableValue);
                    }

                    if (variableValue.Length > 0)
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

                return sb.ToString();
            }
            else
            {
#if DEBUG
                lock (LockObject)
                {
                    File.AppendAllLines("Missing.txt", new string[] { key });
                }
#endif
            }

            return translation;
        }
    }
}
