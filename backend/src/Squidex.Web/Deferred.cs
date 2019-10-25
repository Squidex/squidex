﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Infrastructure;

namespace Squidex.Web
{
    public struct Deferred
    {
        private readonly Lazy<Task<object>> value;

        public Task<object> Value
        {
            get { return value.Value; }
        }

        private Deferred(Func<Task<object>> value)
        {
            this.value = new Lazy<Task<object>>(value);
        }

        public static Deferred Response(Func<object> factory)
        {
            Guard.NotNull(factory);

            return new Deferred(() => Task.FromResult(factory()));
        }

        public static Deferred AsyncResponse<T>(Func<Task<T>> factory)
        {
            Guard.NotNull(factory);

            return new Deferred(async () => (await factory())!);
        }
    }
}
