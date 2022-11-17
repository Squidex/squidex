// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Extensions.Options;

namespace Squidex.Domain.Apps.Entities.Apps.Templates;

public class TemplatesClientTests
{
    private readonly TemplatesClient sut;

    private sealed class CustomHttpClient : HttpClient
    {
        protected override void Dispose(bool disposing)
        {
        }
    }

    public TemplatesClientTests()
    {
        var httpClientFactory = A.Fake<IHttpClientFactory>();

        A.CallTo(() => httpClientFactory.CreateClient(A<string>._))
            .Returns(new CustomHttpClient());

        sut = new TemplatesClient(httpClientFactory, Options.Create(new TemplatesOptions
        {
            Repositories = new[]
            {
                new TemplateRepository
                {
                    ContentUrl = "https://raw.githubusercontent.com/Squidex/templates/main"
                }
            }
        }));
    }

    [Fact]
    public async Task Should_get_templates()
    {
        var templates = await sut.GetTemplatesAsync();

        Assert.NotEmpty(templates);
        Assert.Contains(templates, x => x.IsStarter);
    }

    [Fact]
    public async Task Should_get_details_from_templates()
    {
        var templates = await sut.GetTemplatesAsync();

        foreach (var template in templates)
        {
            var details = await sut.GetDetailAsync(template.Name);

            Assert.NotNull(details);
        }
    }

    [Fact]
    public async Task Should_get_repository_from_templates()
    {
        var templates = await sut.GetTemplatesAsync();

        foreach (var template in templates)
        {
            var repository = await sut.GetRepositoryUrl(template.Name);

            Assert.NotNull(repository);
        }
    }

    [Fact]
    public async Task Should_return_null_details_if_not_found()
    {
        var details = await sut.GetDetailAsync("invalid");

        Assert.Null(details);
    }
}
