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
using Squidex.Infrastructure.Json.Objects;
using Squidex.Shared.Users;

namespace Squidex.Domain.Apps.Core.Scripting.Internal;

public sealed class JintObjectConverter : IObjectConverter
{
    /// <summary>
    /// The types this converter handles, passed to Jint when the converter is registered.
    /// </summary>
    /// <remarks>
    /// Without this list Jint has to offer every property of every .NET object to this converter and cannot
    /// use its faster property reader for any of them. Base types and interfaces count, so
    /// <see cref="IUser"/> covers all implementations. Keep the list in sync with the switch below - a type
    /// that is converted but not listed here fails a test (see JintHostContractVerification). Enums are
    /// missing on purpose, they are converted by Jint itself, see EnumConversion in JintScriptEngine.
    /// </remarks>
    public static readonly Type[] HandledTypes =
    [
        typeof(IUser),
        typeof(ClaimsPrincipal),
        typeof(ScriptVars),
        typeof(JsonValue),
        typeof(DomainId),
        typeof(Guid),
        typeof(Instant),
        typeof(Status),
        typeof(ContentData),
    ];

    public static readonly JintObjectConverter Instance = new JintObjectConverter();

    private JintObjectConverter()
    {
    }

    public bool TryConvert(Engine engine, object value, [MaybeNullWhen(false)] out JsValue result)
    {
        result = null!;

        switch (value)
        {
            case IUser user:
                result = JintUser.Create(engine, user);
                return true;
            case ClaimsPrincipal principal:
                result = JintUser.Create(engine, principal);
                return true;
            case ScriptVars vars:
                result = ObjectWrapper.Create(engine, vars);
                return true;
            case JsonValue jsonValue:
                result = JsonMapper.Map(jsonValue, engine);
                return true;
            case DomainId domainId:
                result = domainId.ToString();
                return true;
            case Guid guid:
                result = guid.ToString();
                return true;
            case Instant instant:
                result = new JsDate(engine, instant.ToDateTimeUtc());
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
