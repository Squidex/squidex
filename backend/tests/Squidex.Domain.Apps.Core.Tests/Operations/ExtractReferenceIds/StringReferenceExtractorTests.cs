// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Core.Operations.ExtractReferenceIds;

public class StringReferenceExtractorTests
{
    private readonly StringReferenceExtractor sut;

    public StringReferenceExtractorTests()
    {
        var urlGenerator = A.Fake<IUrlGenerator>();

        A.CallTo(() => urlGenerator.ContentBase())
            .Returns("https://cloud.squidex.io/api/content/");

        A.CallTo(() => urlGenerator.ContentCDNBase())
            .Returns("https://contents.squidex.io/");

        A.CallTo(() => urlGenerator.AssetContentBase())
            .Returns("https://cloud.squidex.io/api/assets/");

        A.CallTo(() => urlGenerator.AssetContentCDNBase())
            .Returns("https://assets.squidex.io/");

        sut = new StringReferenceExtractor(urlGenerator);
    }

    [Theory]
    [InlineData("before content:a_b-123|after")]
    [InlineData("before contents:a_b-123|after")]
    [InlineData("before https://cloud.squidex.io/api/content/my-app/my-schema/a_b-123|after")]
    [InlineData("before https://contents.squidex.io/my-app/my-schema/a_b-123|after")]
    public void Should_extract_content_id(string input)
    {
        var ids = sut.GetEmbeddedContentIds(input);

        Assert.Contains(DomainId.Create("a_b-123"), ids.ToList());
    }

    [Theory]
    [InlineData("before asset:a_b-123|after")]
    [InlineData("before assets:a_b-123|after")]
    [InlineData("before https://cloud.squidex.io/api/assets/a_b-123|after")]
    [InlineData("before https://cloud.squidex.io/api/assets/my-app/a_b-123|after")]
    [InlineData("before https://assets.squidex.io/a_b-123|after")]
    [InlineData("before https://assets.squidex.io/my-app/a_b-123|after")]
    public void Should_extract_asset_id(string input)
    {
        var ids = sut.GetEmbeddedAssetIds(input);

        Assert.Contains(DomainId.Create("a_b-123"), ids.ToList());
    }
}
