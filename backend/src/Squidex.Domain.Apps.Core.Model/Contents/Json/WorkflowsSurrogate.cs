// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Collections.Generic;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Contents.Json
{
    public sealed class WorkflowsSurrogate : Dictionary<DomainId, Workflow>, ISurrogate<Workflows>
    {
        public void FromSource(Workflows source)
        {
            foreach (var (key, workflow) in source)
            {
                Add(key, workflow);
            }
        }

        public Workflows ToSource()
        {
            return new Workflows(this);
        }
    }
}
