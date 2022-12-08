// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using Jint;
using Jint.Native;
using Jint.Runtime.Interop;
using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.Scripting.ContentWrapper;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Core.Scripting;

public sealed class DefaultConverter : IObjectConverter
{
    public static readonly DefaultConverter Instance = new DefaultConverter();

    private DefaultConverter()
    {
    }

    public bool TryConvert(Engine engine, object value, [MaybeNullWhen(false)] out JsValue result)
    {
        result = null!;

        if (value is Enum)
        {
            result = value.ToString();
            return true;
        }

        switch (value)
        {
            case IUser user:
                result = JintUser.Create(engine, user);
                return true;
            case ClaimsPrincipal principal:
                result = JintUser.Create(engine, principal);
                return true;
            case DomainId domainId:
                result = domainId.ToString();
                return true;
            case Guid guid:
                result = guid.ToString();
                return true;
            case Instant instant:
                result = JsValue.FromObject(engine, instant.ToDateTimeUtc());
                return true;
            case Status status:
                result = status.ToString();
                return true;
            case ContentData content:
                result = new ContentDataObject(engine, content);
                return true;
        }

        return false;
    }
}
