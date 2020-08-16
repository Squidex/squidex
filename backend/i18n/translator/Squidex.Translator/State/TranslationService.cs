﻿// ==========================================================================
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

        private readonly TranslatedTexts translations;
        private readonly TranslationTodos translationsTodo;
        private readonly TranslationsToIgnore translationToIgnore;
        private readonly DirectoryInfo directory;
        private readonly string fileName;
        private readonly bool onlySingleWords;
        private string previousPrefix;

        public IReadOnlyDictionary<string, string> Texts
        {
            get { return translations; }
        }

        static TranslationService()
        {
            SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        public TranslationService(DirectoryInfo directory, string fileName, bool onlySingleWords)
        {
            this.directory = directory;

            this.fileName = fileName;

            translations = Load<TranslatedTexts>("_en.json");
            translationsTodo = Load<TranslationTodos>("__todos.json");
            translationToIgnore = Load<TranslationsToIgnore>("__ignore.json");

            this.onlySingleWords = onlySingleWords;
        }

        private T Load<T>(string name) where T : new()
        {
            var fullName = Path.Combine(directory.FullName, $"{fileName}{name}");

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
            var path = Path.Combine(directory.FullName, this.fileName + name);

            WriteTo(value, path);
        }

        private void WriteTo<T>(T value, string path) where T : new()
        {
            var json = JsonSerializer.Serialize(value, SerializerOptions);

            if (!directory.Exists)
            {
                Directory.CreateDirectory(directory.FullName);
            }

            File.WriteAllText(path, json);
        }

        public void WriteTexts(string path)
        {
            WriteTo(translations, path);
        }

        public void Migrate()
        {
            var oldState = Load<OldTranslationState>(".json");

            foreach (var (key, value) in oldState.Texts)
            {
                if (value.Texts.TryGetValue("en", out var text))
                {
                    translations[key] = text;
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
            Save("_en.json", translations);

            Save("__todos.json", translationsTodo);
            Save("__ignore.json", translationToIgnore);
            SaveKeys();
        }

        public void SaveKeys()
        {
            TranslationKeys translationKeys = new TranslationKeys();
            foreach (var text in Texts)
            {
                translationKeys.Add(text.Key, string.Empty);
            }

            Save("__keys.json", translationKeys);
        }

        public void Translate(string fileName, string text, string originText, Action<string> handler, bool silent = false)
        {
            if (onlySingleWords && text.Contains(' '))
            {
                return;
            }

            if (!IsIgnored(fileName, text))
            {
                var (key, keyState) = translations.FirstOrDefault(x => x.Value == text);

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
            translations[key] = text;
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
