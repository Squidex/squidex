// ==========================================================================
//  MessagesEN.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Events.Apps;
using Squidex.Infrastructure;

namespace Squidex.Store.MongoDb.History
{
    public static class MessagesEN
    {
        public static readonly IReadOnlyDictionary<string, string> Texts =
            new Dictionary<string, string>
            {
                {
                    TypeNameRegistry.GetName<AppContributorAssigned>(),
                    "[User] assigned [Contributor] to app with permission [Permission]"
                },
                {
                    TypeNameRegistry.GetName<AppContributorRemoved>(),
                    "[User] removed [Contributor] from app"
                }
            };
    }
}
