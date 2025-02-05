// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Infrastructure.States;

public class EFState<T> : IVersionedEntity<DomainId>
{
    [Key]
    public DomainId DocumentId { get; set; }

    [Json]
    public T Document { get; set; }

    public long Version { get; set; }

    public virtual void Prepare()
    {
    }
}
