// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Entities;

public interface IContextProvider
{
    // The context is immutable, so it has to be replaced to change it for the current request.
    Context Context { get; set; }
}
