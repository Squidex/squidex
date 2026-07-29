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
    /// The CLR types this converter answers for, declared at registration so the engine can keep its
    /// compiled interop member-read lane for members whose declared type can never reach this converter.
    /// </summary>
    /// <remarks>
    /// Matching is by assignability, so <see cref="IUser"/> covers every implementation. Registering the
    /// converter without this set makes every wrapped CLR member read in the engine take the slow lane.
    /// Enums are not listed: they are handled natively through
    /// <see cref="Options.InteropOptions.EnumConversion"/>.
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
