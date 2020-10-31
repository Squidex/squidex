// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities
{
    public sealed class Q : Cloneable<Q>
    {
        public static readonly Q Empty = new Q();

        public IReadOnlyList<DomainId> Ids { get; private set; }

        public DomainId? Reference { get; private set; }

        public string? ODataQuery { get; private set; }

        public string? JsonQuery { get; private set; }

        public Query<IJsonValue>? ParsedJsonQuery { get; private set; }

        public ClrQuery? Query { get; private set; }

        public Q WithQuery(ClrQuery? query)
        {
            return Clone(clone => clone.Query = query);
        }

        public Q WithODataQuery(string? odataQuery)
        {
            return Clone(clone => clone.ODataQuery = odataQuery);
        }

        public Q WithJsonQuery(string? jsonQuery)
        {
            return Clone(clone => clone.JsonQuery = jsonQuery);
        }

        public Q WithJsonQuery(Query<IJsonValue>? jsonQuery)
        {
            return Clone(clone => clone.ParsedJsonQuery = jsonQuery);
        }

        public Q WithIds(params DomainId[] ids)
        {
            return Clone(clone => clone.Ids = ids.ToList());
        }

        public Q WithReference(DomainId? reference)
        {
            return Clone(clone => clone.Reference = reference);
        }

        public Q WithIds(IEnumerable<DomainId> ids)
        {
            return Clone(clone => clone.Ids = ids.ToList());
        }

        public Q WithIds(string? ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                return Clone(clone =>
                {
                    var idsList = new List<DomainId>();

                    foreach (var id in ids.Split(','))
                    {
                        idsList.Add(DomainId.Create(id));
                    }

                    clone.Ids = idsList;
                });
            }

            return this;
        }
    }
}
