// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Squidex.Domain.Apps.Core.Contents;
using Squidex.Domain.Apps.Core.ExtractReferenceIds;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Json.Objects;

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
    [InlineData("before content:my_content-42|after")]
    [InlineData("before contents:my_content-42|after")]
    [InlineData("before https://cloud.squidex.io/api/content/my-app/my-schema/my_content-42|after")]
    [InlineData("before https://contents.squidex.io/my-app/my-schema/my_content-42|after")]
    public void Should_extract_content_id(string input)
    {
        var ids = sut.GetEmbeddedContentIds(input);

        Assert.Contains(DomainId.Create("my_content-42"), ids.ToList());
    }

    [Theory]
    [InlineData("content:my_content-42")]
    [InlineData("contents:my_content-42")]
    [InlineData("https://cloud.squidex.io/api/content/my-app/my-schema/my_content-42")]
    [InlineData("https://contents.squidex.io/my-app/my-schema/my_content-42")]
    public void Should_extract_content_id_from_rich_text(string href)
    {
        var source = JsonValue.Object()
            .Add("type", "paragraph")
            .Add("content", JsonValue.Array(
                JsonValue.Object()
                    .Add("type", "text")
                    .Add("marks", JsonValue.Array(
                        JsonValue.Object()
                            .Add("type", "link")
                            .Add("attrs", JsonValue.Object()
                                .Add("href", href))))));

        var ids = sut.GetEmbeddedContentIds(RichTextNode.Create(source));

        Assert.Contains(DomainId.Create("my_content-42"), ids.ToList());
    }

    [Theory]
    [InlineData("before asset:my_asset-42|after")]
    [InlineData("before assets:my_asset-42|after")]
    [InlineData("before https://cloud.squidex.io/api/assets/my_asset-42|after")]
    [InlineData("before https://cloud.squidex.io/api/assets/my-app/my_asset-42|after")]
    [InlineData("before https://assets.squidex.io/my_asset-42|after")]
    [InlineData("before https://assets.squidex.io/my-app/my_asset-42|after")]
    public void Should_extract_asset_id(string input)
    {
        var ids = sut.GetEmbeddedAssetIds(input);

        Assert.Contains(DomainId.Create("my_asset-42"), ids.ToList());
    }

    [Theory]
    [InlineData("asset:my_asset-42")]
    [InlineData("assets:my_asset-42")]
    [InlineData("https://cloud.squidex.io/api/assets/my_asset-42")]
    [InlineData("https://cloud.squidex.io/api/assets/my-app/my_asset-42")]
    [InlineData("https://assets.squidex.io/my_asset-42")]
    [InlineData("https://assets.squidex.io/my-app/my_asset-42")]
    public void Should_extract_asset_id_from_rich_text(string src)
    {
        var source = JsonValue.Object()
            .Add("type", "paragraph")
            .Add("content", JsonValue.Array(
                JsonValue.Object()
                    .Add("type", "image")
                    .Add("attrs", JsonValue.Object()
                        .Add("src", src))));

        var ids = sut.GetEmbeddedAssetIds(RichTextNode.Create(source));

        Assert.Contains(DomainId.Create("my_asset-42"), ids.ToList());
    }
}
