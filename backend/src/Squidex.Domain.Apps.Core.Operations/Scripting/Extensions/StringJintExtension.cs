// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.IO;
using System.Text;
using HtmlAgilityPack;
using Jint;
using Jint.Native;
using Markdig;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting.Extensions
{
    public sealed class StringJintExtension : IJintExtension
    {
        private delegate JsValue StringSlugifyDelegate(string text, bool single = false);

        private readonly StringSlugifyDelegate slugify = (text, single) =>
        {
            try
            {
                return text.Slugify(null, single);
            }
            catch
            {
                return JsValue.Undefined;
            }
        };

        private readonly Func<string, JsValue> toCamelCase = text =>
        {
            try
            {
                return text.ToCamelCase();
            }
            catch
            {
                return JsValue.Undefined;
            }
        };

        private readonly Func<string, JsValue> toPascalCase = text =>
        {
            try
            {
                return text.ToPascalCase();
            }
            catch
            {
                return JsValue.Undefined;
            }
        };

        private readonly Func<string, JsValue> html2Text = text =>
        {
            try
            {
                var document = LoadHtml(text);

                var sb = new StringBuilder();

                WriteTextTo(document.DocumentNode, sb);

                return sb.ToString().Trim(' ', '\n', '\r');
            }
            catch
            {
                return JsValue.Undefined;
            }
        };

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

        private readonly Func<string, JsValue> markdown2Text = text =>
        {
            try
            {
                return Markdown.ToPlainText(text).Trim(' ', '\n', '\r');
            }
            catch
            {
                return JsValue.Undefined;
            }
        };

        public Func<string, JsValue> Html2Text => html2Text;

        public void Extend(Engine engine)
        {
            engine.SetValue("slugify", slugify);

            engine.SetValue("toCamelCase", toCamelCase);
            engine.SetValue("toPascalCase", toPascalCase);

            engine.SetValue("html2Text", Html2Text);

            engine.SetValue("markdown2Text", markdown2Text);
        }
    }
}
