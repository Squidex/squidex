﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Security.Claims;
using Squidex.Domain.Apps.Entities.Apps;
using Squidex.Domain.Apps.Entities.Contents;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Security;

namespace Squidex.Domain.Apps.Entities
{
    public sealed class QueryContext : Cloneable<QueryContext>
    {
        private static readonly char[] Separators = { ',', ';' };

        public ClaimsPrincipal User { get; private set; }

        public IAppEntity App { get; private set; }

        public bool Flatten { get; set; }

        public StatusForApi ApiStatus { get; private set; }

        public StatusForFrontend FrontendStatus { get; private set; }

        public IReadOnlyCollection<string> AssetUrlsToResolve { get; private set; }

        public IReadOnlyCollection<Language> Languages { get; private set; }

        private QueryContext()
        {
        }

        public static QueryContext Create(IAppEntity app, ClaimsPrincipal user, string clientId)
        {
            return new QueryContext { App = app, User = user };
        }

        public QueryContext WithFlatten(bool flatten)
        {
            return Clone(c => c.Flatten = flatten);
        }

        public QueryContext WithUnpublished(bool unpublished)
        {
            return WithApiStatus(unpublished ? StatusForApi.PublishedDraft : StatusForApi.PublishedOnly);
        }

        public QueryContext WithApiStatus(StatusForApi status)
        {
            return Clone(c => c.ApiStatus = status);
        }

        public QueryContext WithFrontendStatus(StatusForFrontend status)
        {
            return Clone(c => c.FrontendStatus = status);
        }

        public QueryContext WithFrontendStatus(string status)
        {
            if (status != null && Enum.TryParse<StatusForFrontend>(status, out var result))
            {
                return WithFrontendStatus(result);
            }

            return this;
        }

        public QueryContext WithAssetUrlsToResolve(IEnumerable<string> fieldNames)
        {
            if (fieldNames != null)
            {
                return Clone(c =>
                {
                    var fields = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                    c.AssetUrlsToResolve?.Foreach(x => fields.Add(x));

                    foreach (var part in fieldNames)
                    {
                        foreach (var fieldName in part.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
                        {
                            fields.Add(fieldName.Trim());
                        }
                    }

                    c.AssetUrlsToResolve = fields;
                });
            }

            return this;
        }

        public QueryContext WithLanguages(IEnumerable<string> languageCodes)
        {
            if (languageCodes != null)
            {
                return Clone(c =>
                {
                    var languages = new HashSet<Language>();

                    c.Languages?.Foreach(x => languages.Add(x));

                    foreach (var part in languageCodes)
                    {
                        foreach (var iso2Code in part.Split(Separators, StringSplitOptions.RemoveEmptyEntries))
                        {
                            if (Language.TryGetLanguage(iso2Code.Trim(), out var language))
                            {
                                languages.Add(language);
                            }
                        }
                    }

                    c.Languages = languages;
                });
            }

            return this;
        }

        public bool IsFrontendClient
        {
            get { return User.IsInClient("vega.cms"); }
        }
    }
}
