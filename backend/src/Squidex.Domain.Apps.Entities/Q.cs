// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using System.Linq;
using NodaTime;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;
using Squidex.Infrastructure.Queries;

namespace Squidex.Domain.Apps.Entities
{
    public record Q
    {
        public static Q Empty => new Q();

        public ClrQuery Query { get; init; } = new ClrQuery();

        public IReadOnlyList<DomainId>? Ids { get; init; }

        public DomainId Referencing { get; init; }

        public DomainId Reference { get; init; }

        public string? QueryAsOdata { get; init; }

        public string? QueryAsJson { get; init; }

        public Instant? ScheduledFrom { get; init; }

        public Instant? ScheduledTo { get; init; }

        public Query<IJsonValue>? JsonQuery { get; init; }

        public RefToken? CreatedBy { get; init; }

        public bool NoTotal { get; init; }

        private Q()
        {
        }

        public Q WithQuery(ClrQuery query)
        {
            Guard.NotNull(query, nameof(query));

            return this with { Query = query };
        }

        public Q WithoutTotal(bool value = true)
        {
            return this with { NoTotal = value };
        }

        public Q WithODataQuery(string? query)
        {
            return this with { QueryAsOdata = query };
        }

        public Q WithJsonQuery(string? query)
        {
            return this with { QueryAsJson = query };
        }

        public Q WithJsonQuery(Query<IJsonValue>? query)
        {
            return this with { JsonQuery = query };
        }

        public Q WithReferencing(DomainId id)
        {
            return this with { Referencing = id };
        }

        public Q WithReference(DomainId id)
        {
            return this with { Reference = id };
        }

        public Q WithIds(params DomainId[] ids)
        {
            return this with { Ids = ids?.ToList() };
        }

        public Q WithIds(IEnumerable<DomainId> ids)
        {
            return this with { Ids = ids?.ToList() };
        }

        public Q WithSchedule(Instant from, Instant to)
        {
            return this with { ScheduledFrom = from, ScheduledTo = to };
        }

        public Q WithIds(string? ids)
        {
            if (string.IsNullOrWhiteSpace(ids))
            {
                return this with { Ids = null };
            }

            var idsList = new List<DomainId>();

            if (!string.IsNullOrEmpty(ids))
            {
                foreach (var id in ids.Split(','))
                {
                    idsList.Add(DomainId.Create(id));
                }
            }

            return this with { Ids = idsList };
        }
    }
}
