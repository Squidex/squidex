// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Orleans.Indexes
{
    public class UniqueNameGrain<T> : GrainOfString, IUniqueNameGrain<T>
    {
        private readonly Dictionary<string, (string Name, T Id)> reservations = new Dictionary<string, (string Name, T Id)>();

        public Task<string?> ReserveAsync(T id, string name)
        {
            string? token = null;

            var reservation = reservations.FirstOrDefault(x => x.Value.Name == name);

            if (Equals(reservation.Value.Id, id))
            {
                token = reservation.Key;
            }
            else if (reservation.Key == null)
            {
                token = RandomHash.Simple();

                reservations.Add(token, (name, id));
            }

            return Task.FromResult(token);
        }

        public Task RemoveReservationAsync(string? token)
        {
            reservations.Remove(token ?? string.Empty);

            return Task.CompletedTask;
        }
    }
}
