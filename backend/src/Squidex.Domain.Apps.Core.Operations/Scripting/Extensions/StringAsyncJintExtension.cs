// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Jint.Native;
using Jint.Runtime;
using Squidex.AI;
using Squidex.Domain.Apps.Core.Properties;
using Squidex.Text.Translations;

#pragma warning disable CA1826 // Do not use Enumerable methods on indexable collections

namespace Squidex.Domain.Apps.Core.Scripting.Extensions;

public sealed class StringAsyncJintExtension(ITranslator translator, IChatAgent chatAgent) : IJintExtension, IScriptDescriptor
{
    private delegate void TextGenerateDelegate(string prompt, Action<JsValue> callback);
    private delegate void TextTranslateDelegate(string text, string language, Action<JsValue> callback, string sourceLanguage);

    public void ExtendAsync(Engine engine)
    {
        var generate = new TextGenerateDelegate((prompt, callback) =>
        {
            Generate(engine, prompt, callback);
        });

        var translate = new TextTranslateDelegate((text, language, callback, sourceLanguage) =>
        {
            Translate(engine, text, language, callback, sourceLanguage);
        });

        engine.SetValue("generate", generate);
        engine.SetValue("translate", translate);
    }

    private void Generate(Engine engine, string prompt, Action<JsValue> callback)
    {
        if (callback == null)
        {
            throw new JavaScriptException("Callback is not defined.");
        }

        // We are still inside the engine here, therefore the callback can be invoked directly.
        if (string.IsNullOrWhiteSpace(prompt))
        {
            callback(JsValue.Null);
            return;
        }

        engine.Schedule(async ct =>
        {
            try
            {
                var request = new ChatRequest
                {
                    Prompt = prompt,
                };

                var result = await chatAgent.PromptAsync(request, ct: ct);

                return result.Content;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new JavaScriptException(ex.Message);
            }
        },
        content => callback(JsValue.FromObject(engine, content)));
    }

    private void Translate(Engine engine, string text, string language, Action<JsValue> callback, string sourceLanguage)
    {
        if (callback == null)
        {
            throw new JavaScriptException("Callback is not defined.");
        }

        // We are still inside the engine here, therefore the callback can be invoked directly.
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(language))
        {
            callback(JsValue.Null);
            return;
        }

        engine.Schedule(async ct =>
        {
            try
            {
                var translation = await translator.TranslateAsync(text, language, sourceLanguage, ct);

                return translation.Text;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                throw new JavaScriptException(ex.Message);
            }
        },
        translated => callback(JsValue.FromObject(engine, translated)));
    }

    public void Describe(AddDescription describe, ScriptScope scope)
    {
        if (scope.HasFlag(ScriptScope.Async))
        {
            describe(JsonType.Function, "generate(prompt, callback?",
                Resources.ScriptingGenerate);

            describe(JsonType.Function, "translate(text, language, callback, sourceLanguage?",
                Resources.ScriptingTranslate);
        }
    }
}
