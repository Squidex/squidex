// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Contents.Commands;

public sealed class BulkUpdateContents : SquidexCommand, IAppCommand, ISchemaCommand
{
    public static readonly NamedId<DomainId> NoSchema = NamedId.Of(DomainId.Empty, "none");

    public NamedId<DomainId> AppId { get; set; }

    public NamedId<DomainId> SchemaId { get; set; }

    public bool Publish { get; set; }

    public bool DoNotValidate { get; set; }

    public bool DoNotValidateWorkflow { get; set; }

    public bool DoNotScript { get; set; }

    public bool CheckReferrers { get; set; }

    public bool OptimizeValidation { get; set; }

    public BulkUpdateJob[]? Jobs { get; set; }
}
