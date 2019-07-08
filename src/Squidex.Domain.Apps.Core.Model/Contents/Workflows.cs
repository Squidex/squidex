// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

namespace Squidex.Domain.Apps.Core.Contents
{
    public sealed class Workflows : ArrayDictionary<Guid, Workflow>
    {
        public static readonly Workflows Empty = new Workflows();

        private Workflows()
        {
        }

        public Workflows(KeyValuePair<Guid, Workflow>[] items)
            : base(items)
        {
        }

        [Pure]
        public Workflows Remove(Guid id)
        {
            return new Workflows(Without(id));
        }

        [Pure]
        public Workflows Add(Guid workflowId, string name)
        {
            Guard.NotNullOrEmpty(name, nameof(name));

            return new Workflows(With(workflowId, Workflow.CreateDefault(name)));
        }

        [Pure]
        public Workflows Set(Workflow workflow)
        {
            Guard.NotNull(workflow, nameof(workflow));

            return new Workflows(With(Guid.Empty, workflow));
        }

        [Pure]
        public Workflows Set(Guid id, Workflow workflow)
        {
            Guard.NotNull(workflow, nameof(workflow));

            return new Workflows(With(id, workflow));
        }

        [Pure]
        public Workflows Update(Guid id, Workflow workflow)
        {
            Guard.NotNull(workflow, nameof(workflow));

            if (id == Guid.Empty)
            {
                return Set(workflow);
            }

            if (!ContainsKey(id))
            {
                return this;
            }

            return new Workflows(With(id, workflow));
        }

        public Workflow GetFirst()
        {
            return Values.FirstOrDefault() ?? Workflow.Default;
        }
    }
}
