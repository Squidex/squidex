// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Squidex.Infrastructure.Orleans.Caching
{
    public class UniqueNameGrain : GrainOfString
    {
        private readonly Dictionary<string, (string Name, string Id)> reservations = new Dictionary<string, (string Name, string Id)>();

        public Task<string?> ReserveAsync(string id, string name)
        {
            string? token = null;

            if (!IsReserved(name))
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

        private bool IsReserved(string name)
        {
            return reservations.Values.Any(x => x.Name == name);
        }
    }
}
