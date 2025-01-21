// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.ComponentModel.DataAnnotations;

namespace Squidex.Infrastructure.States;

public class EFState<T>
{
    [Key]
    public string DocumentId { get; set; }

    public T Document { get; set; }

    public long Version { get; set; }

    public virtual void Prepare()
    {
    }
}
