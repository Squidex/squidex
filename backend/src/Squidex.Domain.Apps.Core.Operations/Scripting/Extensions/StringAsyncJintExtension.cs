// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint.Native;
using Jint.Runtime;
using Squidex.AI;
using Squidex.Domain.Apps.Core.Properties;
using Squidex.Text.Translations;

#pragma warning disable CA1826 // Do not use Enumerable methods on indexable collections

namespace Squidex.Domain.Apps.Core.Scripting.Extensions;

public sealed class StringAsyncJintExtension : IJintExtension, IScriptDescriptor
{
    private delegate void TextGenerateDelegate(string prompt, Action<JsValue> callback);
    private delegate void TextTranslateDelegate(string text, string language, Action<JsValue> callback, string sourceLanguage);
    private readonly ITranslator translator;
    private readonly IChatAgent chatAgent;

    public StringAsyncJintExtension(ITranslator translator, IChatAgent chatAgent)
    {
        this.translator = translator;
        this.chatAgent = chatAgent;
    }

    public void ExtendAsync(ScriptExecutionContext context)
    {
        var generate = new TextGenerateDelegate((prompt, callback) =>
        {
            Generate(context, prompt, callback);
        });

        var translate = new TextTranslateDelegate((text, language, callback, sourceLanguage) =>
        {
            Translate(context, text, language, callback, sourceLanguage);
        });

        context.Engine.SetValue("generate", generate);
        context.Engine.SetValue("translate", translate);
    }

    private void Generate(ScriptExecutionContext context, string prompt, Action<JsValue> callback)
    {
        if (callback == null)
        {
            throw new JavaScriptException("Callback is not defined.");
        }

        context.Schedule(async (scheduler, ct) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(prompt))
                {
                    scheduler.Run(callback, JsValue.Null);
                    return;
                }

                var request = new ChatRequest
                {
                    Prompt = prompt
                };

                var result = await chatAgent.PromptAsync(request, ct: ct);

                scheduler.Run(callback, JsValue.FromObject(context.Engine, result.Content));
            }
            catch (Exception ex)
            {
                throw new JavaScriptException(ex.Message);
            }
        });
    }

    private void Translate(ScriptExecutionContext context, string text, string language, Action<JsValue> callback, string sourceLanguage)
    {
        if (callback == null)
        {
            throw new JavaScriptException("Callback is not defined.");
        }

        context.Schedule(async (scheduler, ct) =>
        {
            try
            {
                if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(language))
                {
                    scheduler.Run(callback, JsValue.Null);
                    return;
                }

                var translation = await translator.TranslateAsync(text, language, sourceLanguage, ct);

                scheduler.Run(callback, JsValue.FromObject(context.Engine, translation.Text));
            }
            catch (Exception ex)
            {
                throw new JavaScriptException(ex.Message);
            }
        });
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
