// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Scripting
{
    public sealed class ScriptVars : ScriptContext
    {
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

        public DomainId SchemaId
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

        public string? SchemaName
        {
            get => GetValue<string?>();
            set => SetValue(value);
        }

        public string? Operation
        {
            get => GetValue<string?>();
            set => SetValue(value);
        }

        public ContentData? Data
        {
            get => GetValue<ContentData?>();
            set => SetValue(value);
        }

#pragma warning disable CS0618 // Type or member is obsolete
        public ContentData? DataOld
        {
            get => GetValue<ContentData?>();
            set
            {
                SetValue(value, nameof(OldData));
                SetValue(value);
            }
        }

        public Status StatusOld
        {
            get => GetValue<Status>();
            set
            {
                SetValue(value, nameof(OldStatus));
                SetValue(value);
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete

        [Obsolete("Use dataOld")]
        public ContentData? OldData
        {
            get => GetValue<ContentData?>();
        }

        [Obsolete("Use statusOld")]
        public Status? OldStatus
        {
            get => GetValue<Status?>();
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
