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
                    "assigned {user:[Contributor]} as [Permission]"
                },
                {
                    TypeNameRegistry.GetName<AppContributorRemoved>(),
                    "removed {user:[Contributor]} from app"
                },
                {
                    TypeNameRegistry.GetName<AppClientAttached>(),
                    "added client {[Id]} to app"
                },
                {
                    TypeNameRegistry.GetName<AppClientRevoked>(),
                    "revoked client {[Id]}"
                },
                {
                    TypeNameRegistry.GetName<AppClientRenamed>(),
                    "named client {[Id]} as {[Name]}"
                },
                {
                    TypeNameRegistry.GetName<AppLanguageAdded>(),
                    "added language {[Language]}"
                },
                {
                    TypeNameRegistry.GetName<AppLanguageRemoved>(),
                    "removed language {[Language]}"
                },
                {
                    TypeNameRegistry.GetName<AppMasterLanguageSet>(),
                    "changed master language to {[Language]}"
                }
            };
    }
}
