// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

#pragma warning disable SA1310 // Field names should not contain underscore

using MongoDB.Driver;

namespace Squidex.Infrastructure;

public static class MongoDbErrorCodes
{
    public static bool IsInvalidGeoData(WriteError error)
    {
        return error.Code == 16755;
    }

    public static bool IsInvalidDocumentDbGeoData(WriteError error)
    {
        return error.Code == 2;
    }
}
