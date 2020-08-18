// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Squidex.Translator.State.Old;

namespace Squidex.Translator.State
{
    public sealed class TranslationService
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        private readonly Dictionary<string, TranslatedTexts> translations = new Dictionary<string, TranslatedTexts>();
        private readonly TranslationTodos translationsTodo;
        private readonly TranslationsToIgnore translationToIgnore;
        private readonly DirectoryInfo sourceDirectory;
        private readonly string sourceFileName;
        private readonly string[] supportedLocales;
        private readonly bool onlySingleWords;
        private string previousPrefix;

        public TranslatedTexts MainTranslations
        {
            get { return translations[MainLocale]; }
        }

        public string MainLocale
        {
            get { return supportedLocales[0]; }
        }

        public IEnumerable<string> SupportedLocales
        {
            get { return supportedLocales; }
        }

        public IEnumerable<string> NonMainSupportedLocales
        {
            get { return supportedLocales.Skip(1); }
        }

        public IReadOnlyDictionary<string, TranslatedTexts> Translations
        {
            get { return translations; }
        }

        static TranslationService()
        {
            SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public TranslationService(DirectoryInfo sourceDirectory, string sourceFileName, string[] supportedLocales, bool onlySingleWords)
        {
            this.onlySingleWords = onlySingleWords;

            this.sourceDirectory = sourceDirectory;
            this.sourceFileName = sourceFileName;

            this.supportedLocales = supportedLocales;

            foreach (var locale in supportedLocales)
            {
                translations[locale] = Load<TranslatedTexts>($"_{locale}.json");
            }

            translationsTodo = Load<TranslationTodos>("__todos.json");
            translationToIgnore = Load<TranslationsToIgnore>("__ignore.json");
        }

        public TranslatedTexts GetTextsWithFallback(string locale)
        {
            var result = new TranslatedTexts(MainTranslations);

            if (translations.TryGetValue(locale, out var translated))
            {
                foreach (var key in result.Keys.ToList())
                {
                    if (translated.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                    {
                        result[key] = value;
                    }
                }
            }

            return result;
        }

        private T Load<T>(string name) where T : new()
        {
            var fullName = GetFullName(name);

            if (File.Exists(fullName))
            {
                var json = File.ReadAllText(fullName);

                return JsonSerializer.Deserialize<T>(json, SerializerOptions);
            }
            else
            {
                return new T();
            }
        }

        private void Save<T>(string name, T value) where T : new()
        {
            var fullName = GetFullName(name);

            WriteTo(value, fullName);
        }

        private string GetFullName(string name)
        {
            return Path.Combine(sourceDirectory.FullName, "source", $"{sourceFileName}{name}");
        }

        public void WriteTo<T>(T value, string path) where T : new()
        {
            var json = JsonSerializer.Serialize(value, SerializerOptions);

            if (!sourceDirectory.Exists)
            {
                Directory.CreateDirectory(sourceDirectory.FullName);
            }

            File.WriteAllText(path, json);
        }

        public void Migrate()
        {
            var oldState = Load<OldTranslationState>(".json");

            foreach (var (key, value) in oldState.Texts)
            {
                if (value.Texts.TryGetValue("en", out var text))
                {
                    MainTranslations[key] = text;
                }
            }

            foreach (var (key, value) in oldState.Todos)
            {
                translationsTodo[key] = value;
            }

            Save();
        }

        public void Save()
        {
            foreach (var (locale, texts) in translations)
            {
                Save($"_{locale}.json", texts);
            }

            Save("__todos.json", translationsTodo);
            Save("__ignore.json", translationToIgnore);
        }

        public void Translate(string fileName, string text, string originText, Action<string> handler, bool silent = false)
        {
            if (onlySingleWords && text.Contains(' '))
            {
                return;
            }

            if (!IsIgnored(fileName, text))
            {
                var (key, keyState) = MainTranslations.FirstOrDefault(x => x.Value == text);

                if (string.IsNullOrWhiteSpace(key))
                {
                    if (silent)
                    {
                        handler("DUMMY");
                    }
                    else
                    {
                        Console.WriteLine();
                        Console.WriteLine(">>> {0}", text);
                        Console.WriteLine("{1} in {0}", fileName, originText);
                        Console.WriteLine();

                        while (true)
                        {
                            Console.WriteLine("Enter key or <Enter> (USE with previous prefix), <t> (TODO), <s> (SKIP), <i> (IGNORE), <f> (IGNORE FILE)");

                            key = Console.ReadLine();

                            if (string.IsNullOrWhiteSpace(key))
                            {
                                key = $"common.{text.ToLower()}";

                                if (translations.TryGetValue(key, out var existing))
                                {
                                    Console.WriteLine("Key is already in use with {0}", existing);
                                    continue;
                                }
                                else
                                {
                                    AddText(key, text);

                                    handler(key);
                                }

                                break;
                            }
                            else if (key.Equals("s", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                Console.WriteLine("Skipped");
                            }
                            else if (key.Equals("i", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                Console.WriteLine("Ignored");
                                AddIgnore(fileName, text);
                            }
                            else if (key.Equals("f", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                Console.WriteLine("Ignored File");
                                AddIgnore(fileName, "*");
                            }
                            else if (key.Equals("t", StringComparison.OrdinalIgnoreCase) == true)
                            {
                                Console.WriteLine("ToDo");
                                AddTodo(fileName, text);

                                AddIgnore(fileName, text);
                            }
                            else
                            {
                                if (!key.Contains("."))
                                {
                                    if (previousPrefix != null)
                                    {
                                        key = $"{previousPrefix}.{key}";
                                    }
                                    else
                                    {
                                        key = $"common.{key}";
                                    }
                                }
                                else
                                {
                                    previousPrefix = string.Join('.', key.Split('.', StringSplitOptions.RemoveEmptyEntries).Skip(1));
                                }

                                var useOlder = key.EndsWith("?", StringComparison.OrdinalIgnoreCase);
                                var useNewer = key.EndsWith("!", StringComparison.OrdinalIgnoreCase);

                                if (translations.TryGetValue(key, out var existing) && !useOlder && !useNewer)
                                {
                                    Console.WriteLine("Key is already in use with '{0}'", existing);
                                    continue;
                                }
                                else
                                {
                                    key = key.TrimEnd('!', '?');

                                    if (!useOlder)
                                    {
                                        AddText(key, text);
                                    }

                                    handler(key);
                                }
                            }

                            break;
                        }
                    }
                }
                else
                {
                    handler(key);
                }

                Save();
            }
        }

        private bool IsIgnored(string name, string text)
        {
            return translationToIgnore.TryGetValue(name, out var ignores) && (ignores.Contains(text) || ignores.Contains("*"));
        }

        private void AddText(string key, string text)
        {
            MainTranslations[key] = text;
        }

        private void AddIgnore(string name, string text)
        {
            if (!translationToIgnore.TryGetValue(name, out var ignores))
            {
                ignores = new SortedSet<string>();

                translationToIgnore[name] = ignores;
            }

            ignores.Add(text);
        }

        private void AddTodo(string name, string text)
        {
            if (!translationsTodo.TryGetValue(name, out var todos))
            {
                todos = new SortedSet<string>();

                translationsTodo[name] = todos;
            }

            todos.Add(text);
        }
    }
}
