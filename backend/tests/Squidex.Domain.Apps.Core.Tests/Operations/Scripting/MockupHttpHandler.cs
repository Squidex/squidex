// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschraenkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

namespace Squidex.Domain.Apps.Core.Operations.Scripting;

internal sealed class MockupHttpHandler : HttpMessageHandler
{
    private readonly HttpResponseMessage response;
    private HttpRequestMessage currentRequest;
    private string? currentContent;
    private string? currentContentType;

    public void ShouldBeMethod(HttpMethod method)
    {
        Assert.Equal(method, currentRequest.Method);
    }

    public void ShouldBeUrl(string url)
    {
        Assert.Equal(url, currentRequest.RequestUri?.ToString());
    }

    public void ShouldBeHeader(string key, string value)
    {
        Assert.Equal(value, currentRequest.Headers.GetValues(key).FirstOrDefault());
    }

    public void ShouldBeBody(string content, string contentType)
    {
        Assert.Equal(content, currentContent);
        Assert.Equal(contentType, currentContentType);
    }

    public MockupHttpHandler(HttpResponseMessage response)
    {
        this.response = response;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        await Task.Delay(1000, cancellationToken);

        currentRequest = request;

        if (request.Content is StringContent body)
        {
            currentContent = await body.ReadAsStringAsync(cancellationToken);
            currentContentType = body.Headers.ContentType?.MediaType;
        }

        return response;
    }
}
