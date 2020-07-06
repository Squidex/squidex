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
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class ScriptVars : Dictionary<string, object?>
    {
        public ScriptVars()
            : base(StringComparer.OrdinalIgnoreCase)
        {
        }

        public ClaimsPrincipal? User
        {
            get => GetValue<ClaimsPrincipal?>();
            set => SetValue(value);
        }

        public DomainId AppId
        {
            get => GetValue<DomainId>();
            set => SetValue(value);
        }

        public DomainId ContentId
        {
            get => GetValue<DomainId>();
            set => SetValue(value);
        }

        public Status Status
        {
            get => GetValue<Status>();
            set => SetValue(value);
        }

        public string? AppName
        {
            get => GetValue<string?>();
            set => SetValue(value);
        }

        public string? Operation
        {
            get => GetValue<string?>();
            set => SetValue(value);
        }

        public NamedContentData? Data
        {
            get => GetValue<NamedContentData?>();
            set => SetValue(value);
        }

        public NamedContentData? DataOld
        {
            get => GetValue<NamedContentData?>();
            set
            {
                SetValue(value, "oldData");
                SetValue(value);
            }
        }

        public Status StatusOld
        {
            get => GetValue<Status>();
            set
            {
                SetValue(value, "oldStatus");
                SetValue(value);
            }
        }

        public void SetValue(object? value, [CallerMemberName] string? key = null)
        {
            if (key != null)
            {
                this[key] = value;
            }
        }

        public T GetValue<T>([CallerMemberName] string? key = null)
        {
            if (key != null && TryGetValue(key, out var temp) && temp is T result)
            {
                return result;
            }

            return default!;
        }
    }
}
