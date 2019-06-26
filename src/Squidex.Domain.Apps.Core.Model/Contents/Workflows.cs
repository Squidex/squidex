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
        public Workflows Set(Workflow workflow)
        {
            Guard.NotNull(workflow, nameof(workflow));

            return new Workflows(With(Guid.Empty, workflow));
        }

        public Workflow GetFirst()
        {
            return Values.FirstOrDefault() ?? Workflow.Default;
        }
    }
}
