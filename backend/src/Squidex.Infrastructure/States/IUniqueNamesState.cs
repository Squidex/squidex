// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.States
{
    public interface IUniqueNamesState
    {
        Task LoadAsync(
            CancellationToken ct = default);

        Task<string?> ReserveAsync(DomainId id, string name,
            CancellationToken ct = default);

        Task RemoveReservationAsync(string? token,
            CancellationToken ct = default);
    }
}
