// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using FakeItEasy;
using Xunit;

namespace Squidex.Domain.Apps.Entities.Apps.Templates
{
    public class TemplatesClientTests
    {
        private readonly TemplatesClient sut;

        public TemplatesClientTests()
        {
            var httpClientFactory = A.Fake<IHttpClientFactory>();

            A.CallTo(() => httpClientFactory.CreateClient(null))
                .Returns(new HttpClient());

            sut = new TemplatesClient(httpClientFactory);
        }

        [Fact]
        public async Task Should_get_templates()
        {
            var templates = await sut.GetTemplatesAsync();

            Assert.NotEmpty(templates);
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
        public async Task Should_return_null_details_if_not_found()
        {
            var details = await sut.GetDetailAsync("invalid");

            Assert.Null(details);
        }
    }
}
