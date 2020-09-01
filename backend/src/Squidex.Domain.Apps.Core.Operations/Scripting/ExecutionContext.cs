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
    public delegate bool ExceptionHandler(Exception exception);

    public sealed class ExecutionContext : Dictionary<string, object>
    {
        private readonly ExceptionHandler? exceptionHandler;

        public Engine Engine { get; }

        public CancellationToken CancellationToken { get; }

        public bool IsAsync { get; private set; }

        internal ExecutionContext(Engine engine, CancellationToken cancellationToken, ExceptionHandler? exceptionHandler = null)
            : base(StringComparer.OrdinalIgnoreCase)
        {
            Engine = engine;

            CancellationToken = cancellationToken;

            this.exceptionHandler = exceptionHandler;
        }

        public void MarkAsync()
        {
            IsAsync = true;
        }

        public void Fail(Exception exception)
        {
            exceptionHandler?.Invoke(exception);
        }

        public void AddVariables(ScriptVars vars, ScriptOptions options)
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
        }
    }
}
