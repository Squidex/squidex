// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Infrastructure.States
{
    public sealed class UniqueNamesState : IUniqueNamesState
    {
        private readonly SimpleState<State> state;

        public sealed class State
        {
            public Dictionary<string, (string Name, DomainId Id)> Reservations { get; set; } = new Dictionary<string, (string Name, DomainId Id)>();

            public string? Reserve(DomainId id, string name)
            {
                string? token = null;

                var reservation = Reservations.FirstOrDefault(x => x.Value.Name == name);

                if (Equals(reservation.Value.Id, id))
                {
                    token = reservation.Key;
                }
                else if (reservation.Key == null)
                {
                    token = RandomHash.Simple();

                    Reservations.Add(token, (name, id));
                }

                return token;
            }

            public void Remove(string? token)
            {
                Reservations.Remove(token ?? string.Empty);
            }
        }

        public UniqueNamesState(IPersistenceFactory<State> persistenceFactory)
        {
            state = new SimpleState<State>(persistenceFactory, GetType(), "Default");
        }

        public Task LoadAsync(
            CancellationToken ct = default)
        {
            return state.LoadAsync(ct);
        }

        public async Task<string?> ReserveAsync(DomainId id, string name,
            CancellationToken ct = default)
        {
            try
            {
                return await state.UpdateAsync(s => s.Reserve(id, name), ct: ct);
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
                await state.UpdateAsync(s => s.Remove(token), ct: ct);
            }
            catch (InconsistentStateException)
            {
                return;
            }
        }
    }
}
