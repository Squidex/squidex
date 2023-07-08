// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Text.Json;

namespace Squidex.Infrastructure.Json.System;

internal static class Constants
{
    public const string DefaultDiscriminatorProperty = "$type";

    public static readonly JsonEncodedText DefaultDiscriminiatorPropertyJson = JsonEncodedText.Encode("$type");
}
