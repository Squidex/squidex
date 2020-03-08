// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using Jint.Native;
using Jint.Native.Object;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class ScriptContext : Dictionary<string, object?>
    {
        public ScriptContext()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public ClaimsPrincipal? User
        {
            get => GetValue<ClaimsPrincipal?>();
            set => SetValue(value);
        }

        public Guid ContentId
        {
            get => GetValue<Guid>();
            set => SetValue(value);
        }

        public NamedContentData? Data
        {
            get => GetValue<NamedContentData?>();
            set => SetValue(value);
        }

        public NamedContentData? DataOld
        {
            get => GetValue<NamedContentData?>("oldData");
            set => SetValue(value, "oldData");
        }

        public Status Status
        {
            get => GetValue<Status>();
            set => SetValue(value);
        }

        public Status StatusOld
        {
            get => GetValue<Status>();
            set => SetValue(value);
        }

        public string? Operation
        {
            get => GetValue<string?>();
            set => SetValue(value);
        }

        public void SetValue(object? value, [CallerMemberNameAttribute] string? key = null)
        {
            if (key != null)
            {
                this[key] = value;
            }
        }

        public T GetValue<T>([CallerMemberNameAttribute] string? key = null)
        {
            if (key != null && TryGetValue(key, out var temp) && temp is T result)
            {
                return result;
            }

            return default!;
        }

        internal void Add(ExecutionContext context, bool nested)
        {
            var engine = context.Engine;

            if (nested)
            {
                var contextInstance = new ObjectInstance(engine);

                foreach (var (key, value) in this)
                {
                    var property = key.ToCamelCase();

                    if (value != null)
                    {
                        contextInstance.FastAddProperty(property, JsValue.FromObject(engine, value), true, true, true);
                        context[property] = value;
                    }
                }

                engine.SetValue("ctx", contextInstance);
                engine.SetValue("context", contextInstance);
            }
            else
            {
                foreach (var (key, value) in this)
                {
                    var property = key.ToCamelCase();

                    if (value != null)
                    {
                        engine.SetValue(property, value);
                        context[property] = value;
                    }
                }
            }
        }
    }
}
