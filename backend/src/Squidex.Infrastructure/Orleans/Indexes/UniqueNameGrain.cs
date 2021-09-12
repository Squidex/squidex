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
    public class UniqueNameGrain : GrainOfString
    {
        private readonly Dictionary<string, (string Name, string Id)> reservations = new Dictionary<string, (string Name, string Id)>();

        public Task<string?> ReserveAsync(string id, string name)
        {
            string? token = null;

            var reservation = reservations.FirstOrDefault(x => x.Value.Name == name);

            if (reservation.Value.Id == id)
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
