// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;
using Squidex.Infrastructure.Collections;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter

namespace Squidex.Domain.Apps.Entities.Jobs;

public record struct JobRequest(RefToken Actor, string TaskName, ReadonlyDictionary<string, string> Arguments)
{
    public NamedId<DomainId>? AppId { get; set; }

    public static JobRequest Create(RefToken actor, string taskName, Dictionary<string, string>? arguments = null)
    {
        var args = arguments?.ToReadonlyDictionary() ?? ReadonlyDictionary.Empty<string, string>();

        return new JobRequest(actor, taskName, args);
    }
}
