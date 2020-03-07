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

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class ExecutionContext : Dictionary<string, object>
    {
        private readonly Action<Exception>? exceptionHandler;

        public Engine Engine { get; }

        public CancellationToken CancellationToken { get; }

        public ExecutionContext(Engine engine, CancellationToken cancellationToken, Action<Exception>? exceptionHandler = null)
        {
            Engine = engine;

            CancellationToken = cancellationToken;

            this.exceptionHandler = exceptionHandler;
        }

        public void Fail(Exception exception)
        {
            exceptionHandler?.Invoke(exception);
        }

        public ExecutionContext SetValue(string key, object value)
        {
            this[key] = value;

            return this;
        }
    }
}
