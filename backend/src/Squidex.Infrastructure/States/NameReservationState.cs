// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.States;

public sealed class NameReservationState : SimpleState<NameReservationState.State>
{
    [CollectionName("Names")]
    public sealed class State
    {
        public List<NameReservation> Reservations { get; set; } = new List<NameReservation>();

        public (bool, string?) Reserve(DomainId id, string name)
        {
            string? token = null;

            var reservation = Reservations.Find(x => x.Name == name);
            var reserved = false;

            if (reservation?.Id == id)
            {
                token = reservation.Token;
            }
            else if (reservation == null)
            {
                token = RandomHash.Simple();

                Reservations.Add(new NameReservation(token, name, id));
                reserved = true;
            }

            return (reserved, token);
        }

        public bool Remove(string? token)
        {
            return Reservations.RemoveAll(x => x.Token == token) > 0;
        }
    }

    public NameReservationState(IPersistenceFactory<State> persistenceFactory, string id)
        : base(persistenceFactory, typeof(NameReservationState), id)
    {
    }

    public NameReservationState(IPersistenceFactory<State> persistenceFactory, DomainId id)
        : base(persistenceFactory, typeof(NameReservationState), id)
    {
    }

    public async Task<string?> ReserveAsync(DomainId id, string name,
        CancellationToken ct = default)
    {
        try
        {
            return await UpdateAsync(s => s.Reserve(id, name), ct: ct);
        }
        catch (InconsistentStateException)
        {
            return null;
        }
    }

    public async Task RemoveReservationAsync(string? token,
        CancellationToken ct = default)
    {
        try
        {
            await UpdateAsync(s => s.Remove(token), ct: ct);
        }
        catch (InconsistentStateException)
        {
            return;
        }
    }
}
