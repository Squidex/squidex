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
    public delegate bool ExceptionHandler(Exception exception);

    public sealed class ExecutionContext : Dictionary<string, object>
    {
        private readonly ExceptionHandler? exceptionHandler;

        public Engine Engine { get; }

        public CancellationToken CancellationToken { get; }

        internal ExecutionContext(Engine engine, CancellationToken cancellationToken, ExceptionHandler? exceptionHandler = null)
            : base(StringComparer.OrdinalIgnoreCase)
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
