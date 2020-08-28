// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Jint;
using Jint.Native;
using Squidex.Text;

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
                return TextHelpers.Html2Text(text);
            }
            catch
            {
                return JsValue.Undefined;
            }
        };

        private readonly Func<string, JsValue> markdown2Text = text =>
        {
            try
            {
                return TextHelpers.Markdown2Text(text);
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
