// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class ScriptExecutionContext : ScriptContext
    {
        private Func<Exception, bool>? completion;

        public Engine Engine { get; }

        public CancellationToken CancellationToken { get; private set; }

        public bool IsAsync { get; private set; }

        internal ScriptExecutionContext(Engine engine)
        {
            Engine = engine;
        }

        public void MarkAsync()
        {
            IsAsync = true;
        }

        public void Fail(Exception exception)
        {
            completion?.Invoke(exception);
        }

        public ScriptExecutionContext ExtendAsync(IEnumerable<IJintExtension> extensions, Func<Exception, bool> completion,
            CancellationToken ct)
        {
            CancellationToken = ct;

            this.completion = completion;

            foreach (var extension in extensions)
            {
                extension.ExtendAsync(this);
            }

            return this;
        }

        public ScriptExecutionContext Extend(IEnumerable<IJintExtension> extensions)
        {
            foreach (var extension in extensions)
            {
                extension.Extend(this);
            }

            return this;
        }

        public ScriptExecutionContext Extend(ScriptVars vars, ScriptOptions options)
        {
            var engine = Engine;

            if (options.AsContext)
            {
                var contextInstance = new WritableContext(engine, vars);

                foreach (var (key, value) in vars.Where(x => x.Value != null))
                {
                    this[key.ToCamelCase()] = value;
                }

                engine.SetValue("ctx", contextInstance);
                engine.SetValue("context", contextInstance);
            }
            else
            {
                foreach (var (key, value) in vars)
                {
                    var property = key.ToCamelCase();

                    if (value != null)
                    {
                        engine.SetValue(property, value);

                        this[property] = value;
                    }
                }
            }

            engine.SetValue("async", true);

            return this;
        }
    }
}
