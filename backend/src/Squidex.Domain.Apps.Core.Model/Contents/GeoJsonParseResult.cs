// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Contents;

public enum GeoJsonParseResult
{
    Success,
    InvalidLatitude,
    InvalidLongitude,
    InvalidValue
}
