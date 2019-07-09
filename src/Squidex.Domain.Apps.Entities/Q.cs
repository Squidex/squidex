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
    public sealed class Q : Cloneable<Q>
    {
        public static readonly Q Empty = new Q();

        public IReadOnlyList<Guid> Ids { get; private set; }

        public string ODataQuery { get; private set; }

        public Q WithODataQuery(string odataQuery)
        {
            return Clone(c => c.ODataQuery = odataQuery);
        }

        public Q WithIds(params Guid[] ids)
        {
            return Clone(c => c.Ids = ids.ToList());
        }

        public Q WithIds(IEnumerable<Guid> ids)
        {
            return Clone(c => c.Ids = ids.ToList());
        }

        public Q WithIds(string ids)
        {
            if (!string.IsNullOrEmpty(ids))
            {
                return Clone(c =>
                {
                    var idsList = new List<Guid>();

                    foreach (var id in ids.Split(','))
                    {
                        if (Guid.TryParse(id, out var guid))
                        {
                            idsList.Add(guid);
                        }
                    }

                    c.Ids = idsList;
                });
            }

            return this;
        }
    }
}
