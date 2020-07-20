// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
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
