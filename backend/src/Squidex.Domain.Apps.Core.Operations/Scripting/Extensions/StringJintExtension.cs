// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Jint.Native;
using Squidex.Domain.Apps.Core.Properties;
using Squidex.Infrastructure;
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.Scripting.Extensions;

public sealed class StringJintExtension : IJintExtension, IScriptDescriptor
{
    private delegate JsValue StringSlugifyDelegate(string text, bool single = false);

    private readonly Func<string, JsValue> sha256 = text =>
    {
        try
        {
            return text.ToSha256();
        }
        catch
        {
            return JsValue.Undefined;
        }
    };

    private readonly Func<string, JsValue> sha512 = text =>
    {
        try
        {
            return text.ToSha512();
        }
        catch
        {
            return JsValue.Undefined;
        }
    };

    private readonly Func<string, JsValue> md5 = text =>
    {
        try
        {
            return text.ToMD5();
        }
        catch
        {
            return JsValue.Undefined;
        }
    };

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
            return text.Html2Text();
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
            return text.Markdown2Text();
        }
        catch
        {
            return JsValue.Undefined;
        }
    };

    private readonly Func<string> guid = () =>
    {
        return Guid.NewGuid().ToString();
    };

    public Func<string, JsValue> Html2Text => html2Text;

    public void Extend(Engine engine)
    {
        engine.SetValue("guid", guid);
        engine.SetValue("html2Text", Html2Text);
        engine.SetValue("markdown2Text", markdown2Text);
        engine.SetValue("md5", md5);
        engine.SetValue("sha256", sha256);
        engine.SetValue("sha512", sha512);
        engine.SetValue("slugify", slugify);
        engine.SetValue("toCamelCase", toCamelCase);
        engine.SetValue("toPascalCase", toPascalCase);
    }

    public void Describe(AddDescription describe, ScriptScope scope)
    {
        describe(JsonType.Function, "html2Text(text)",
            Resources.ScriptingHtml2Text);

        describe(JsonType.Function, "markdown2Text(text)",
            Resources.ScriptingMarkdown2Text);

        describe(JsonType.Function, "toCamelCase(text)",
            Resources.ScriptingToCamelCase);

        describe(JsonType.Function, "toPascalCase(text)",
            Resources.ScriptingToPascalCase);

        describe(JsonType.Function, "md5(text)",
            Resources.ScriptingMD5);

        describe(JsonType.Function, "sha256(text)",
            Resources.ScriptingSHA256);

        describe(JsonType.Function, "sha512(text)",
            Resources.ScriptingSHA512);

        describe(JsonType.Function, "slugify(text)",
            Resources.ScriptingSlugify);

        describe(JsonType.Function, "guid()",
            Resources.ScriptingGuid);
    }
}
