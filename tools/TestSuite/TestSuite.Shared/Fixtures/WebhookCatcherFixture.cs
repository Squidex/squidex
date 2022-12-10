// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using TestSuite.Utils;

namespace TestSuite.Fixtures;

public sealed class WebhookCatcherFixture
{
    public WebhookCatcherClient Client { get; }

    public WebhookCatcherFixture()
    {
        Client = new WebhookCatcherClient(
            TestHelpers.GetAndPrintValue("webhookcatcher:host:api", "localhost"), 1026,
            TestHelpers.GetAndPrintValue("webhookcatcher:host:endpoint", "localhost"), 1026);
    }
}
