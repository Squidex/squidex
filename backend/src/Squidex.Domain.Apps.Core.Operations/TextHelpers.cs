// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text;
using HtmlAgilityPack;
using Markdig;

namespace Squidex.Domain.Apps.Core
{
    public static class TextHelpers
    {
        public static string Markdown2Text(string markdown)
        {
            return Markdown.ToPlainText(markdown).Trim(' ', '\n', '\r');
        }

        public static string Html2Text(string html)
        {
            var document = LoadHtml(html);

            var sb = new StringBuilder();

            WriteTextTo(document.DocumentNode, sb);

            return sb.ToString().Trim(' ', '\n', '\r');
        }

        private static HtmlDocument LoadHtml(string text)
        {
            var document = new HtmlDocument();

            document.LoadHtml(text);

            return document;
        }

        private static void WriteTextTo(HtmlNode node, StringBuilder sb)
        {
            switch (node.NodeType)
            {
                case HtmlNodeType.Comment:
                    break;
                case HtmlNodeType.Document:
                    WriteChildrenTextTo(node, sb);
                    break;
                case HtmlNodeType.Text:
                    var html = ((HtmlTextNode)node).Text;

                    if (HtmlNode.IsOverlappedClosingElement(html))
                    {
                        break;
                    }

                    if (!string.IsNullOrWhiteSpace(html))
                    {
                        sb.Append(HtmlEntity.DeEntitize(html));
                    }

                    break;

                case HtmlNodeType.Element:
                    switch (node.Name)
                    {
                        case "p":
                            sb.AppendLine();
                            break;
                        case "br":
                            sb.AppendLine();
                            break;
                        case "style":
                            return;
                        case "script":
                            return;
                    }

                    if (node.HasChildNodes)
                    {
                        WriteChildrenTextTo(node, sb);
                    }

                    break;
            }
        }

        private static void WriteChildrenTextTo(HtmlNode node, StringBuilder sb)
        {
            foreach (var child in node.ChildNodes)
            {
                WriteTextTo(child, sb);
            }
        }

        public static int CharacterCount(string text)
        {
            var characterCount = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (char.IsLetter(text[i]))
                {
                    characterCount++;
                }
            }

            return characterCount;
        }

        public static int WordCount(string text)
        {
            var numWords = 0;

            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsWhiteSpace(text[i - 1]))
                {
                    var character = text[i];

                    if (char.IsLetterOrDigit(character) || char.IsPunctuation(character))
                    {
                        numWords++;
                    }
                }
            }

            if (text.Length > 2)
            {
                numWords++;
            }

            return numWords;
        }
    }
}
