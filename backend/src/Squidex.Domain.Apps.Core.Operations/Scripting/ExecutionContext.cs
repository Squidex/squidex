// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Threading;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Squidex.Text;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class ExecutionContext : ScriptContext
    {
        private Func<Exception, bool>? completion;

        public Engine Engine { get; }

        public CancellationToken CancellationToken { get; private set; }

        public bool IsAsync { get; private set; }

        internal ExecutionContext(Engine engine)
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

        public ExecutionContext ExtendAsync(IEnumerable<IJintExtension> extensions, Func<Exception, bool> completion,
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

        public ExecutionContext Extend(IEnumerable<IJintExtension> extensions)
        {
            foreach (var extension in extensions)
            {
                extension.Extend(this);
            }

            return this;
        }

        public ExecutionContext Extend(ScriptVars vars, ScriptOptions options)
        {
            var engine = Engine;

            if (options.AsContext)
            {
                var contextInstance = new ObjectInstance(engine);

                foreach (var (key, value) in vars)
                {
                    var property = key.ToCamelCase();

                    if (value != null)
                    {
                        contextInstance.FastAddProperty(property, JsValue.FromObject(engine, value), true, true, true);

                        this[property] = value;
                    }
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
