// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Security.Claims;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Entities
{
    public sealed class QueryContext : Cloneable<QueryContext>
    {
        public ClaimsPrincipal User { get; private set; }

        public IAppEntity App { get; private set; }

        public bool Archived { get; private set; }

        public bool Flatten { get; private set; }

        public IEnumerable<Language> Languages { get; private set; }

        private QueryContext()
        {
        }

        public static QueryContext Create(IAppEntity app, ClaimsPrincipal user)
        {
            return new QueryContext { App = app, User = user };
        }

        public QueryContext WithArchived(bool archived)
        {
            return Clone(c => c.Archived = archived);
        }

        public QueryContext WithFlatten(bool flatten)
        {
            return Clone(c => c.Flatten = flatten);
        }

        public QueryContext WithLanguages(IEnumerable<string> languageCodes)
        {
            if (languageCodes != null)
            {
                return Clone(c =>
                {
                    var languages = new List<Language>();

                    foreach (var iso2Code in languageCodes)
                    {
                        if (Language.TryGetLanguage(iso2Code, out var language))
                        {
                            languages.Add(language);
                        }
                    }

                    c.Languages = languages;
                });
            }

            return this;
        }

        public bool IsFrontendClient
        {
            get { return User.IsInClient("squidex-frontend"); }
        }
    }
}
