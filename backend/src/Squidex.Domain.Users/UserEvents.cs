// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Shared.Users;

namespace Squidex.Domain.Users
{
    public sealed class UserEvents : IUserEvents
    {
        private readonly IEnumerable<IUserEventHandler> userEventHandlers;

        public UserEvents(IEnumerable<IUserEventHandler> userEventHandlers)
        {
            Guard.NotNull(userEventHandlers, nameof(userEventHandlers));

            this.userEventHandlers = userEventHandlers;
        }

        public async Task<IEnumerable<Claim>> OnUserRegisteringAsync(IUser user)
        {
            var claims = new List<Claim>();

            foreach (var handler in userEventHandlers)
            {
                var handlerClaims = await handler.OnUserRegisteringAsync(user);

                claims.AddRange(handlerClaims);
            }

            return claims;
        }

        public void OnUserRegistered(IUser user)
        {
            foreach (var handler in userEventHandlers)
            {
                handler.OnUserRegistered(user);
            }
        }

        public void OnUserUpdated(IUser user)
        {
            foreach (var handler in userEventHandlers)
            {
                handler.OnUserUpdated(user);
            }
        }

        public void OnConsentGiven(IUser user)
        {
            foreach (var handler in userEventHandlers)
            {
                handler.OnConsentGiven(user);
            }
        }
    }
}
