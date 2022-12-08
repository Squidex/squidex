// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using NodaTime;
using Squidex.Domain.Apps.Core.Contents;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Commands;

public sealed class ChangeContentStatus : ContentCommand
{
    public Status Status { get; set; }

    public Instant? DueTime { get; set; }

    public DomainId? StatusJobId { get; set; }

    public bool CheckReferrers { get; set; }

    public bool DoNotValidate { get; set; }

    public bool DoNotValidateWorkflow { get; set; }

    public bool OptimizeValidation { get; set; }
}
