// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities
{
    public sealed class Query : Cloneable<Query>
    {
        public static readonly Query Empty = new Query();

        public List<Guid> Ids { get; private set; }

        public string ODataQuery { get; private set; }

        public Query WithODataQuery(string odataQuery)
        {
            return Clone(c => c.ODataQuery = odataQuery);
        }

        public Query WithIds(IEnumerable<Guid> ids)
        {
            return Clone(c => c.Ids = ids.ToList());
        }

        public Query WithIds(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                return Clone(c =>
                {
                    c.Ids = new List<Guid>();

                    foreach (var id in ids.Split(','))
                    {
                        if (Guid.TryParse(id, out var guid))
                        {
                            c.Ids.Add(guid);
                        }
                    }
                });
            }

            return this;
        }
    }
}
