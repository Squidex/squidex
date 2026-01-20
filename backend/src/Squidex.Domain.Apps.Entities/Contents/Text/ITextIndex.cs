// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Apps;
using Squidex.Infrastructure;

#pragma warning disable MA0048 // File name must match type name
#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Entities.Contents.Text;

public record class UserInfoResult(DomainId ContentId, string Role);

public interface ITextIndex
{
    Task<List<DomainId>?> SearchAsync(App app, TextQuery query, SearchScope scope,
        CancellationToken ct = default);

    Task<List<DomainId>?> SearchAsync(App app, GeoQuery query, SearchScope scope,
        CancellationToken ct = default);

    Task<UserInfoResult?> FindUserInfo(App app, ApiKeyQuery query, SearchScope scope,
        CancellationToken ct = default);

    Task ClearAsync(
        CancellationToken ct = default);

    Task ExecuteAsync(IndexCommand[] commands,
        CancellationToken ct = default);
}
