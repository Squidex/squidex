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
using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;
using Squidex.Infrastructure.Reflection.Equality;

namespace Squidex.Domain.Apps.Core.Contents
{
    public sealed class Workflows : ImmutableDictionary<Guid, Workflow>
    {
        public static readonly Workflows Empty = new Workflows();

        private Workflows()
        {
        }

        public Workflows(Dictionary<Guid, Workflow> inner)
            : base(inner)
        {
        }

        [Pure]
        public Workflows Remove(Guid id)
        {
            return Without<Workflows>(id);
        }

        [Pure]
        public Workflows Add(Guid workflowId, string name)
        {
            Guard.NotNullOrEmpty(name);

            return With<Workflows>(workflowId, Workflow.CreateDefault(name));
        }

        [Pure]
        public Workflows Set(Workflow workflow)
        {
            Guard.NotNull(workflow);

            return With<Workflows>(Guid.Empty, workflow, DeepEqualityComparer<Workflow>.Default);
        }

        [Pure]
        public Workflows Set(Guid id, Workflow workflow)
        {
            Guard.NotNull(workflow);

            return With<Workflows>(id, workflow, DeepEqualityComparer<Workflow>.Default);
        }

        [Pure]
        public Workflows Update(Guid id, Workflow workflow)
        {
            Guard.NotNull(workflow);

            if (id == Guid.Empty)
            {
                return Set(workflow);
            }

            if (!ContainsKey(id))
            {
                return this;
            }

            return With<Workflows>(id, workflow, DeepEqualityComparer<Workflow>.Default);
        }

        public Workflow GetFirst()
        {
            return Values.FirstOrDefault() ?? Workflow.Default;
        }
    }
}
