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
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using Squidex.Translator.State;

#pragma warning disable SA1025 // Code should not contain multiple whitespace in a row

namespace Squidex.Translator.Processes
{
    public class TranslateTemplates
    {
        private static readonly HashSet<string> TagsToIgnore = new HashSet<string>
        {
            "code",
            "script",
            "sqx-code",
            "style"
        };

        private static readonly HashSet<string> AttributesToTranslate = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "title",        // Tooltip
            "placeholder",  // Input placeholder
            "confirmTitle", // Confirm Click
            "confirmText",  // Confirm Click
            "message",      // Title Component
        };

        private readonly TranslationService service;
        private readonly DirectoryInfo folder;
        private bool isReplaced;
        private bool isSilent;
        private int total;

        public TranslateTemplates(DirectoryInfo folder, TranslationService service)
        {
            this.folder = Frontend.GetFolder(folder);

            this.service = service;
        }

        public void Run(bool reportMissing)
        {
            isSilent = reportMissing;

            foreach (var (file, relativeName) in Frontend.GetTemplateFiles(folder))
            {
                isReplaced = false;

                // Keep the original casing, otherwise *ngIf is translated to ngif
                var html = new HtmlDocument
                {
                    OptionOutputOriginalCase = true
                };

                html.LoadHtml(File.ReadAllText(file.FullName));

                Traverse(relativeName, html.DocumentNode);

                if (isReplaced && !reportMissing)
                {
                    Console.WriteLine("-----------------------------");
                    Console.WriteLine("FILE {0} done", relativeName);

                    SaveHtml(file, html);
                }
            }

            if (reportMissing)
            {
                Console.WriteLine("TODO: {0}", total);
            }
        }

        private void Traverse(string fileName, HtmlNode node)
        {
            if (TagsToIgnore.Contains(node.Name))
            {
                return;
            }

            if (node is HtmlTextNode textNode)
            {
                var text = textNode.Text;

                // For strings like Next: <div></div>
                var trimmed = text.Trim().Trim(':', '(', ')');

                const string whitespace = "&nbsp;";

                while (trimmed.StartsWith(whitespace, StringComparison.OrdinalIgnoreCase))
                {
                    trimmed = trimmed.Substring(whitespace.Length);
                }

                while (trimmed.EndsWith(whitespace, StringComparison.OrdinalIgnoreCase))
                {
                    trimmed = trimmed.Substring(0, trimmed.Length - whitespace.Length);
                }

                if (!string.IsNullOrWhiteSpace(trimmed) && !IsTranslated(trimmed) && !IsVariable(trimmed))
                {
                    if (!string.IsNullOrWhiteSpace(trimmed))
                    {
                        // Extract prefix and suffix to keep our original indentation.
                        var originalIndex = text.IndexOf(trimmed, StringComparison.Ordinal);

                        var originalPrefix = text.Substring(0, originalIndex);
                        var originalSuffix = text.Substring(originalIndex + trimmed.Length);

                        var originText = $"text in {textNode.ParentNode.Name}";

                        service.Translate(fileName, trimmed, originText, key =>
                        {
                            if (isSilent)
                            {
                                total++;
                            }
                            else
                            {
                                // Keep our original indentation.
                                textNode.Text = originalPrefix + $"{{{{ '{key}' | sqxTranslate }}}}" + originalSuffix;

                                isReplaced = true;
                            }
                        }, isSilent);
                    }
                }
            }
            else
            {
                foreach (var attribute in node.Attributes.ToList())
                {
                    if (AttributesToTranslate.Contains(attribute.Name) && !string.IsNullOrWhiteSpace(attribute.Value) && !IsPipe(attribute) && !IsTranslatedAttribute(attribute.Value))
                    {
                        var originText = $"{attribute.Name} attribute";

                        service.Translate(fileName, attribute.Value, originText, key =>
                        {
                            if (isSilent)
                            {
                                total++;
                            }
                            else
                            {
                                if (attribute.Name.Contains("["))
                                {
                                    node.SetAttributeValue(attribute.Name, $"{{{{ '{key}' | sqxTranslate }}}}");
                                }
                                else
                                {
                                    node.SetAttributeValue(attribute.Name, $"i18n:{key}");
                                }

                                isReplaced = true;
                            }
                        }, isSilent);
                    }
                }
            }

            foreach (var child in node.ChildNodes)
            {
                Traverse(fileName, child);
            }
        }

        private static bool IsPipe(HtmlAttribute attribute)
        {
            return attribute.Value.Contains("{");
        }

        private bool IsTranslatedAttribute(string text)
        {
            return text.Contains("i18n:");
        }

        private bool IsTranslated(string text)
        {
            return text.Contains("| sqxTranslate");
        }

        private bool IsVariable(string text)
        {
            return text.StartsWith("{{", StringComparison.Ordinal) && Regex.Matches(text, "\\}\\}").Count == 1;
        }

        private static void SaveHtml(FileInfo file, HtmlDocument html)
        {
            html.Save(file.FullName);

            var text = File.ReadAllText(file.FullName);

            // Fix the attributes, because html agility packs converts attributes without value to attributes with empty string.
            // For example
            // <ng-container content> becomes <ng-container content="">
            text = Regex.Replace(text, " (?<Name>[^\\s]*)=\"\"", x => " " + x.Groups["Name"].Value);

            File.WriteAllText(file.FullName, text);
        }
    }
}
